using DotnetEventsViewer.CallTree.CountAggregators;
using DotnetEventsViewer.Models;

namespace DotnetEventsViewer.Querying;

internal class Query
{
    public EventKey[] EventKeys { get; set; } = [];
    public Filter[] Filters { get; set; } = [];
    public IEnumerable<Field>? SelectedColumnFields { get; set; }
    public ICallTreeCountAggregator? SelectedAggregator { get; set; }
    public bool BottomUpTree { get; set; } = true;
}
