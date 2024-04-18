namespace DotnetEventViewer.Querying.Operators;

internal interface IOperator
{
    /// <summary>Text representation.</summary>
    public string Text { get; }

    public bool IsCompatible(TypeCode code);

    public bool Match(object evtFieldValue, object filterValue);
}
