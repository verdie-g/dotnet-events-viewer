using NetTrace;

namespace DotnetEventsViewer.Querying;

public class Field(string name, NetTraceTypeCode type, Func<Event, object?> selector, EventMetadata[]? associatedEventMetadata = null)
{
    public static readonly Field CaptureThreadIdField = new(nameof(Event.CaptureThreadId), NetTraceTypeCode.Int64, e => e.CaptureThreadId);
    public static readonly Field ThreadIdField = new(nameof(Event.ThreadId), NetTraceTypeCode.Int64, e => e.ThreadId);
    public static readonly Field TimeStampField = new(nameof(Event.TimeStamp), NetTraceTypeCode.Single, e => e.TimeStamp / 1_000_000f);
    public static readonly Field ActivityIdField = new(nameof(Event.ActivityId), NetTraceTypeCode.Guid, e => e.ActivityId);
    public static readonly Field RelatedActivityIdField = new(nameof(Event.RelatedActivityId), NetTraceTypeCode.Guid, e => e.RelatedActivityId);
    public static readonly Field ProviderNameField = new(nameof(EventMetadata.ProviderName), NetTraceTypeCode.NullTerminatedUtf16String, e => e.Metadata.ProviderName);
    public static readonly Field EventNameField = new(nameof(EventMetadata.EventName), NetTraceTypeCode.NullTerminatedUtf16String, e => e.Metadata.EventName);

    public static readonly Field[] StaticEventField =
    [
        CaptureThreadIdField,
        ThreadIdField,
        TimeStampField,
        ActivityIdField,
        RelatedActivityIdField,
        ProviderNameField,
        EventNameField,
    ];

    public static Field FromNameAndType(string name, NetTraceTypeCode typeCode, EventMetadata[] eventMetadata)
    {
        return new Field(
            name,
            typeCode,
            e => e.Payload.GetValueOrDefault(name),
            eventMetadata);
    }

    public string Name { get; } = name;
    public NetTraceTypeCode Type { get; } = type;
    public Func<Event, object?> Selector { get; } = selector;

    /// <summary>
    /// The event the field is associated to. Can contain several events if the same name is used for different events.
    /// </summary>
    public EventMetadata[]? AssociatedEventMetadata { get; } = associatedEventMetadata;
}
