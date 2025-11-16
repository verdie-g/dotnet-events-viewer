namespace NetTrace;

/// <summary>
/// Definition of the field of an event pipe event.
/// </summary>
/// <param name="Name">Name of the field.</param>
/// <param name="TypeCode">Type code of the field.</param>
/// <param name="ArrayTypeCode">Type code of the element in the array if <paramref name="TypeCode"/> is <see cref="NetTraceTypeCode.Array"/>; otherwise null.</param>
/// <param name="SubFieldDefinitions">Defines the field of the sub-object if <paramref name="TypeCode"/> is <see cref="NetTraceTypeCode.Object"/>; otherwise null.</param>
public record EventFieldDefinition(string Name, NetTraceTypeCode TypeCode, NetTraceTypeCode? ArrayTypeCode = null, EventFieldDefinition[]? SubFieldDefinitions = null);