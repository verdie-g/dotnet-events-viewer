namespace EventPipe;

public class Trace
{
    internal Trace(TraceMetadata metadata, IReadOnlyList<Event> events, IReadOnlyDictionary<int, StackTrace> stackTraces)
    {
        Metadata = metadata;
        Events = events;
        StackTraces = stackTraces;
    }

    public TraceMetadata Metadata { get; }
    public IReadOnlyList<Event> Events { get; }
    public IReadOnlyDictionary<int, StackTrace> StackTraces { get; }
}