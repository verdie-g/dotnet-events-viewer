namespace EventPipe;

public class Trace
{
    internal Trace(TraceMetadata metadata, IReadOnlyList<EventMetadata> eventMetadata,
        IReadOnlyList<Event> events, IReadOnlyList<StackTrace> stackTraces)
    {
        Metadata = metadata;
        Events = events;
        StackTraces = stackTraces;
        EventMetadata = eventMetadata;
    }

    public TraceMetadata Metadata { get; }
    public IReadOnlyList<EventMetadata> EventMetadata { get; }
    public IReadOnlyList<Event> Events { get; }
    public IReadOnlyList<StackTrace> StackTraces { get; }
}