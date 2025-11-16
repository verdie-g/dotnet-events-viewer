using NetTrace;

namespace DotnetEventsViewer.Querying.Operators;

internal interface IOperator
{
    /// <summary>Text representation.</summary>
    public string Text { get; }

    public bool IsCompatible(NetTraceTypeCode code);

    public bool Match(object? evtFieldValue, object filterValue);
}
