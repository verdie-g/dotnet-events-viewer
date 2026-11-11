using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using NetTrace.FastSerializer;

namespace NetTrace;

/// <summary>Resolves schema-less CLR layouts before handing normalized fields to the wire decoder.</summary>
internal sealed class RuntimePayloadReader(EventPayloadReader payloadReader)
{
    private readonly Dictionary<(int Id, int Version, int PointerSize), Candidate[]> _candidates = [];
    private readonly Dictionary<(IReadOnlyList<EventFieldDefinition> Fields, int PointerSize), EventFieldDefinition[]>
        _wireFields = [];

    internal sealed record Candidate(string Name, EventFieldDefinition[] Fields, long MinimumSize, long? FixedSize)
    {
        public bool AcceptsSize(long size) => FixedSize is { } fixedSize ? size == fixedSize : size >= MinimumSize;
    }

    public bool TryRead(ReadOnlySequence<byte> buffer, int eventId, int declaredVersion, int pointerSize,
        [NotNullWhen(true)] out Candidate? schema,
        [NotNullWhen(true)] out IReadOnlyDictionary<string, object>? payload)
    {
        foreach (var candidate in GetCandidates(eventId, declaredVersion, pointerSize))
        {
            // Most CLR layouts have a fixed size. Reject mismatches without allocating a
            // payload or throwing on a short read. Variable layouts only have a lower bound.
            if (!candidate.AcceptsSize(buffer.Length)) continue;
            FastSerializerSequenceReader reader = new(buffer, 0);
            try
            {
                var decoded = payloadReader.ReadObject(ref reader, candidate.Fields, buffer);
                // A length match is not enough to identify strings/count-driven arrays.
                // Preserve the original trial order and require complete consumption.
                if (!reader.End) continue;
                schema = candidate;
                payload = decoded;
                return true;
            }
            catch (Exception e) when (e is EndOfStreamException or ArgumentException or KeyNotFoundException
                or OverflowException or InvalidDataException)
            {
            }
        }
        schema = null;
        payload = null;
        return false;
    }

    internal Candidate[] GetCandidates(int eventId, int declaredVersion, int pointerSize)
    {
        var key = (eventId, declaredVersion, pointerSize);
        if (_candidates.TryGetValue(key, out var cached)) return cached;
        var schemas = RuntimeEventSchema.Candidates(eventId);
        var candidates = new Candidate[schemas.Count];
        int index = 0;
        // Exact declared version first, then descending version, as in the original fallback.
        foreach (var schema in schemas)
            if (schema.Version == declaredVersion) candidates[index++] = Compile(schema);
        foreach (var schema in schemas)
            if (schema.Version != declaredVersion) candidates[index++] = Compile(schema);
        _candidates[key] = candidates;
        return candidates;

        Candidate Compile(RuntimeEventSchema.Schema schema)
        {
            var fields = NormalizeFields(schema.Fields, pointerSize);
            var size = GetSize(fields);
            return new Candidate(schema.Name, fields, size.Minimum, size.Fixed);
        }
    }

    // Arrays are shared across declared-version candidate lists, so successful public
    // metadata uses stable field identity and hits NetTraceReader's metadata cache.
    private EventFieldDefinition[] NormalizeFields(IReadOnlyList<EventFieldDefinition> fields, int pointerSize)
    {
        if (_wireFields.TryGetValue((fields, pointerSize), out var cached)) return cached;
        List<EventFieldDefinition> normalized = new(fields.Count);
        foreach (var field in fields)
        {
            if (field.AlignPointers && pointerSize == 8)
                normalized.Add(new EventFieldDefinition("", NetTraceTypeCode.Padding32));
            normalized.Add(Normalize(field));
        }
        var result = normalized.ToArray();
        _wireFields[(fields, pointerSize)] = result;
        return result;

        EventFieldDefinition Normalize(EventFieldDefinition field) => field with
        {
            TypeCode = WireType(field.TypeCode),
            ArrayTypeCode = field.ArrayTypeCode is { } elementType ? WireType(elementType) : null,
            SubFieldDefinitions = field.SubFieldDefinitions is { } subFields ? NormalizeFields(subFields, pointerSize) : null,
            ElementDefinition = field.ElementDefinition is { } element ? Normalize(element) : null,
            AlignPointers = false
        };
        NetTraceTypeCode WireType(NetTraceTypeCode type) => type == NetTraceTypeCode.Pointer
            ? (pointerSize == 4 ? NetTraceTypeCode.UInt32 : NetTraceTypeCode.UInt64)
            : type;
    }

    private static (long Minimum, long? Fixed) GetSize(IReadOnlyList<EventFieldDefinition> fields)
    {
        long minimum = 0;
        long? fixedSize = 0;
        foreach (var field in fields)
        {
            var size = GetSize(field);
            minimum = checked(minimum + size.Minimum);
            fixedSize += size.Fixed;
        }
        return (minimum, fixedSize);
    }

    private static (long Minimum, long? Fixed) GetSize(EventFieldDefinition field)
    {
        // An externally counted array has no prefix and may contain zero elements.
        if (field.CountFieldName != null) return (0, null);
        return field.TypeCode switch
        {
            NetTraceTypeCode.Boolean8 or NetTraceTypeCode.Byte or NetTraceTypeCode.SByte => (1, 1),
            NetTraceTypeCode.Int16 or NetTraceTypeCode.UInt16 or NetTraceTypeCode.Utf16CodeUnit => (2, 2),
            NetTraceTypeCode.Int32 or NetTraceTypeCode.UInt32 or NetTraceTypeCode.Single
                or NetTraceTypeCode.Boolean32 or NetTraceTypeCode.Padding32 => (4, 4),
            NetTraceTypeCode.Int64 or NetTraceTypeCode.UInt64 or NetTraceTypeCode.Double => (8, 8),
            NetTraceTypeCode.Guid or NetTraceTypeCode.DateTime or NetTraceTypeCode.Decimal => (16, 16),
            NetTraceTypeCode.Object => GetSize(field.SubFieldDefinitions!),
            NetTraceTypeCode.NullTerminatedUtf16String or NetTraceTypeCode.Utf8String
                or NetTraceTypeCode.Array => (2, null),
            NetTraceTypeCode.VarInt or NetTraceTypeCode.VarUInt => (1, null),
            NetTraceTypeCode.RelLoc or NetTraceTypeCode.DataLoc => (4, null),
            // Conservative for layouts not present in the CLR manifest: never reject a
            // payload based on a size we cannot prove. The decoder remains authoritative.
            _ => (0, null)
        };
    }
}
