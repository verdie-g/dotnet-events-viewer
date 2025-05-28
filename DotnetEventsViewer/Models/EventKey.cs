using NetTrace;

namespace DotnetEventsViewer.Models;

public record EventKey(string ProviderName, string EventName, int EventId)
{
    public static EventKey From(EventMetadata m) => new(m.ProviderName, m.EventName, m.EventId);

    public bool Matches(Event evt) => Matches(evt.Metadata);
    public bool Matches(EventMetadata m) => EventId == m.EventId && ProviderName == m.ProviderName;
}