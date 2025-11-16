using System.Diagnostics.Tracing;

namespace NetTrace;

public record EventMetadata(
    int MetadataId,
    Guid? ProviderGuid,
    string ProviderName,
    int EventId,
    string EventName,
    string? EventDescription,
    EventKeywords Keywords,
    int Version,
    EventLevel Level,
    EventOpcode? Opcode,
    IReadOnlyList<EventFieldDefinition> FieldDefinitions,
    string? MessageTemplate);
