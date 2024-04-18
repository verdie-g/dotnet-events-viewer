using EventPipe;

namespace DotnetEventViewer.Querying.Operators;

internal class GreaterOperator : IOperator
{
    public static GreaterOperator Instance { get; } = new();

    public string Text => ">";
    public bool IsCompatible(TypeCode code) => code != TypeCodeExtensions.Array;
    public bool Match(object evtFieldValue, object filterValue) => ((IComparable)evtFieldValue).CompareTo(filterValue) > 0;
}