using System.Linq.Expressions;
using DotnetEventsViewer.Models;
using DotnetEventsViewer.Pages;
using DotnetEventsViewer.Querying;
using DotnetEventsViewer.State;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using NetTrace;
using NUnit.Framework;
using static DotnetEventsViewer.Test.ComponentTestAccess;

// Parameters are read from the real generated render tree and assigned to standalone sort descriptors.
#pragma warning disable BL0005

namespace DotnetEventsViewer.Test;

[TestFixture]
public class EventsListComponentTest
{
    [TestCase(0)]
    [TestCase(2)]
    public async Task GeneratedGridUsesVirtualizedTableLayoutAndQueryItemsProvider(int eventCount)
    {
        EventsList page = CreatePage();
        Event[] events = Enumerable.Range(0, eventCount).Select(i => SyntheticEvents.Event(i)).ToArray();
        await Submit(page, events, Field.ThreadIdField);

        var grid = Grid(page);
        Assert.Multiple(() =>
        {
            Assert.That(grid.Type, Is.EqualTo(typeof(FluentDataGrid<Event>)));
            Assert.That(grid.Parameter<bool>(nameof(FluentDataGrid<Event>.Virtualize)), Is.True);
            Assert.That(grid.Parameter<DataGridDisplayMode>(nameof(FluentDataGrid<Event>.DisplayMode)), Is.EqualTo(DataGridDisplayMode.Table),
                "Virtualized spacer rows require table layout; real viewport geometry is covered by browser tests.");
            Assert.That(grid.Parameter<float>(nameof(FluentDataGrid<Event>.ItemSize)), Is.EqualTo(32));
            Assert.That(grid.Parameters.ContainsKey(nameof(FluentDataGrid<Event>.Items)), Is.False);
            Assert.That(grid.Parameter<Func<Event, object>>(nameof(FluentDataGrid<Event>.ItemKey))(SyntheticEvents.Event(42)), Is.EqualTo(42));
        });

        var result = await Provider(grid)(Request());
        Assert.Multiple(() =>
        {
            Assert.That(result.Items, Is.EqualTo(events));
            Assert.That(result.TotalItemCount, Is.EqualTo(eventCount));
        });
    }

    [Test]
    public async Task ReorderedColumnsReuseKeyAndExpressionByFieldIdentityNotName()
    {
        EventsList page = CreatePage();
        Event evt = SyntheticEvents.Event(0, threadId: 7);
        Field original = new("Dynamic", NetTraceTypeCode.Int64, _ => 11L);
        await Submit(page, [evt], original, Field.ThreadIdField);
        var first = Columns(Grid(page));

        await Submit(page, [evt], Field.ThreadIdField, original, Field.EventNameField);
        var reordered = Columns(Grid(page));
        Assert.Multiple(() =>
        {
            Assert.That(reordered.Select(column => column.Key), Is.EqualTo(new[] { Field.ThreadIdField, original, Field.EventNameField }));
            Assert.That(Property(reordered[0]), Is.SameAs(Property(first[1])));
            Assert.That(Property(reordered[1]), Is.SameAs(Property(first[0])), "Keeping a field must not recreate its expression/sort descriptor.");
            Assert.That(Property(reordered[0]).Compile()(evt), Is.EqualTo(7L));
        });

        Field replacement = new(original.Name, NetTraceTypeCode.Int64, _ => 22L);
        await Submit(page, [evt], Field.ThreadIdField, replacement);
        var changed = Columns(Grid(page));
        Assert.Multiple(() =>
        {
            Assert.That(changed[1].Key, Is.SameAs(replacement));
            Assert.That(Property(changed[0]), Is.SameAs(Property(first[1])));
            Assert.That(Property(changed[1]), Is.Not.SameAs(Property(first[0])), "A different field with the same name needs its own selector.");
            Assert.That(Property(changed[1]).Compile()(evt), Is.EqualTo(22L));
        });
    }

