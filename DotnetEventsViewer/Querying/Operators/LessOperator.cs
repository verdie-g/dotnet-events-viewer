using NetTrace;

namespace DotnetEventsViewer.Querying.Operators;

internal class LessOperator : IOperator
{
    public static LessOperator Instance { get; } = new();

    public string Text => "<";
    public bool IsCompatible(NetTraceTypeCode code) => code != NetTraceTypeCode.Array;
    public bool Match(object? evtFieldValue, object filterValue) => ((IComparable)filterValue).CompareTo(evtFieldValue) > 0;
}