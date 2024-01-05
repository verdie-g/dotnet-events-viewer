using System.Diagnostics.Tracing;

namespace EventPipe;

public record EventMetadata(
    int MetadataId,
    string ProviderName,
    int EventId,
    string EventName,
    EventKeywords Keywords,
    int Version,
    EventLevel Level,
    EventOpcode? OpCode,
    IReadOnlyList<EventFieldDefinition> FieldDefinitions);
