namespace EventPipe;

public class Trace
{
    internal Trace(TraceMetadata metadata, IReadOnlyList<Event> events)
    {
        Metadata = metadata;
        Events = events;
    }

    public TraceMetadata Metadata { get; }
    public IReadOnlyList<Event> Events { get; }
}