using System.Buffers;
using System.Collections.Frozen;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Tracing;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using EventPipe.FastSerializer;

// ReSharper disable UnusedVariable

namespace EventPipe;

public class EventPipeReader(Stream stream)
{
    private const int ReaderVersion = 4;
    private const string RuntimeProvider = "Microsoft-Windows-DotNETRuntime";
    private const string RundownProvider = "Microsoft-Windows-DotNETRuntimeRundown";

    private static readonly FrozenDictionary<MetadataKey, EventMetadata> KnownEventMetadata = new Dictionary<MetadataKey, EventMetadata>
    {
        [new MetadataKey(RuntimeProvider, 10, 4)] =
            new(default, string.Empty, default, "GCAllocationTick", default, default, default,
                null, new EventFieldDefinition[]
                {
                    new("AllocationAmount", TypeCode.UInt32),
                    new("AllocationKind", TypeCode.UInt32),
                    new("ClrInstanceID", TypeCode.UInt16),
                    new("AllocationAmount64", TypeCode.UInt64),
                    new("TypeID", TypeCode.UInt64),
                    new("TypeName", TypeCode.String),
                    new("HeapIndex", TypeCode.UInt32),
                    new("Address", TypeCode.UInt64),
                    new("ObjectSize", TypeCode.UInt64),
                }),
        [new MetadataKey(RundownProvider, 144, 1)] =
            new(default, string.Empty, default, "MethodLoadUnloadVerbose", default, default, default,
                null, new EventFieldDefinition[]
                {
                    new("MethodID", TypeCode.UInt64),
                    new("ModuleID", TypeCode.UInt64),
                    new("MethodStartAddress", TypeCode.UInt64),
                    new("MethodSize", TypeCode.UInt32),
                    new("MethodToken", TypeCode.UInt32),
                    new("MethodFlags", TypeCode.UInt32),
                    new("MethodNamespace", TypeCode.String),
                    new("MethodName", TypeCode.String),
                    new("MethodSignature", TypeCode.String),
                    new("ClrInstanceID", TypeCode.UInt16),
                }),
        [new MetadataKey(RundownProvider, 144, 2)] =
            new(default, string.Empty, default, "MethodLoadUnloadVerbose", default, default, default,
                null, new EventFieldDefinition[]
                {
                    new("MethodID", TypeCode.UInt64),
                    new("ModuleID", TypeCode.UInt64),
                    new("MethodStartAddress", TypeCode.UInt64),
                    new("MethodSize", TypeCode.UInt32),
                    new("MethodToken", TypeCode.UInt32),
                    new("MethodFlags", TypeCode.UInt32),
                    new("MethodNamespace", TypeCode.String),
                    new("MethodName", TypeCode.String),
                    new("MethodSignature", TypeCode.String),
                    new("ReJITID", TypeCode.UInt64),
                    new("ClrInstanceID", TypeCode.UInt16),
                }),
    }.ToFrozenDictionary();

    private static ReadOnlySpan<byte> MagicBytes => "Nettrace"u8;
    private static ReadOnlySpan<byte> SerializerSignature => "!FastSerialization.1"u8;
    private static readonly object TrueBoolean = true;
    private static readonly object FalseBoolean = false;

    private readonly HashSet<string> _internedStrings = [];
    private readonly Dictionary<byte, object> _internedByte = [];
    private readonly Dictionary<sbyte, object> _internedSByte = [];
    private readonly Dictionary<short, object> _internedInt16 = [];
    private readonly Dictionary<ushort, object> _internedUInt16 = [];

    private readonly Dictionary<int, EventMetadata> _eventMetadata = [];
    private readonly List<Event> _events = [];
    private readonly StackResolver _stackResolver = new();

    private IProgress<Progression>? _progress;

    private bool _headerRead;
    private long _readPosition;
    private TraceMetadata? _traceMetadata;

    public EventPipeReader AddProgress(IProgress<Progression> progress)
    {
        _progress = progress;
        return this;
    }

