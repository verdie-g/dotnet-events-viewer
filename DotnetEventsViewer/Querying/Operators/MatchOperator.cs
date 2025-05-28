using System.Text.RegularExpressions;
using NetTrace;

namespace DotnetEventsViewer.Querying.Operators;

internal class MatchOperator : IOperator
{
    public static MatchOperator Instance { get; } = new();

    public string Text => "≃";
    public bool IsCompatible(TypeCode code) => code != TypeCodeExtensions.Array;
    public bool Match(object? evtFieldValue, object filterValue)
    {
        string? evtFieldValueStr = evtFieldValue?.ToString();
        return evtFieldValueStr != null && ((Regex)filterValue).IsMatch(evtFieldValueStr);
    }
}