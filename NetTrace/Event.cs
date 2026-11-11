using System.Diagnostics;

namespace NetTrace;

[DebuggerDisplay("{(Metadata.EventName.Length == 0 ? Metadata.EventId.ToString() : Metadata.EventName)}")]
public sealed class Event
{
    private static readonly IReadOnlyDictionary<string, object> EmptyLabels = new Dictionary<string, object>();

    /// <summary>Uniquely identifies the <see cref="Event"/> in a <see cref="Trace"/>.</summary>
    public int Index { get; }
    public int SequenceNumber { get; }
    public long CaptureThreadId { get; }
    public long ThreadId { get; }
    public long ProcessId { get; internal set; } = -1;
    public int ProcessorNumber { get; internal set; }
    public string? ThreadName { get; internal set; }
    public Guid? TraceId { get; internal set; }
    public ulong? SpanId { get; internal set; }
    public IReadOnlyDictionary<string, object> Labels { get; internal set; } = EmptyLabels;
    internal int StackIndex { get; set; }
    public long TimeStamp { get; }
    public Guid ActivityId { get; }
    public Guid RelatedActivityId { get; }
    public IReadOnlyDictionary<string, object> Payload { get; }
    public EventMetadata Metadata { get; }
    public StackTrace StackTrace { get; internal set; }

    internal Event(int index, int sequenceNumber, long captureThreadId, long threadId, int stackIndex, long timeStamp,
        Guid activityId, Guid relatedActivityId, IReadOnlyDictionary<string, object> payload, EventMetadata metadata)
    {
        Index = index;
        SequenceNumber = sequenceNumber;
        CaptureThreadId = captureThreadId;
        ThreadId = threadId;
        StackIndex = stackIndex;
        TimeStamp = timeStamp;
        ActivityId = activityId;
        RelatedActivityId = relatedActivityId;
        Payload = payload;
        Metadata = metadata;
        StackTrace = StackTrace.Empty;
    }
}