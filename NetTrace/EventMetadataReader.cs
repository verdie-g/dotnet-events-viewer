using System.Diagnostics.Tracing;
using NetTrace.FastSerializer;

namespace NetTrace;

internal sealed class EventMetadataReader
{
    private readonly HashSet<string> _internedStrings = [];

    // The caller bounds the reader to one V5 metadata payload.
    public EventMetadata ReadEventMetadata(ref FastSerializerSequenceReader reader)
    {
        int metadataId = reader.ReadInt32();
        string providerName = InternString(reader.ReadNullTerminatedUtf16String());
        int eventId = reader.ReadInt32();
        string eventName = reader.ReadNullTerminatedUtf16String();
        var keywords = (EventKeywords)reader.ReadInt64();
        int version = reader.ReadInt32();
        var level = (EventLevel)reader.ReadInt32();

        IReadOnlyList<EventFieldDefinition> fieldDefinitions = ReadFieldDefinitions(ref reader, EventFieldDefinitionVersion.V1);

        EventOpcode? opCode = null;
        while (!reader.End)
        {
            int tagPayloadBytes = reader.ReadInt32();
            var tag = (EventMetadataTag)reader.ReadByte();
            if (tagPayloadBytes < 0) throw new InvalidDataException("Invalid metadata tag payload size");
            FastSerializerSequenceReader tagReader = new(reader.ReadBytes(tagPayloadBytes), 0);

            if (tag == EventMetadataTag.OpCode)
            {
                opCode = (EventOpcode)tagReader.ReadByte();
            }
            else if (tag == EventMetadataTag.ParameterPayload)
            {
                fieldDefinitions = ReadFieldDefinitions(ref tagReader, EventFieldDefinitionVersion.V2);
            }

            // Tag payloads can contain extensions unknown to this reader.
            tagReader.Advance(tagReader.Remaining);
        }

        if (KnownEvent.All.TryGetValue(new KnownEvent.Key(providerName, eventId, version), out var knownMetadata))
        {
            eventName = knownMetadata.EventName;
            opCode = knownMetadata.Opcode;
            fieldDefinitions = knownMetadata.FieldDefinitions;
        }

        // If the metadata is neither complete nor hardcoded.
        if (eventName.Length == 0)
        {
            eventName = $"Event {eventId}";
        }

        return new EventMetadata(metadataId, null, providerName, eventId, eventName, "", keywords,
            version, level, opCode, fieldDefinitions, null);
    }

    // The caller bounds the reader to one V6 metadata row.
    public EventMetadata ReadEventMetadataV6(ref FastSerializerSequenceReader reader)
    {
        int metadataId = reader.ReadVarInt32();
        string providerName = InternString(reader.ReadUtf8String());
        int eventId = reader.ReadVarInt32();
        string eventName = reader.ReadUtf8String();
        IReadOnlyList<EventFieldDefinition> fieldDefinitions = ReadFieldDefinitions(ref reader, EventFieldDefinitionVersion.V6);

        EventOpcode? opCode = null;
        EventLevel level = EventLevel.Informational;
        EventKeywords keywords = EventKeywords.None;
        int version = 0;
        Guid? providerGuid = null;
        string? messageTemplate = null;
        string description = "";

        if (!reader.End)
        {
            ushort optionalMetadataSize = reader.ReadUInt16();
            FastSerializerSequenceReader optional = new(reader.ReadBytes(optionalMetadataSize), 0);

            while (!optional.End)
            {
                var kind = (EventMetadataTag)optional.ReadByte();
                switch (kind)
                {
                    case EventMetadataTag.OpCode:
                        opCode = (EventOpcode)optional.ReadByte();
                        break;
                    case EventMetadataTag.Keyword:
                        keywords = (EventKeywords)optional.ReadInt64();
                        break;
                    case EventMetadataTag.Level:
                        level = (EventLevel)optional.ReadByte();
                        break;
                    case EventMetadataTag.Version:
                        version = optional.ReadByte();
                        break;
                    case EventMetadataTag.MessageTemplate:
                        messageTemplate = optional.ReadUtf8String();
                        break;
                    case EventMetadataTag.Description:
                        description = optional.ReadUtf8String();
                        break;
                    case EventMetadataTag.KeyValue:
                        optional.ReadUtf8String(); // key
                        optional.ReadUtf8String(); // value
                        break;
                    case EventMetadataTag.ProviderGuid:
                        providerGuid = optional.ReadGuid();
                        break;
                    default:
                        throw new InvalidDataException($"Unknown metadata tag {kind}");
                }
            }
        }

        // Preserve forward compatibility with extensions after the optional metadata.
        reader.Advance(reader.Remaining);

        return new EventMetadata(metadataId, providerGuid, providerName, eventId, eventName, description,
            keywords, version, level, opCode, fieldDefinitions, messageTemplate);
    }

