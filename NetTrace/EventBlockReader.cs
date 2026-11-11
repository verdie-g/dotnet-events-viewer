using NetTrace.FastSerializer;

namespace NetTrace;

internal static class EventBlockReader
{
    internal readonly record struct Header(int MetadataId, int SequenceNumber, long ThreadId,
        long CaptureThreadId, int ProcessorNumber, int StackId, long TimeStamp, int PayloadSize,
        Guid ActivityId = default, Guid RelatedActivityId = default, int LabelListId = 0);

    public static bool ReadBlockHeader(ref FastSerializerSequenceReader reader)
    {
        long start = reader.AbsolutePosition;
        int headerSize = reader.ReadUInt16();
        int flags = reader.ReadUInt16();
        reader.ReadInt64(); // minimum timestamp
        reader.ReadInt64(); // maximum timestamp
        if (headerSize < reader.AbsolutePosition - start)
            throw new InvalidDataException("Invalid event block header size");
        long extensionSize = headerSize - (reader.AbsolutePosition - start);
        if (extensionSize > reader.Remaining) throw new EndOfStreamException("Incomplete event block header");
        reader.Advance(extensionSize);
        return (flags & 1) != 0;
    }

    public static Header ReadUncompressed(ref FastSerializerSequenceReader row, bool isV6)
    {
        int metadataId = row.ReadInt32() & 0x7FFFFFFF;
        int sequence = row.ReadInt32();
        long thread = row.ReadInt64();
        long capture = row.ReadInt64();
        int processor = row.ReadInt32();
        int stack = row.ReadInt32();
        long timestamp = row.ReadInt64();
        Guid activity = default;
        Guid related = default;
        int labels = 0;
        if (isV6)
        {
            labels = row.ReadInt32();
        }
        else
        {
            activity = row.ReadGuid();
            related = row.ReadGuid();
        }
        int payloadSize = row.ReadInt32();
        return new Header(metadataId, sequence, thread, capture, processor, stack, timestamp,
            payloadSize, activity, related, labels);
    }

    public static Header ReadCompressed(ref FastSerializerSequenceReader reader, bool isV6, Header previous)
    {
        var flags = (Flags)reader.ReadByte();
        int metadata = flags.HasFlag(Flags.Metadata) ? reader.ReadVarInt32() : previous.MetadataId;
        int sequence = previous.SequenceNumber;
        long capture = previous.CaptureThreadId;
        int processor = previous.ProcessorNumber;
        if (flags.HasFlag(Flags.SequenceCaptureProcessor))
        {
            sequence += reader.ReadVarInt32();
            capture = reader.ReadVarInt64();
            processor = reader.ReadVarInt32();
        }
        if (metadata != 0) sequence++;
        long thread = flags.HasFlag(Flags.Thread) ? reader.ReadVarInt64() : previous.ThreadId;
        int stack = flags.HasFlag(Flags.Stack) ? reader.ReadVarInt32() : previous.StackId;
        long timestamp = reader.ReadVarInt64() + previous.TimeStamp;
        Guid activity = previous.ActivityId;
        Guid related = previous.RelatedActivityId;
        int labels = previous.LabelListId;
        if (isV6)
        {
            if (flags.HasFlag(Flags.ActivityOrLabels)) labels = reader.ReadVarInt32();
        }
        else
        {
            if (flags.HasFlag(Flags.ActivityOrLabels)) activity = reader.ReadGuid();
            if (flags.HasFlag(Flags.RelatedActivity)) related = reader.ReadGuid();
        }
        int payloadSize = flags.HasFlag(Flags.PayloadSize) ? reader.ReadVarInt32() : previous.PayloadSize;
        return new Header(metadata, sequence, thread, capture, processor, stack, timestamp,
            payloadSize, activity, related, labels);
    }

    [Flags]
    private enum Flags : byte
    {
        Metadata = 1,
        SequenceCaptureProcessor = 2,
        Thread = 4,
        Stack = 8,
        ActivityOrLabels = 16,
        RelatedActivity = 32,
        PayloadSize = 128,
    }
}
