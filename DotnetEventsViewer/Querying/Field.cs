using EventPipe;

namespace DotnetEventsViewer.Querying;

public class Field(string name, TypeCode type, Func<Event, object?> selector, EventMetadata[]? associatedEventMetadata = null)
{
    public static readonly Field CaptureThreadIdField = new(nameof(Event.CaptureThreadId), TypeCode.Int64, e => e.CaptureThreadId);
    public static readonly Field ThreadIdField = new(nameof(Event.ThreadId), TypeCode.Int64, e => e.ThreadId);
    public static readonly Field TimeStampField = new(nameof(Event.TimeStamp), TypeCode.Single, e => e.TimeStamp / 1_000_000f);
    public static readonly Field ActivityIdField = new(nameof(Event.ActivityId), TypeCodeExtensions.Guid, e => e.ActivityId);
    public static readonly Field RelatedActivityIdField = new(nameof(Event.RelatedActivityId), TypeCodeExtensions.Guid, e => e.RelatedActivityId);
    public static readonly Field ProviderNameField = new(nameof(EventMetadata.ProviderName), TypeCode.String, e => e.Metadata.ProviderName);
    public static readonly Field EventNameField = new(nameof(EventMetadata.EventName), TypeCode.String, e => e.Metadata.EventName);

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

    public static Field FromNameAndType(string name, TypeCode typeCode, EventMetadata[] eventMetadata)
    {
        return new Field(
            name,
            typeCode,
            e => e.Payload.GetValueOrDefault(name),
            eventMetadata);
    }

    public string Name { get; } = name;
    public TypeCode Type { get; } = type;
    public Func<Event, object?> Selector { get; } = selector;

    /// <summary>
    /// The event the field is associated to. Can contain several events if the same name is used for different events.
    /// </summary>
    public EventMetadata[]? AssociatedEventMetadata { get; } = associatedEventMetadata;
}