    public async Task<Trace> ReadFullTraceAsync()
    {
        Pipe pipe = new();

        _ = Task.Run(() => FillPipeWithStreamAsync(pipe.Writer, CancellationToken.None));

        var reader = pipe.Reader;
        while (true)
        {
            var result = await reader.ReadAsync();
            var buffer = result.Buffer;

            SequencePosition consumed = HandleBuffer(in buffer);

            reader.AdvanceTo(consumed, buffer.End);

            if (result.IsCompleted)
            {
                break;
            }
        }

        await reader.CompleteAsync();

        // Sort the events because correlation algorithms expect a Stop event to appear after a Start.
        _events.Sort((x, y) => x.TimeStamp.CompareTo(y.TimeStamp));

        var stackTraces = _stackResolver.ResolveAllStackTraces();
        foreach (var evt in _events)
        {
            if (stackTraces.TryGetValue(evt.StackId, out var stackTrace))
            {
                evt.StackTrace = stackTrace;
            }
        }

        return new Trace(
            _traceMetadata!,
            _eventMetadata.Values.ToArray(),
            _events,
            stackTraces.Values.ToArray());
    }

    private SequencePosition HandleBuffer(in ReadOnlySequence<byte> buffer)
    {
        long oldReadPosition = _readPosition;
        FastSerializerSequenceReader reader = new(buffer, _readPosition);
        SequencePosition consumed = reader.Position;

        if (!_headerRead)
        {
            if (!TryReadHeader(ref reader))
            {
                return consumed;
            }

            _headerRead = true;
            consumed = reader.Position;
            _readPosition = oldReadPosition + reader.Consumed;
        }

        while (!reader.End && TryReadObject(ref reader))
        {
            consumed = reader.Position;
            _readPosition = oldReadPosition + reader.Consumed;

            _progress?.Report(new Progression(_readPosition, _events.Count));
        }

        return consumed;
    }

    private static bool TryReadHeader(ref FastSerializerSequenceReader reader)
    {
        if (!reader.TryReadBytes(MagicBytes.Length, out var magicBytesSeq))
        {
            return false;
        }

        Span<byte> magicBytes = stackalloc byte[MagicBytes.Length];
        magicBytesSeq.CopyTo(magicBytes);

        if (!magicBytes.SequenceEqual(MagicBytes))
        {
            throw new Exception("Stream is not in the event pipe format");
        }

        if (!reader.TryReadString(out var serializerSignatureSeq))
        {
            return false;
        }

        Span<byte> serializerSignature = stackalloc byte[SerializerSignature.Length];
        serializerSignatureSeq.CopyTo(serializerSignature);

        if (!serializerSignature.SequenceEqual(SerializerSignature))
        {
            throw new Exception("Unexpected serializer signature");
        }

        return true;
    }

    private bool TryReadObject(ref FastSerializerSequenceReader reader)
    {
        if (!TryReadByteAsEnum(ref reader, out FastSerializerTag tag))
        {
            return false;
        }

        // Last object is just a null ref tag.
        if (tag == FastSerializerTag.NullReference)
        {
            return true;
        }

        AssertTag(FastSerializerTag.BeginPrivateObject, tag);

        if (!TryReadSerializationType(ref reader, out var serializationType))
        {
            return false;
        }

        var blockRead = serializationType.Name == "Trace"
            ? TryReadTraceObject(ref reader)
            : TryReadBlock(ref reader, in serializationType);

        return blockRead
               && TryReadAndAssertTag(ref reader, FastSerializerTag.EndObject);
    }

    private bool TryReadSerializationType(
        ref FastSerializerSequenceReader reader,
        out SerializationType serializationType)
    {
        if (!TryReadAndAssertTag(ref reader, FastSerializerTag.BeginPrivateObject)
            || !TryReadAndAssertTag(ref reader, FastSerializerTag.NullReference)
            || !reader.TryReadInt32(out int objectVersion)
            || !reader.TryReadInt32(out int minReaderVersion)
            || !reader.TryReadInt32(out int typeNameLength)
            || !reader.TryReadBytes(typeNameLength, out var typeNameSeq)
            || !TryReadAndAssertTag(ref reader, FastSerializerTag.EndObject))
        {
            serializationType = default;
            return false;
        }

        // TODO: A custom dictionary allowing ReadOnlySequence for lookups could be built to avoid this allocation.
        string serializationTypeName = Encoding.UTF8.GetString(typeNameSeq);
        serializationType = new SerializationType(objectVersion, minReaderVersion, serializationTypeName);
        return true;
    }

