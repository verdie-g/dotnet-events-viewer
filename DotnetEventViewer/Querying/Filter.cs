using DotnetEventViewer.Querying.Operators;

namespace DotnetEventViewer.Querying;

internal class Filter(Field field, IOperator @operator, string value)
{
    public Field Field { get; set; } = field;
    public IOperator Operator { get; set; } = @operator;
    public string Value { get; set; } = value;
    public object ParsedValue { get; set; } = default!;
}