    [Test]
    public async Task NewQueryRetainsActiveSortForKeptFieldButUsesFreshItemsProvider()
    {
        EventsList page = CreatePage();
        await Submit(page, [SyntheticEvents.Event(0, threadId: 20), SyntheticEvents.Event(1, threadId: 10)], Field.ThreadIdField, Field.EventNameField);
        var first = Grid(page);
        var firstProperty = Property(Columns(first)[0]);
        PropertyColumn<Event, object?> column = new() { Property = firstProperty, SortBy = GridSort<Event>.ByAscending(firstProperty) };
        await using SortRenderer renderer = new();
        SortGrid grid = new() { Virtualize = true };
        await renderer.AttachAsync(grid);
        first.Capture!(grid);
        await renderer.Dispatcher.InvokeAsync(() => grid.SortByColumnAsync(column, DataGridSortDirection.Descending));
        int sortChanges = 0;
        grid.OnSortChanged = new EventCallbackFactory().Create<DataGridSortEventArgs<Event>>(new object(), _ => { sortChanges++; });
        var oldPage = await Provider(first)(Request(grid.SortColumns));
        Event[] nextEvents = [SyntheticEvents.Event(10, threadId: 5), SyntheticEvents.Event(11, threadId: 30), SyntheticEvents.Event(12, threadId: 1)];

        await renderer.Dispatcher.InvokeAsync(() => Submit(page, nextEvents, Field.EventNameField, Field.ThreadIdField));
        var next = Grid(page);
        var nextPage = await Provider(next)(Request(grid.SortColumns));
        Assert.Multiple(() =>
        {
            Assert.That(Property(Columns(next)[1]), Is.SameAs(firstProperty));
            Assert.That(grid.SortColumns, Has.Count.EqualTo(1));
            Assert.That(grid.SortColumns[0].Column, Is.SameAs(column));
            Assert.That(grid.SortColumns[0].Ascending, Is.False);
            Assert.That(sortChanges, Is.Zero, "Rerunning/reordering kept fields must not reset the user's sort.");
            Assert.That(Provider(next), Is.Not.EqualTo(Provider(first)), "Each query needs a new data source/cache.");
            Assert.That(oldPage.Items.Select(evt => evt.Index), Is.EqualTo(new[] { 0, 1 }));
            Assert.That(nextPage.Items.Select(evt => evt.Index), Is.EqualTo(new[] { 11, 10, 12 }));
            Assert.That(nextPage.TotalItemCount, Is.EqualTo(3));
        });
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task RemovingSortedFieldClearsSortAndReturnsPendingGridCallbackTask(bool failCallback)
    {
        EventsList page = CreatePage();
        await Submit(page, [SyntheticEvents.Event(0)], Field.ThreadIdField, Field.EventNameField);
        var first = Grid(page);
        var property = Property(Columns(first)[0]);
        PropertyColumn<Event, object?> column = new() { Property = property, SortBy = GridSort<Event>.ByAscending(property) };
        await using SortRenderer renderer = new();
        SortGrid grid = new() { Virtualize = true };
        await renderer.AttachAsync(grid);
        first.Capture!(grid);
        await renderer.Dispatcher.InvokeAsync(() => grid.SortByColumnAsync(column, DataGridSortDirection.Ascending));
        TaskCompletionSource completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        int sortChanges = 0;
        grid.OnSortChanged = new EventCallbackFactory().Create<DataGridSortEventArgs<Event>>(new object(), _ =>
        {
            sortChanges++;
            return completion.Task;
        });
        Task submitted = Task.CompletedTask;
        Event[] nextEvents = [SyntheticEvents.Event(10), SyntheticEvents.Event(11)];
        InvalidOperationException expected = new("sort reset failed");

        try
        {
            await renderer.Dispatcher.InvokeAsync(() => { submitted = Submit(page, nextEvents, Field.EventNameField); });
            Assert.Multiple(() =>
            {
                Assert.That(grid.SortColumns, Is.Empty, "Removing the sorted field must discard its detached column.");
                Assert.That(sortChanges, Is.EqualTo(1));
                Assert.That(submitted.IsCompleted, Is.False, "Page submit must wait for Fluent's asynchronous sort reset.");
                Assert.That(Columns(Grid(page)).Select(item => item.Key), Is.EqualTo(new[] { Field.EventNameField }));
            });
            var nextPage = await Provider(Grid(page))(Request(grid.SortColumns));
            Assert.That(nextPage.Items, Is.EqualTo(nextEvents), "Replacement source must be installed before resetting the sort.");

            if (failCallback)
            {
                completion.SetException(expected);
                var actual = await Assert.ThrowsAsync<InvalidOperationException>(async () => await submitted);
                Assert.That(actual, Is.SameAs(expected));
            }
            else
            {
                completion.SetResult();
                await submitted;
                Assert.That(submitted.IsCompletedSuccessfully, Is.True);
            }
        }
        finally
        {
            completion.TrySetResult();
        }
    }

    private static EventsList CreatePage()
    {
        EventsList page = new();
        SetProperty(page, "State", new StateContainer
        {
            TraceExtraMetadata = new TraceExtraMetadata("synthetic.nettrace", "tests", "test", new Version(11, 0), TimeSpan.Zero)
        });
        return page;
    }

    private static Task Submit(EventsList page, IReadOnlyList<Event> events, params Field[] fields) =>
        Invoke<Task>(page, "OnQuery", new QueryResult(events, fields, null, false));

    private static RenderedComponent Grid(EventsList page) => Render(page).Single(component => component.Type == typeof(FluentDataGrid<Event>));
    private static RenderedComponent[] Columns(RenderedComponent grid) => Render(grid.Parameter<RenderFragment>("ChildContent"));
    private static Expression<Func<Event, object?>> Property(RenderedComponent column) =>
        column.Parameter<Expression<Func<Event, object?>>>(nameof(PropertyColumn<Event, object?>.Property));
    private static GridItemsProvider<Event> Provider(RenderedComponent grid) => grid.Parameter<GridItemsProvider<Event>>(nameof(FluentDataGrid<Event>.ItemsProvider));
    private static GridItemsProviderRequest<Event> Request(IReadOnlyList<DataGridSortColumn<Event>>? sorts = null) => new()
    {
        StartIndex = 0,
        Count = null,
        SortColumns = sorts ?? Array.Empty<DataGridSortColumn<Event>>()
    };
}