    private bool TryReadTraceObject(ref FastSerializerSequenceReader reader)
    {
        if (!reader.TryReadInt16(out short year)
            || !reader.TryReadInt16(out short month)
            || !reader.TryReadInt16(out short dayOfWeek)
            || !reader.TryReadInt16(out short day)
            || !reader.TryReadInt16(out short hour)
            || !reader.TryReadInt16(out short minute)
            || !reader.TryReadInt16(out short second)
            || !reader.TryReadInt16(out short millisecond)
            || !reader.TryReadInt64(out long qpcSyncTime)
            || !reader.TryReadInt64(out long qpcFrequency)
            || !reader.TryReadInt32(out int pointerSize)
            || !reader.TryReadInt32(out int processId)
            || !reader.TryReadInt32(out int numberOfProcessors)
            || !reader.TryReadInt32(out int cpuSamplingRate))
        {
            return false;
        }

        var date = new DateTime(year, month, day, hour, minute, second, millisecond);
        _traceMetadata = new TraceMetadata(date, qpcSyncTime, qpcFrequency, pointerSize, processId, numberOfProcessors,
            cpuSamplingRate);

        return true;
    }

    private bool TryReadBlock(ref FastSerializerSequenceReader reader, in SerializationType serializationType)
    {
        if (!reader.TryReadInt32(out int blockSize)
            || !TryReadPadding(ref reader)
            || reader.Remaining < blockSize)
        {
            return false;
        }

        if (serializationType.MinReaderVersion > ReaderVersion)
        {
            _ = reader.ReadBytes(blockSize); // Skip the block for forward compatibility.
            return true;
        }

        long blockEndPosition = reader.AbsolutePosition + blockSize;

        switch (serializationType.Name)
        {
            case "StackBlock":
                ReadStackBlock(ref reader);
                break;
            case "MetadataBlock":
            case "EventBlock":
                ReadMetadataOrEventBlock(ref reader, blockSize);
                break;
            case "SPBlock":
                ReadSequencePointBlock(ref reader);
                break;
            default:
                _ = reader.ReadBytes(blockSize); // Skip the block for forward compatibility.
                break;
        }

        Debug.Assert(reader.AbsolutePosition == blockEndPosition,
            $"{serializationType.Name} end was not reached (expected: {blockEndPosition}, actual: {reader.AbsolutePosition})");

        return true;
    }

    private void ReadStackBlock(ref FastSerializerSequenceReader reader)
    {
        int firstId = reader.ReadInt32();
        int count = reader.ReadInt32();

        for (int i = 0; i < count; i += 1)
        {
            int stackSize = reader.ReadInt32();
            int addressesCount = stackSize / sizeof(ulong);
            var addresses = new ulong[addressesCount];
            for (int j = 0; j < addressesCount; j += 1)
            {
                addresses[j] = (ulong)reader.ReadInt64();
            }

            int stackId = firstId + i;
            _stackResolver.AddStackAddresses(stackId, addresses);
        }
    }

    private void ReadMetadataOrEventBlock(ref FastSerializerSequenceReader reader, long blockSize)
    {
        long headerStartPosition = reader.AbsolutePosition;
        long blockEndPosition = headerStartPosition + blockSize;

        short headerSize = reader.ReadInt16();
        var flags = ReadInt16AsEnum<EventBlockFlags>(ref reader);
        long minTimestamp = reader.ReadInt64();
        long maxTimestamp = reader.ReadInt64();
        _ = reader.ReadBytes((int)(headerSize - reader.AbsolutePosition + headerStartPosition));

        if (flags.HasFlag(EventBlockFlags.Compressed))
        {
            CompressedEventBlobState state = new();
            while (reader.AbsolutePosition < blockEndPosition)
            {
                ReadCompressedEventBlob(ref reader, ref state);
            }
        }
        else
        {
            while (reader.AbsolutePosition < blockEndPosition)
            {
                ReadUncompressedEventBlob(ref reader);
            }
        }
    }

    private void ReadUncompressedEventBlob(ref FastSerializerSequenceReader reader)
    {
        Debug.Assert(false, "uncompressed");
        int eventSize = reader.ReadInt32();
        int metadataId = reader.ReadInt32();
        int sequenceNumber = reader.ReadInt32();
        long threadId = reader.ReadInt64();
        long captureThreadId = reader.ReadInt64();
        int processorNumber = reader.ReadInt32();
        int stackId = reader.ReadInt32();
        long timeStamp = reader.ReadInt64();
        Guid activityId = reader.ReadGuid();
        int payloadSize = reader.ReadInt32();
        var payloadSeq = reader.ReadBytes(payloadSize);
        ReadPadding(ref reader);
    }

