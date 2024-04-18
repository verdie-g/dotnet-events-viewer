using EventPipe;

namespace DotnetEventViewer.Querying;

internal class Field(string name, TypeCode type, Func<Event, object> selector)
{
    public static readonly Field CaptureThreadIdField = new(nameof(Event.CaptureThreadId), TypeCode.Int64, e => e.CaptureThreadId);
    public static readonly Field ThreadIdField = new(nameof(Event.ThreadId), TypeCode.Int64, e => e.ThreadId);
    public static readonly Field TimeStampField = new(nameof(Event.TimeStamp), TypeCode.Int64, e => e.TimeStamp);
    public static readonly Field ActivityIdField = new(nameof(Event.ActivityId), TypeCodeExtensions.Guid, e => e.ActivityId);
    public static readonly Field RelatedActivityIdField = new(nameof(Event.RelatedActivityId), TypeCodeExtensions.Guid, e => e.RelatedActivityId);
    public static readonly Field EventNameField = new(nameof(EventMetadata.EventName), TypeCode.String, e => e.Metadata.EventName);

    public static readonly Field[] StaticEventFieldSelectors =
    [
        CaptureThreadIdField,
        ThreadIdField,
        TimeStampField,
        ActivityIdField,
        RelatedActivityIdField,
        EventNameField,
    ];

    public static Field FromPayloadFieldDefinition(EventFieldDefinition fieldDefinition)
    {
        return new Field(
            fieldDefinition.Name,
            fieldDefinition.TypeCode,
            e => e.Payload.GetValueOrDefault(fieldDefinition.Name, ""));
    }

    public string Name { get; } = name;
    public TypeCode Type { get; } = type;
    public Func<Event, object> Selector { get; } = selector;
}
