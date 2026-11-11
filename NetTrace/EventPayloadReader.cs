using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;
using NetTrace.FastSerializer;

namespace NetTrace;

/// <summary>Decodes normalized wire layouts, including arrays located elsewhere in the payload.</summary>
internal sealed class EventPayloadReader
{
    private static readonly object TrueBoolean = true;
    private static readonly object FalseBoolean = false;
    private static readonly IReadOnlyDictionary<string, object> EmptyPayload = new Dictionary<string, object>();

    private readonly Dictionary<byte, object> _internedByte = [];
    private readonly Dictionary<sbyte, object> _internedSByte = [];
    private readonly Dictionary<short, object> _internedInt16 = [];
    private readonly Dictionary<ushort, object> _internedUInt16 = [];
    private readonly Dictionary<IReadOnlyList<EventFieldDefinition>, EventPayload.Layout> _layouts =
        new(ReferenceEqualityComparer.Instance);

    public IReadOnlyDictionary<string, object> Read(ref FastSerializerSequenceReader reader,
        IReadOnlyList<EventFieldDefinition> fields)
    {
        var buffer = reader.ReadRemainingBytes();
        FastSerializerSequenceReader payloadReader = new(buffer, 0);
        return ReadObject(ref payloadReader, fields, buffer);
    }

    // The caller supplies the full payload so nested data locations use the same origin.
    // Keeping the cursor visible also lets schema selection require complete consumption.
    public IReadOnlyDictionary<string, object> ReadObject(ref FastSerializerSequenceReader reader,
        IReadOnlyList<EventFieldDefinition> fields, ReadOnlySequence<byte> payloadBuffer)
    {
        if (fields.Count == 0) return EmptyPayload;
        if (!_layouts.TryGetValue(fields, out var layout))
        {
            layout = new EventPayload.Layout(fields);
            _layouts.Add(fields, layout);
        }
        object[] values = new object[layout.Names.Length];
        EventPayload payload = new(layout, values);
        for (int i = 0; i < fields.Count; i++)
        {
            var field = fields[i];
            if (field.TypeCode == NetTraceTypeCode.Padding32)
            {
                reader.ReadUInt32();
                continue;
            }
            if (field.CountFieldName != null)
            {
                int count = checked(Convert.ToInt32(payload[field.CountFieldName]));
                if (count < 0 || count > reader.Remaining) throw new InvalidDataException("Invalid array element count");
                values[layout.ValueIndexes[i]] = ReadArrayElements(ref reader, GetArrayElement(field), count, payloadBuffer);
            }
            else
            {
                values[layout.ValueIndexes[i]] = ReadFieldValue(ref reader, field, payloadBuffer);
            }
        }
        return payload;
    }

    private object ReadFieldValue(ref FastSerializerSequenceReader reader,
        EventFieldDefinition field, ReadOnlySequence<byte> payloadBuffer)
    {
        return field.TypeCode switch
        {
            NetTraceTypeCode.Object => ReadObject(ref reader, field.SubFieldDefinitions!, payloadBuffer),
            NetTraceTypeCode.Boolean32 => reader.ReadInt32() != 0 ? TrueBoolean : FalseBoolean,
            NetTraceTypeCode.Boolean8 => reader.ReadByte() != 0 ? TrueBoolean : FalseBoolean,
            NetTraceTypeCode.Utf16CodeUnit => (char)reader.ReadUInt16(),
            NetTraceTypeCode.SByte => Intern((sbyte)reader.ReadByte(), _internedSByte),
            NetTraceTypeCode.Byte => Intern(reader.ReadByte(), _internedByte),
            NetTraceTypeCode.Int16 => Intern(reader.ReadInt16(), _internedInt16),
            NetTraceTypeCode.UInt16 => Intern(reader.ReadUInt16(), _internedUInt16),
            NetTraceTypeCode.Int32 => reader.ReadInt32(),
            NetTraceTypeCode.UInt32 => reader.ReadUInt32(),
            NetTraceTypeCode.Int64 => reader.ReadInt64(),
            NetTraceTypeCode.UInt64 => reader.ReadUInt64(),
            NetTraceTypeCode.Single => reader.ReadSingle(),
            NetTraceTypeCode.Double => reader.ReadDouble(),
            NetTraceTypeCode.Decimal => new decimal([reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32()]),
            NetTraceTypeCode.DateTime => ReadDateTime(ref reader),
            NetTraceTypeCode.Guid => reader.ReadGuid(),
            NetTraceTypeCode.NullTerminatedUtf16String => reader.ReadNullTerminatedUtf16String(),
            NetTraceTypeCode.VarInt => reader.ReadZigZagInt64(),
            NetTraceTypeCode.VarUInt => reader.ReadVarUInt64(),
            NetTraceTypeCode.Utf8String => reader.ReadShortUtf8String(),
            NetTraceTypeCode.Array or NetTraceTypeCode.FixedLengthArray or NetTraceTypeCode.RelLoc or NetTraceTypeCode.DataLoc
                => ReadArray(ref reader, field, payloadBuffer),
            _ => throw new NotSupportedException($"Type {field.TypeCode} is not supported")
        };
    }

