namespace EventPipe;

public sealed class Trace
{
    internal Trace(TraceMetadata metadata, IReadOnlyList<EventMetadata> eventMetadata,
        IReadOnlyList<Event> events)
    {
        Metadata = metadata;
        Events = events;
        EventMetadata = eventMetadata;
    }

    public TraceMetadata Metadata { get; }
    public IReadOnlyList<EventMetadata> EventMetadata { get; }
    public IReadOnlyList<Event> Events { get; }
}