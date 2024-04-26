using EventPipe;

namespace DotnetEventViewer.Querying.Operators;

internal class LessOrEqualOperator : IOperator
{
    public static LessOrEqualOperator Instance { get; } = new();

    public string Text => "≤";
    public bool IsCompatible(TypeCode code) => code != TypeCodeExtensions.Array;
    public bool Match(object? evtFieldValue, object filterValue) => ((IComparable)filterValue).CompareTo(evtFieldValue) >= 0;
}