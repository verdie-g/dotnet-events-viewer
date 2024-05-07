using EventPipe;

namespace DotnetEventsViewer.Querying.Operators;

internal class EqualOperator : IOperator
{
    public static EqualOperator Instance { get; } = new();

    public string Text => "=";
    public bool IsCompatible(TypeCode code) => code != TypeCodeExtensions.Array;
    public bool Match(object? evtFieldValue, object filterValue) => Equals(evtFieldValue, filterValue);
}