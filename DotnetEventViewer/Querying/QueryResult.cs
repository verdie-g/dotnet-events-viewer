using DotnetEventViewer.CallTree.CountAggregators;
using EventPipe;

namespace DotnetEventViewer.Querying;

public class QueryResult(
    IReadOnlyList<Event> filteredEvents,
    IEnumerable<Field>? columnFields,
    ICallTreeCountAggregator? callTreeAggregator,
    bool bottomUpTree)
{
    public IReadOnlyList<Event> FilteredEvents { get; } = filteredEvents;

    /// <summary>Non-null for <see cref="QueryType.List"/>.</summary>
    public IEnumerable<Field>? ColumnFields { get; } = columnFields;

    /// <summary>Non-null for <see cref="QueryType.Tree"/>.</summary>
    public ICallTreeCountAggregator? CallTreeAggregator { get; } = callTreeAggregator;

    public bool BottomUpTree { get; } = bottomUpTree;
}