using DotnetEventViewer.CallTree.CountAggregators;

namespace DotnetEventViewer.Querying;

internal class Query
{
    public string[] EventNames { get; set; } = [];
    public Filter[] Filters { get; set; } = [];
    public IEnumerable<Field>? SelectedColumnFields { get; set; }
    public ICallTreeCountAggregator? SelectedAggregator { get; set; }
    public bool ThreadPoolStacksOnly { get; set; }
}