    private static DateTime ReadDateTime(ref FastSerializerSequenceReader reader)
    {
        int year = reader.ReadUInt16();
        int month = reader.ReadUInt16();
        reader.ReadUInt16(); // day of week
        int day = reader.ReadUInt16();
        int hour = reader.ReadUInt16();
        int minute = reader.ReadUInt16();
        int second = reader.ReadUInt16();
        int millisecond = reader.ReadUInt16();
        return new DateTime(year, month, day, hour, minute, second, millisecond);
    }

    private object ReadArray(ref FastSerializerSequenceReader reader, EventFieldDefinition field,
        ReadOnlySequence<byte> payloadBuffer)
    {
        var element = GetArrayElement(field);
        if (field.TypeCode is NetTraceTypeCode.RelLoc or NetTraceTypeCode.DataLoc)
        {
            uint location = reader.ReadUInt32();
            long offset = (location & 0xFFFF) + (field.TypeCode == NetTraceTypeCode.RelLoc ? reader.AbsolutePosition : 0);
            int size = (int)(location >> 16);
            int elementSize = GetFixedSize(element);
            if (elementSize == 0 || size % elementSize != 0) throw new InvalidDataException("Invalid location array size");
            var buffer = payloadBuffer.Slice(offset, size);
            FastSerializerSequenceReader arrayReader = new(buffer, offset);
            return ReadArrayElements(ref arrayReader, element, size / elementSize, payloadBuffer);
        }
        int count = field.TypeCode == NetTraceTypeCode.Array ? reader.ReadUInt16() : field.ElementCount!.Value;
        return ReadArrayElements(ref reader, element, count, payloadBuffer);
    }

    private object ReadArrayElements(ref FastSerializerSequenceReader reader, EventFieldDefinition element,
        int count, ReadOnlySequence<byte> payloadBuffer)
    {
        if (element.TypeCode == NetTraceTypeCode.Utf8String)
        {
            return Encoding.UTF8.GetString(reader.ReadBytes(count)).TrimEnd('\0');
        }
        if (element.TypeCode == NetTraceTypeCode.Utf16CodeUnit)
        {
            return Encoding.Unicode.GetString(reader.ReadBytes(checked(count * 2))).TrimEnd('\0');
        }
        object[] values = new object[count];
        for (int i = 0; i < count; i += 1) values[i] = ReadFieldValue(ref reader, element, payloadBuffer);
        return values;
    }

    private static EventFieldDefinition GetArrayElement(EventFieldDefinition field)
    {
        return field.ElementDefinition ?? new EventFieldDefinition("", field.ArrayTypeCode!.Value,
            SubFieldDefinitions: field.SubFieldDefinitions);
    }

    private static int GetFixedSize(EventFieldDefinition field)
    {
        return field.TypeCode switch
        {
            NetTraceTypeCode.Boolean8 or NetTraceTypeCode.Byte or NetTraceTypeCode.SByte or NetTraceTypeCode.Utf8String => 1,
            NetTraceTypeCode.Int16 or NetTraceTypeCode.UInt16 or NetTraceTypeCode.Utf16CodeUnit => 2,
            NetTraceTypeCode.Int32 or NetTraceTypeCode.UInt32 or NetTraceTypeCode.Single or NetTraceTypeCode.Boolean32 or NetTraceTypeCode.Padding32 => 4,
            NetTraceTypeCode.Int64 or NetTraceTypeCode.UInt64 or NetTraceTypeCode.Double => 8,
            NetTraceTypeCode.Guid or NetTraceTypeCode.DateTime or NetTraceTypeCode.Decimal => 16,
            NetTraceTypeCode.Object => field.SubFieldDefinitions!.Sum(GetFixedSize),
            NetTraceTypeCode.FixedLengthArray => checked(field.ElementCount!.Value * GetFixedSize(GetArrayElement(field))),
            _ => throw new InvalidDataException("Location arrays require fixed-size elements")
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static object Intern<T>(T value, Dictionary<T, object> dictionary) where T : notnull
    {
        if (!dictionary.TryGetValue(value, out object? boxed))
        {
            boxed = value;
            dictionary[value] = boxed;
        }
        return boxed;
    }
}