    private void ReadCompressedEventBlob(ref FastSerializerSequenceReader reader,
        ref CompressedEventBlobState state)
    {
        var flags = ReadByteAsEnum<CompressedEventFlags>(ref reader);

        var metadataId = flags.HasFlag(CompressedEventFlags.HasMetadataId)
            ? reader.ReadVarInt32()
            : state.PreviousMetadataId;

        int sequenceNumber;
        long captureThreadId;
        int processorNumber;
        if (flags.HasFlag(CompressedEventFlags.HasSequenceNumberAndCaptureThreadIdAndProcessorNumber))
        {
            sequenceNumber = reader.ReadVarInt32();
            captureThreadId = reader.ReadVarInt64();
            processorNumber = reader.ReadVarInt32();

            sequenceNumber += state.PreviousSequenceNumber;
        }
        else
        {
            sequenceNumber = state.PreviousSequenceNumber;
            captureThreadId = state.PreviousCaptureThreadId;
            processorNumber = state.PreviousProcessorNumber;
        }

        if (metadataId != 0)
        {
            sequenceNumber += 1;
        }

        long threadId = flags.HasFlag(CompressedEventFlags.HasThreadId)
            ? reader.ReadVarInt64()
            : state.PreviousThreadId;

        int stackId = flags.HasFlag(CompressedEventFlags.HasStackId)
            ? reader.ReadVarInt32()
            : state.PreviousStackId;

        long timeStamp = reader.ReadVarInt64() + state.PreviousTimeStamp;

        Guid activityId = flags.HasFlag(CompressedEventFlags.HasActivityId)
            ? reader.ReadGuid()
            : state.PreviousActivityId;

        Guid relatedActivityId = flags.HasFlag(CompressedEventFlags.HasRelatedActivityId)
            ? reader.ReadGuid()
            : state.PreviousRelatedActivityId;

        bool isSorted = flags.HasFlag(CompressedEventFlags.IsSorted);

        int payloadSize = flags.HasFlag(CompressedEventFlags.HasPayloadSize)
            ? reader.ReadVarInt32()
            : state.PreviousPayloadSize;

        long payloadEndPosition = reader.AbsolutePosition + payloadSize;

        bool isMetadataBlock = metadataId == 0;
        if (isMetadataBlock)
        {
            var metadata = ReadEventMetadata(ref reader, payloadEndPosition);
            _eventMetadata[metadata.MetadataId] = metadata;
        }
        else
        {
            var metadata = _eventMetadata[metadataId];

            // Some events (e.g. from Microsoft-Windows-DotNETRuntimeRundown) don't define any fields but still have
            // a payload. Skip them.
            if (metadata.FieldDefinitions.Count == 0)
            {
                _ = reader.ReadBytes((int)(payloadEndPosition - reader.AbsolutePosition));
            }
            else
            {
                var payload = ReadEventPayload(ref reader, metadata);

                Event evt = new(_events.Count, sequenceNumber, captureThreadId, threadId, stackId, timeStamp,
                    activityId, relatedActivityId, payload, metadata);
                _events.Add(evt);

                HandleSpecialEvent(evt);
            }
        }

        Debug.Assert(reader.AbsolutePosition == payloadEndPosition,
            $"Event blob payload end was not reached (expected: {payloadEndPosition}, actual: {reader.AbsolutePosition})");

        state.PreviousMetadataId = metadataId;
        state.PreviousSequenceNumber = sequenceNumber;
        state.PreviousCaptureThreadId = captureThreadId;
        state.PreviousProcessorNumber = processorNumber;
        state.PreviousThreadId = threadId;
        state.PreviousStackId = stackId;
        state.PreviousTimeStamp = timeStamp;
        state.PreviousActivityId = activityId;
        state.PreviousRelatedActivityId = relatedActivityId;
        state.PreviousPayloadSize = payloadSize;
    }

