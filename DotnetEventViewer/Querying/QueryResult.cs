using DotnetEventViewer.CallTree.Decorators;
using EventPipe;

namespace DotnetEventViewer.Querying;

public class QueryResult(IReadOnlyList<Event> filteredEvents, IEnumerable<Field>? columnFields, ICallTreeNodeDecorator? callTreeDecorator)
{
    public IReadOnlyList<Event> FilteredEvents { get; } = filteredEvents;

    /// <summary>Non-null for <see cref="QueryType.List"/>.</summary>
    public IEnumerable<Field>? ColumnFields { get; } = columnFields;

    /// <summary>Non-null for <see cref="QueryType.Tree"/>.</summary>
    public ICallTreeNodeDecorator? CallTreeDecorator { get; } = callTreeDecorator;
}