    private EventFieldDefinition[] ReadFieldDefinitions(
        ref FastSerializerSequenceReader reader,
        EventFieldDefinitionVersion version)
    {
        int fieldCount = version == EventFieldDefinitionVersion.V6 ? reader.ReadUInt16() : reader.ReadInt32();
        if (fieldCount < 0 || fieldCount > reader.Remaining)
        {
            throw new InvalidDataException("Invalid metadata field count");
        }
        if (fieldCount == 0)
        {
            return [];
        }

        var fieldDefinitions = new EventFieldDefinition[fieldCount];
        for (int i = 0; i < fieldCount; i += 1)
        {
            if (version == EventFieldDefinitionVersion.V2)
            {
                // V2 includes the size prefix in the declared field size.
                int size = reader.ReadInt32();
                if (size < sizeof(int)) throw new InvalidDataException("Invalid field description size");
                FastSerializerSequenceReader fieldReader = new(reader.ReadBytes(size - sizeof(int)), 0);
                fieldDefinitions[i] = ReadFieldDefinition(ref fieldReader, version);
                fieldReader.Advance(fieldReader.Remaining);
            }
            else
            {
                fieldDefinitions[i] = version == EventFieldDefinitionVersion.V6
                    ? ReadFieldDefinitionV6(ref reader)
                    : ReadFieldDefinition(ref reader, version);
            }
        }

        return fieldDefinitions;
    }

    private EventFieldDefinition ReadFieldDefinition(
        ref FastSerializerSequenceReader reader,
        EventFieldDefinitionVersion version)
    {
        var typeCode = (NetTraceTypeCode)reader.ReadInt32();

        NetTraceTypeCode arrayTypeCode = default;
        if (version == EventFieldDefinitionVersion.V2
            && typeCode == NetTraceTypeCode.Array)
        {
            arrayTypeCode = (NetTraceTypeCode)reader.ReadInt32();
        }

        EventFieldDefinition[]? subFieldDefinitions = null;
        if (typeCode == NetTraceTypeCode.Object)
        {
            subFieldDefinitions = ReadFieldDefinitions(ref reader, version);
        }

        string fieldName = InternString(reader.ReadNullTerminatedUtf16String());

        NetTraceTypeCode? nullableArrayTypeCode = arrayTypeCode == default ? null : arrayTypeCode;
        return new EventFieldDefinition(fieldName, typeCode, nullableArrayTypeCode, subFieldDefinitions);
    }

    private EventFieldDefinition ReadFieldDefinitionV6(ref FastSerializerSequenceReader reader)
    {
        ushort fieldLength = reader.ReadUInt16();
        FastSerializerSequenceReader fieldReader = new(reader.ReadBytes(fieldLength), 0);
        string fieldName = fieldReader.ReadUtf8String();
        var definition = ReadTypeDefinitionV6(ref fieldReader, fieldName);
        fieldReader.Advance(fieldReader.Remaining);
        return definition;
    }

    private EventFieldDefinition ReadTypeDefinitionV6(ref FastSerializerSequenceReader reader, string name)
    {
        var typeCode = (NetTraceTypeCode)reader.ReadByte();
        EventFieldDefinition? element = null;
        ushort? elementCount = null;
        EventFieldDefinition[]? fields = null;
        if (typeCode is NetTraceTypeCode.Array or NetTraceTypeCode.FixedLengthArray or NetTraceTypeCode.RelLoc or NetTraceTypeCode.DataLoc)
        {
            element = ReadTypeDefinitionV6(ref reader, "");
            if (typeCode == NetTraceTypeCode.FixedLengthArray) elementCount = reader.ReadUInt16();
        }
        else if (typeCode == NetTraceTypeCode.Object)
        {
            fields = ReadFieldDefinitions(ref reader, EventFieldDefinitionVersion.V6);
        }
        return new EventFieldDefinition(InternString(name), typeCode, element?.TypeCode,
            fields ?? element?.SubFieldDefinitions, elementCount, element);
    }

    private string InternString(string str)
    {
        if (_internedStrings.TryGetValue(str, out string? cachedStr))
        {
            return cachedStr;
        }

        _internedStrings.Add(str);
        return str;
    }

    private enum EventFieldDefinitionVersion
    {
        V1,
        V2,
        V6,
    }

    private enum EventMetadataTag : byte
    {
        OpCode = 1,
        ParameterPayload = 2,
        Keyword = 3,
        MessageTemplate = 4,
        Description = 5,
        KeyValue = 6,
        ProviderGuid = 7,
        Level = 8,
        Version = 9,
    }
}
