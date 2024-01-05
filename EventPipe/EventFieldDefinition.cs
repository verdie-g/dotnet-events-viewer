namespace EventPipe;

/// <summary>
/// Definition of the field of an event pipe event.
/// </summary>
/// <param name="Name">Name of the field.</param>
/// <param name="TypeCode">Extended type code of the field (can be <see cref="TypeCodeExtensions.Array"/> and <see cref="TypeCodeExtensions.Guid"/>).</param>
/// <param name="ArrayTypeCode">Extended type code of the element in the array if <paramref name="TypeCode"/> is <see cref="TypeCodeExtensions.Array"/>; otherwise null.</param>
/// <param name="SubFieldDefinitions">Defines the field of the sub-object if <paramref name="TypeCode"/> is <see cref="TypeCode.Object"/>; otherwise null.</param>
public record EventFieldDefinition(string Name, TypeCode TypeCode, TypeCode? ArrayTypeCode = null, EventFieldDefinition[]? SubFieldDefinitions = null);