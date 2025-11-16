using NetTrace;

namespace DotnetEventsViewer.Querying.Operators;

internal class LessOrEqualOperator : IOperator
{
    public static LessOrEqualOperator Instance { get; } = new();

    public string Text => "≤";
    public bool IsCompatible(NetTraceTypeCode code) => code != NetTraceTypeCode.Array;
    public bool Match(object? evtFieldValue, object filterValue) => ((IComparable)filterValue).CompareTo(evtFieldValue) >= 0;
}