    private EventMetadata ReadEventMetadata(
        ref FastSerializerSequenceReader reader,
        long metadataEndPosition)
    {
        int metadataId = reader.ReadInt32();
        string providerName = ReadNullTerminatedUtf16String(ref reader);
        int eventId = reader.ReadInt32();
        string eventName = ReadNullTerminatedUtf16String(ref reader);
        long keywords = reader.ReadInt64();
        int version = reader.ReadInt32();
        var level = ReadInt32AsEnum<EventLevel>(ref reader);

        providerName = InternString(providerName);

        IReadOnlyList<EventFieldDefinition> fieldDefinitions = ReadFieldDefinitions(ref reader, EventFieldDefinitionVersion.V1);

        EventOpcode? opCode = null;
        while (reader.AbsolutePosition < metadataEndPosition)
        {
            int tagPayloadBytes = reader.ReadInt32();
            var tag = ReadByteAsEnum<EventMetadataTag>(ref reader);

            long tagPayloadEndPosition = reader.AbsolutePosition + tagPayloadBytes;

            if (tag == EventMetadataTag.OpCode)
            {
                opCode = ReadByteAsEnum<EventOpcode>(ref reader);
            }
            else if (tag == EventMetadataTag.ParameterPayload)
            {
                Debug.Assert(fieldDefinitions.Count == 0,
                    "No V2 field definitions are expected after V1 field definitions");

                fieldDefinitions = ReadFieldDefinitions(ref reader, EventFieldDefinitionVersion.V2);
            }

            Debug.Assert(reader.AbsolutePosition == tagPayloadEndPosition,
                $"Event metadata tag end was not reached (expected: {tagPayloadEndPosition}, actual: {reader.AbsolutePosition})");
        }

        // Some Microsoft-Windows-DotNETRuntimeRundown events are needed to resolve stack symbols but for some reasons
        // their metadata are incomplete in the trace (https://github.com/dotnet/runtime/issues/96365) so they are hardcoded.
        if (KnownEventMetadata.TryGetValue(new MetadataKey(providerName, eventId, version), out var knownMetadata))
        {
            eventName = knownMetadata.EventName;
            fieldDefinitions = knownMetadata.FieldDefinitions;
        }

        return new EventMetadata(metadataId, providerName, eventId, eventName, (EventKeywords)keywords,
            version, level, opCode, fieldDefinitions);
    }

    private EventFieldDefinition[] ReadFieldDefinitions(
        ref FastSerializerSequenceReader reader,
        EventFieldDefinitionVersion version)
    {
        int fieldCount = reader.ReadInt32();
        if (fieldCount == 0)
        {
            return Array.Empty<EventFieldDefinition>();
        }

        var fieldDefinitions = new EventFieldDefinition[fieldCount];
        for (int i = 0; i < fieldCount; i += 1)
        {
            var typeCode = ReadInt32AsEnum<TypeCode>(ref reader);

            TypeCode arrayTypeCode = default;
            if (version == EventFieldDefinitionVersion.V2
                && typeCode == TypeCodeExtensions.Array)
            {
                arrayTypeCode = ReadInt32AsEnum<TypeCode>(ref reader);
            }

            EventFieldDefinition[]? subFieldDefinitions = null;
            if (typeCode == TypeCode.Object)
            {
                subFieldDefinitions = ReadFieldDefinitions(ref reader, version);
            }

            string fieldName = ReadNullTerminatedUtf16String(ref reader);
            fieldName = InternString(fieldName);

            TypeCode? nullableArrayTypeCode = arrayTypeCode == default ? null : arrayTypeCode;
            fieldDefinitions[i] = new EventFieldDefinition(fieldName, typeCode, nullableArrayTypeCode, subFieldDefinitions);
        }

        return fieldDefinitions;
    }

    private Dictionary<string, object> ReadEventPayload(
        ref FastSerializerSequenceReader reader,
        EventMetadata metadata)
    {
        Dictionary<string, object> payload = new(capacity: metadata.FieldDefinitions.Count);
        foreach (var fieldDefinition in metadata.FieldDefinitions)
        {
            object value = ReadFieldValue(ref reader, fieldDefinition);
            payload[fieldDefinition.Name] = value;
        }

        return payload;
    }

    private object ReadFieldValue(
        ref FastSerializerSequenceReader reader,
        EventFieldDefinition fieldDefinition)
    {
        if (fieldDefinition.TypeCode == TypeCodeExtensions.Guid)
        {
            return reader.ReadGuid();
        }

        return fieldDefinition.TypeCode switch
        {
            TypeCode.Boolean => InternBoolean(reader.ReadInt32() != 0),
            TypeCode.SByte => Intern((sbyte)reader.ReadInt32(), _internedSByte),
            TypeCode.Byte => Intern(reader.ReadByte(), _internedByte),
            TypeCode.Int16 => Intern(reader.ReadInt16(), _internedInt16),
            TypeCode.UInt16 => Intern((ushort)reader.ReadInt16(), _internedUInt16),
            TypeCode.Int32 => reader.ReadInt32(),
            TypeCode.UInt32 => (uint)reader.ReadInt32(),
            TypeCode.Int64 => reader.ReadInt64(),
            TypeCode.UInt64 => (ulong)reader.ReadInt64(),
            TypeCode.Single => reader.ReadSingle(),
            TypeCode.Double => reader.ReadDouble(),
            TypeCode.String => ReadNullTerminatedUtf16String(ref reader),
            _ => throw new NotSupportedException($"Type {fieldDefinition.TypeCode} is not supported")
        };
    }

