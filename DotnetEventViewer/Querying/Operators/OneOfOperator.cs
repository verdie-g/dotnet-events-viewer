using EventPipe;

namespace DotnetEventViewer.Querying.Operators;

internal class OneOfOperator : IOperator
{
    public static OneOfOperator Instance { get; } = new();

    public string Text => "oneof";
    public bool IsCompatible(TypeCode code) => code != TypeCodeExtensions.Array;
    public bool Match(object? evtFieldValue, object filterValue)
    {
        string? evtFieldValueStr = evtFieldValue?.ToString();
        string[]? filterValueStrs = (string[]?)filterValue;
        return evtFieldValueStr != null
               && filterValueStrs != null
               && Array.IndexOf(filterValueStrs, evtFieldValueStr) != -1;
    }
}