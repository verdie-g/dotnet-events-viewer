using DotnetEventViewer.CallTree.Decorators;

namespace DotnetEventViewer.Querying;

internal class Query
{
    public string[] EventNames { get; set; } = [];
    public Filter[] Filters { get; set; } = [];
    public IEnumerable<Field>? SelectedColumnFields { get; set; }
    public ICallTreeNodeDecorator? SelectedDecorator { get; set; }
    public bool ThreadPoolStacksOnly { get; set; }
}
