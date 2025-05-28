using System.Diagnostics.Tracing;

namespace NetTrace;

public record EventMetadata(
    int MetadataId,
    string ProviderName,
    int EventId,
    string EventName,
    EventKeywords Keywords,
    int Version,
    EventLevel Level,
    EventOpcode? Opcode,
    IReadOnlyList<EventFieldDefinition> FieldDefinitions);
