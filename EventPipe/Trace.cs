namespace EventPipe;

public class Trace
{
    internal Trace(TraceMetadata metadata, IReadOnlyDictionary<int, EventMetadata> eventMetadata,
        IReadOnlyList<Event> events, IReadOnlyDictionary<int, StackTrace> stackTraces)
    {
        Metadata = metadata;
        Events = events;
        StackTraces = stackTraces;
        EventMetadata = eventMetadata;
    }

    public TraceMetadata Metadata { get; }
    public IReadOnlyDictionary<int, EventMetadata> EventMetadata { get; }
    public IReadOnlyList<Event> Events { get; }
    public IReadOnlyDictionary<int, StackTrace> StackTraces { get; }
}