using DotnetEventViewer.CallTree.CountAggregators;
using DotnetEventViewer.Models;

namespace DotnetEventViewer.Querying;

internal class Query
{
    public EventKey[] EventKeys { get; set; } = [];
    public Filter[] Filters { get; set; } = [];
    public IEnumerable<Field>? SelectedColumnFields { get; set; }
    public ICallTreeCountAggregator? SelectedAggregator { get; set; }
    public bool ThreadPoolStacksOnly { get; set; }
}
