using NetTrace;

namespace DotnetEventsViewer.Querying.Operators;

internal class GreaterOrEqualOperator : IOperator
{
    public static GreaterOrEqualOperator Instance { get; } = new();

    public string Text => "≥";
    public bool IsCompatible(TypeCode code) => code != TypeCodeExtensions.Array;
    public bool Match(object? evtFieldValue, object filterValue) => ((IComparable)filterValue).CompareTo(evtFieldValue) <= 0;
}