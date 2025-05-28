using System.Text.RegularExpressions;
using NetTrace;

namespace DotnetEventsViewer.Querying.Operators;

internal class NotMatchOperator : IOperator
{
    public static NotMatchOperator Instance { get; } = new();

    public string Text => "≄";
    public bool IsCompatible(TypeCode code) => code != TypeCodeExtensions.Array;
    public bool Match(object? evtFieldValue, object filterValue)
    {
        string? evtFieldValueStr = evtFieldValue?.ToString();
        return evtFieldValueStr != null && !((Regex)filterValue).IsMatch(evtFieldValueStr);
    }
}