    private static string ReadNullTerminatedUtf16String(ref FastSerializerSequenceReader reader)
    {
        var unreadCharSpan = MemoryMarshal.Cast<byte, char>(reader.UnreadSpan);
        int nullIdx = unreadCharSpan.IndexOf((char)0);
        if (nullIdx == 0)
        {
            reader.Advance(sizeof(char));
            return "";
        }

        if (nullIdx != -1)
        {
            string str = new(unreadCharSpan[..nullIdx]);
            reader.Advance((nullIdx + 1) * sizeof(char));
            return str;
        }

        // Ain't nobody got time for that.
        return ReadNullTerminatedUtf16StringSlow(ref reader);
    }

    private static string ReadNullTerminatedUtf16StringSlow(ref FastSerializerSequenceReader reader)
    {
        StringBuilder sb = new();

        while (true)
        {
            short c = reader.ReadInt16();
            if (c == 0)
            {
                break;
            }

            sb.Append(Convert.ToChar(c));
        }

        return sb.ToString();
    }

    private void HandleSpecialEvent(Event evt)
    {
        if (evt.Metadata.ProviderName == RundownProvider)
        {
            switch (evt.Metadata.EventId)
            {
                case 144:
                {
                    var address = (ulong)evt.Payload["MethodStartAddress"];
                    var size = (uint)evt.Payload["MethodSize"];
                    var @namespace = (string)evt.Payload["MethodNamespace"];
                    var name = (string)evt.Payload["MethodName"];
                    var signature = (string)evt.Payload["MethodSignature"];
                    _stackResolver.AddMethodSymbolInfo(new MethodDescription(name, @namespace, signature, address, size));
                    break;
                }
            }
        }
    }

    private void ReadSequencePointBlock(ref FastSerializerSequenceReader reader)
    {
        long timeStamp = reader.ReadInt64();
        int threadCount = reader.ReadInt32();

        for (int i = 0; i < threadCount; i += 1)
        {
            ReadThreadSequencePoint(ref reader);
        }
    }

    private static void ReadThreadSequencePoint(ref FastSerializerSequenceReader reader)
    {
        long threadId = reader.ReadInt64();
        int sequenceNumber = reader.ReadInt32();
    }

    private static TEnum ReadByteAsEnum<TEnum>(ref FastSerializerSequenceReader reader) where TEnum : Enum
    {
        CorruptedBlockException.ThrowIfFalse(TryReadByteAsEnum<TEnum>(ref reader, out var value), reader.AbsolutePosition);
        return value;
    }

    private static bool TryReadByteAsEnum<TEnum>(
        ref FastSerializerSequenceReader reader,
        [MaybeNullWhen(false)] out TEnum value) where TEnum : Enum
    {
        if (!reader.TryReadByte(out byte b))
        {
            value = default;
            return false;
        }

        value = Unsafe.As<byte, TEnum>(ref b);
        return true;
    }

    private TEnum ReadInt16AsEnum<TEnum>(ref FastSerializerSequenceReader reader)
    {
        short value = reader.ReadInt16();
        return Unsafe.As<short, TEnum>(ref value);
    }

    private static TEnum ReadInt32AsEnum<TEnum>(ref FastSerializerSequenceReader reader) where TEnum : Enum
    {
        CorruptedBlockException.ThrowIfFalse(TryReadInt32AsEnum<TEnum>(ref reader, out var value), reader.AbsolutePosition);
        return value;
    }

    private static bool TryReadInt32AsEnum<TEnum>(
        ref FastSerializerSequenceReader reader,
        [MaybeNullWhen(false)] out TEnum value) where TEnum : Enum
    {
        if (!reader.TryReadInt32(out int i))
        {
            value = default;
            return false;
        }

        value = Unsafe.As<int, TEnum>(ref i);
        return true;
    }

