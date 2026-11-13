using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components.DataGrid.Infrastructure;
using NetTrace;

namespace DotnetEventsViewer.Querying;

/// <summary>Caches the current ordering; virtual pages only copy their requested slice.</summary>
internal sealed class EventGridDataSource
{
    private readonly IReadOnlyList<Event> _events;
    private SortLevel[] _sortLevels = [];
    private IReadOnlyList<Event> _orderedEvents;

    public EventGridDataSource(IReadOnlyList<Event> events)
    {
        _events = events;
        _orderedEvents = events;
    }

    public ValueTask<GridItemsProviderResult<Event>> Provide(GridItemsProviderRequest<Event> request)
    {
        request.CancellationToken.ThrowIfCancellationRequested();
        if (!MatchesSort(request.SortColumns))
        {
            var sortLevels = request.SortColumns
                .Select(sort => new SortLevel(sort.Column, sort.Column.SortBy, sort.Ascending)).ToArray();
            IReadOnlyList<Event> orderedEvents = sortLevels.Length == 0
                ? _events
                : request.ApplySorting(_events.AsQueryable()).ToArray();
            request.CancellationToken.ThrowIfCancellationRequested();
            _sortLevels = sortLevels;
            _orderedEvents = orderedEvents;
        }

        int start = Math.Clamp(request.StartIndex, 0, _orderedEvents.Count);
        int count = Math.Min(Math.Max(request.Count ?? _orderedEvents.Count, 0), _orderedEvents.Count - start);
        var page = new Event[count];
        for (int i = 0; i < count; i++) page[i] = _orderedEvents[start + i];
        return ValueTask.FromResult(new GridItemsProviderResult<Event>
        {
            Items = page,
            TotalItemCount = _orderedEvents.Count
        });
    }

    private bool MatchesSort(IReadOnlyList<DataGridSortColumn<Event>> columns)
    {
        if (columns.Count != _sortLevels.Length) return false;
        for (int i = 0; i < columns.Count; i++)
        {
            var column = columns[i];
            var cached = _sortLevels[i];
            if (!ReferenceEquals(column.Column, cached.Column)
                || !ReferenceEquals(column.Column.SortBy, cached.SortBy)
                || column.Ascending != cached.Ascending) return false;
        }
        return true;
    }

    private readonly record struct SortLevel(ColumnBase<Event> Column, IGridSort<Event>? SortBy, bool Ascending);
}
