namespace NetTrace;

/// <summary>
/// Definition of the field of an event pipe event.
/// </summary>
/// <param name="Name">Name of the field.</param>
/// <param name="TypeCode">Type code of the field.</param>
/// <param name="ArrayTypeCode">Type code of the element in the array if <paramref name="TypeCode"/> is <see cref="NetTraceTypeCode.Array"/>; otherwise null.</param>
/// <param name="SubFieldDefinitions">Defines the field of the sub-object if <paramref name="TypeCode"/> is <see cref="NetTraceTypeCode.Object"/>; otherwise null.</param>
/// <param name="ElementCount">Number of elements in a fixed-length array.</param>
/// <param name="ElementDefinition">Recursive element layout for nested arrays.</param>
/// <param name="CountFieldName">An earlier payload field holding this array's element count.</param>
/// <param name="AlignPointers">CLR schema annotation normalized to an explicit padding field before payload decoding.</param>
public record EventFieldDefinition(
    string Name,
    NetTraceTypeCode TypeCode,
    NetTraceTypeCode? ArrayTypeCode = null,
    EventFieldDefinition[]? SubFieldDefinitions = null,
    ushort? ElementCount = null,
    EventFieldDefinition? ElementDefinition = null,
    string? CountFieldName = null,
    bool AlignPointers = false);