    private static bool TryReadAndAssertTag(ref FastSerializerSequenceReader reader, FastSerializerTag expectedTag)
    {
        if (!TryReadByteAsEnum(ref reader, out FastSerializerTag tag))
        {
            return false;
        }

        AssertTag(expectedTag, tag);
        return true;
    }

    private static void AssertTag(FastSerializerTag expectedTag, FastSerializerTag actualTag)
    {
        if (expectedTag != actualTag)
        {
            throw new Exception($"Expected tag '{expectedTag}' but got '{actualTag}'");
        }
    }

    private static void ReadPadding(ref FastSerializerSequenceReader reader)
    {
        CorruptedBlockException.ThrowIfFalse(TryReadPadding(ref reader), reader.AbsolutePosition);
    }

    private static bool TryReadPadding(ref FastSerializerSequenceReader reader)
    {
        const int padding = 4;

        long position = reader.AbsolutePosition;
        if (position % padding == 0)
        {
            return true;
        }

        int bytesToSkip = (int)(padding - position % padding);
        Debug.Assert(bytesToSkip > 0);
        if (!reader.TryReadBytes(bytesToSkip, out var paddingSeq))
        {
            return false;
        }

        Debug.Assert(paddingSeq.ToArray().All(b => b == 0), "Padding is not zero");
        return true;
    }

    private string InternString(string str)
    {
        if (_internedStrings.TryGetValue(str, out string? cachedStr))
        {
            return cachedStr;
        }

        _internedStrings.Add(str);
        return str;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static object InternBoolean(bool b)
    {
        return b ? TrueBoolean : FalseBoolean;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static object Intern<T>(T val, Dictionary<T, object> dictionary) where T : notnull
    {
        if (!dictionary.TryGetValue(val, out object? obj))
        {
            obj = val;
            dictionary[val] = obj;
        }

        return obj;
    }

    private async Task FillPipeWithStreamAsync(PipeWriter writer, CancellationToken cancellationToken)
    {
        const int minimumBufferSize = 128 * 1024;

        while (true)
        {
            var buffer = writer.GetMemory(minimumBufferSize);
            try
            {
                int bytesRead = await stream.ReadAsync(buffer, cancellationToken);
                if (bytesRead == 0)
                {
                    break;
                }

                writer.Advance(bytesRead);
            }
            catch (Exception e)
            {
                await writer.CompleteAsync(e);
                break;
            }

            var flushResult = await writer.FlushAsync(cancellationToken);
            if (flushResult.IsCompleted)
            {
                break;
            }
        }

        await writer.CompleteAsync();
    }

    public class Progression
    {
        internal Progression(long bytesRead, int eventsRead)
        {
            BytesRead = bytesRead;
            EventsRead = eventsRead;
        }

        public long BytesRead { get; }
        public int EventsRead { get; }
    }

    private readonly record struct SerializationType(int ObjectVersion, int MinReaderVersion, string Name);

    [Flags]
    private enum EventBlockFlags : byte
    {
        Compressed = 1 << 0,
    }

    private struct CompressedEventBlobState
    {
        public int PreviousMetadataId { get; set; }
        public int PreviousSequenceNumber { get; set; }
        public long PreviousCaptureThreadId { get; set; }
        public int PreviousProcessorNumber { get; set; }
        public long PreviousThreadId { get; set; }
        public int PreviousStackId { get; set; }
        public long PreviousTimeStamp { get; set; }
        public Guid PreviousActivityId { get; set; }
        public Guid PreviousRelatedActivityId { get; set; }
        public int PreviousPayloadSize { get; set; }
    }

    [Flags]
    private enum CompressedEventFlags : byte
    {
        HasMetadataId = 1 << 0,
        HasSequenceNumberAndCaptureThreadIdAndProcessorNumber = 1 << 1,
        HasThreadId = 1 << 2,
        HasStackId = 1 << 3,
        HasActivityId = 1 << 4,
        HasRelatedActivityId = 1 << 5,
        IsSorted = 1 << 6,
        HasPayloadSize = 1 << 7,
    }

    private enum EventFieldDefinitionVersion
    {
        V1,
        V2,
    }

    private enum EventMetadataTag : byte
    {
        OpCode = 1,
        ParameterPayload = 2,
    }

    private record struct MetadataKey(string ProviderName, int EventId, int Version);
}
