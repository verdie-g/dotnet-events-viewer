using System.Collections;
using DotnetEventsViewer.Querying;
using Microsoft.FluentUI.AspNetCore.Components;
using NetTrace;
using NUnit.Framework;

// Standalone columns are sort descriptors here, not renderer-managed components.
#pragma warning disable BL0005

namespace DotnetEventsViewer.Test;

[TestFixture]
public class EventGridDataSourceTest
{
    [TestCase(0, 2, new[] { 0, 1 })]
    [TestCase(2, 20, new[] { 2, 3 })]
    [TestCase(2, null, new[] { 2, 3 })]
    [TestCase(4, 2, new int[0])]
    [TestCase(100, 2, new int[0])]
    [TestCase(-2, 2, new[] { 0, 1 })]
    [TestCase(1, 0, new int[0])]
    [TestCase(1, -1, new int[0])]
    public async Task UnsortedPagesUseIndexingAndPreserveSourceOrder(int start, int? count, int[] expected)
    {
        IndexedOnlyEvents events = new(Enumerable.Range(0, 4).Select(i => SyntheticEvents.Event(i, 4 - i)).ToArray());
        EventGridDataSource source = new(events);

        var page = await source.Provide(Request(start, count));

        Assert.Multiple(() =>
        {
            Assert.That(page.Items.Select(evt => evt.Index), Is.EqualTo(expected));
            Assert.That(page.TotalItemCount, Is.EqualTo(4));
            Assert.That(events.IndexReads, Is.EqualTo(expected.Length), "Unsorted paging must not walk earlier rows.");
        });
    }

    [Test]
    public async Task EmptySourceReturnsEmptyPageAndZeroTotal()
    {
        var page = await new EventGridDataSource(Array.Empty<Event>()).Provide(Request(0, null));
        Assert.Multiple(() =>
        {
            Assert.That(page.Items, Is.Empty);
            Assert.That(page.TotalItemCount, Is.Zero);
        });
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task SortedPagesMatchFluentSortingAndProjectEachRowOnlyOnce(bool ascending)
    {
        Event[] events = [SyntheticEvents.Event(0, 20), SyntheticEvents.Event(1, 10),
            SyntheticEvents.Event(2, 20), SyntheticEvents.Event(3, 30), SyntheticEvents.Event(4, 10)];
        KeyCounter counter = new();
        var column = Column(GridSort<Event>.ByAscending(evt => counter.Timestamp(evt)));
        EventGridDataSource source = new(events);

        var first = await source.Provide(Request(0, 2, [new(column, ascending)]));
        var second = await source.Provide(Request(2, 2, [new(column, ascending)]));
        var last = await source.Provide(Request(4, null, [new(column, ascending)]));
        var repeated = await source.Provide(Request(0, 2, [new(column, ascending)]));
        var expected = ascending ? new[] { 1, 4, 0, 2, 3 } : new[] { 3, 0, 2, 1, 4 };

        Assert.Multiple(() =>
        {
            Assert.That(first.Items.Concat(second.Items).Concat(last.Items).Select(evt => evt.Index), Is.EqualTo(expected));
            Assert.That(repeated.Items, Is.EqualTo(first.Items));
            Assert.That(new[] { first.TotalItemCount, second.TotalItemCount, last.TotalItemCount }, Is.All.EqualTo(events.Length));
            Assert.That(counter.Calls, Is.EqualTo(events.Length), "New page requests/descriptor arrays must reuse the sorted cache.");
            Assert.That(events.Select(evt => evt.Index), Is.EqualTo(new[] { 0, 1, 2, 3, 4 }), "Sorting must not mutate source order.");
        });
    }

    [Test]
    public async Task OneHundredVirtualPagesAddZeroSelectorInvocationsAfterInitialSort()
    {
        const int eventCount = 10_000;
        const int pageSize = 100;
        Event[] events = Enumerable.Range(0, eventCount).Select(i => SyntheticEvents.Event(i, eventCount - i)).ToArray();
        KeyCounter counter = new();
        var column = Column(GridSort<Event>.ByAscending(evt => counter.Timestamp(evt)));
        EventGridDataSource source = new(events);
        await source.Provide(Request(0, pageSize, [new(column, true)]));
        int initialCalls = counter.Calls;
        Assert.That(initialCalls, Is.EqualTo(eventCount));

        for (int start = 0; start < eventCount; start += pageSize)
        {
            var page = await source.Provide(Request(start, pageSize, [new(column, true)]));
            Assert.That(page.Items.Select(evt => evt.Index), Is.EqualTo(
                Enumerable.Range(start, pageSize).Select(offset => eventCount - 1 - offset)));
        }

        Assert.That(counter.Calls - initialCalls, Is.Zero, "Scrolling 100 pages must project no additional sort keys.");
        TestContext.Out.WriteLine($"Grid cache workload: {eventCount:N0} initial selectors; 100 pages; {counter.Calls - initialCalls} additional selectors.");
    }

    [Test]
    public async Task ChangedDirectionColumnAndSortByEachInvalidateCache()
    {
        Event[] events = [SyntheticEvents.Event(0, 30), SyntheticEvents.Event(1, 10), SyntheticEvents.Event(2, 20)];
        KeyCounter counter = new();
        var sort = GridSort<Event>.ByAscending(evt => counter.Timestamp(evt));
        var column = Column(sort);
        EventGridDataSource source = new(events);
        await source.Provide(Request(0, 1, [new(column, true)]));
        var descending = await source.Provide(Request(0, null, [new(column, false)]));
        Assert.That(descending.Items.Select(evt => evt.Index), Is.EqualTo(new[] { 0, 2, 1 }));
        Assert.That(counter.Calls, Is.EqualTo(2 * events.Length));

        var replacementColumn = Column(sort);
        await source.Provide(Request(1, 1, [new(replacementColumn, false)]));
        Assert.That(counter.Calls, Is.EqualTo(3 * events.Length), "Column identity is part of the key even with the same SortBy.");

        replacementColumn.SortBy = GridSort<Event>.ByAscending(evt => counter.Index(evt));
        var changedSort = await source.Provide(Request(0, null, [new(replacementColumn, false)]));
        Assert.Multiple(() =>
        {
            Assert.That(changedSort.Items.Select(evt => evt.Index), Is.EqualTo(new[] { 2, 1, 0 }));
            Assert.That(counter.Calls, Is.EqualTo(4 * events.Length));
        });
        await source.Provide(Request(1, 1, [new(replacementColumn, false)]));
        Assert.That(counter.Calls, Is.EqualTo(4 * events.Length));
    }

    [Test]
    public async Task MultipleSortLevelsRespectPriorityDirectionsAndInvalidateWhenReordered()
    {
        Event[] events = [SyntheticEvents.Event(0, 20, 1), SyntheticEvents.Event(1, 10, 2),
            SyntheticEvents.Event(2, 20, 2), SyntheticEvents.Event(3, 10, 1)];
        KeyCounter counter = new();
        var timeColumn = Column(GridSort<Event>.ByAscending(evt => counter.Timestamp(evt)));
        var threadColumn = Column(GridSort<Event>.ByAscending(evt => counter.Thread(evt)));
        EventGridDataSource source = new(events);
        var first = await source.Provide(Request(0, null, [new(timeColumn, true), new(threadColumn, false)]));
        Assert.That(first.Items.Select(evt => evt.Index), Is.EqualTo(new[] { 1, 3, 2, 0 }));
        Assert.That(counter.Calls, Is.EqualTo(2 * events.Length));
        await source.Provide(Request(2, 1, [new(timeColumn, true), new(threadColumn, false)]));
        Assert.That(counter.Calls, Is.EqualTo(2 * events.Length));

        var reordered = await source.Provide(Request(0, null, [new(threadColumn, false), new(timeColumn, true)]));
        Assert.That(reordered.Items.Select(evt => evt.Index), Is.EqualTo(new[] { 1, 2, 3, 0 }));
        Assert.That(counter.Calls, Is.EqualTo(4 * events.Length));
        var removed = await source.Provide(Request(0, null, [new(timeColumn, true)]));
        Assert.That(removed.Items.Select(evt => evt.Index), Is.EqualTo(new[] { 1, 3, 0, 2 }));
        Assert.That(counter.Calls, Is.EqualTo(5 * events.Length));
    }

    [Test]
    public async Task CompositeSortRulesAreDelegatedToFluentApplySorting()
    {
        Event[] events = [SyntheticEvents.Event(0, 20, 1), SyntheticEvents.Event(1, 10, 2),
            SyntheticEvents.Event(2, 20, 2), SyntheticEvents.Event(3, 10, 1)];
        var column = Column(GridSort<Event>.ByAscending(evt => evt.TimeStamp).ThenAlwaysDescending(evt => evt.ThreadId));
        EventGridDataSource source = new(events);
        foreach (bool ascending in new[] { true, false })
        {
            var request = Request(0, null, [new(column, ascending)]);
            var expected = request.ApplySorting(events.AsQueryable()).ToArray();
            var actual = await source.Provide(request);
            Assert.That(actual.Items, Is.EqualTo(expected));
        }
    }

    [Test]
    public async Task ClearingSortRestoresOriginalOrderAndResortingRebuildsCurrentCache()
    {
        Event[] events = [SyntheticEvents.Event(0, 30), SyntheticEvents.Event(1, 10), SyntheticEvents.Event(2, 20)];
        KeyCounter counter = new();
        var column = Column(GridSort<Event>.ByAscending(evt => counter.Timestamp(evt)));
        EventGridDataSource source = new(events);
        await source.Provide(Request(0, null, [new(column, true)]));
        var unsorted = await source.Provide(Request(0, null));
        Assert.That(unsorted.Items, Is.EqualTo(events));
        Assert.That(counter.Calls, Is.EqualTo(events.Length));
        await source.Provide(Request(0, 1));
        Assert.That(counter.Calls, Is.EqualTo(events.Length));
        await source.Provide(Request(0, 1, [new(column, true)]));
        Assert.That(counter.Calls, Is.EqualTo(2 * events.Length));
    }

    [Test]
    public async Task NewQueryInstanceNeverReusesThePreviousQueryOrdering()
    {
        Event[] firstEvents = [SyntheticEvents.Event(0, 20), SyntheticEvents.Event(1, 10)];
        Event[] secondEvents = [SyntheticEvents.Event(10, 5), SyntheticEvents.Event(11, 30), SyntheticEvents.Event(12, 1)];
        KeyCounter counter = new();
        var column = Column(GridSort<Event>.ByAscending(evt => counter.Timestamp(evt)));
        var request = Request(0, null, [new(column, true)]);
        var first = await new EventGridDataSource(firstEvents).Provide(request);
        var second = await new EventGridDataSource(secondEvents).Provide(request);
        Assert.Multiple(() =>
        {
            Assert.That(first.Items.Select(evt => evt.Index), Is.EqualTo(new[] { 1, 0 }));
            Assert.That(second.Items.Select(evt => evt.Index), Is.EqualTo(new[] { 12, 10, 11 }));
            Assert.That(second.TotalItemCount, Is.EqualTo(3));
            Assert.That(counter.Calls, Is.EqualTo(5));
        });
    }

    [Test]
    public async Task AlreadyCancelledRequestDoesNotReadOrSortSource()
    {
        IndexedOnlyEvents events = new([SyntheticEvents.Event(0)]);
        KeyCounter counter = new();
        var column = Column(GridSort<Event>.ByAscending(evt => counter.Timestamp(evt)));
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        EventGridDataSource source = new(events);
        var request = Request(0, 1, [new(column, true)], cancellation.Token);

        await Assert.ThrowsAsync<OperationCanceledException>(async () => await source.Provide(request));
        Assert.Multiple(() =>
        {
            Assert.That(events.IndexReads, Is.Zero);
            Assert.That(counter.Calls, Is.Zero);
        });
    }

    [Test]
    public async Task CancellationDuringSortingDoesNotPublishNewCacheOrDiscardExistingOne()
    {
        Event[] events = [SyntheticEvents.Event(0, 30), SyntheticEvents.Event(1, 10), SyntheticEvents.Event(2, 20)];
        KeyCounter initialCounter = new();
        var initialColumn = Column(GridSort<Event>.ByAscending(evt => initialCounter.Timestamp(evt)));
        EventGridDataSource source = new(events);
        await source.Provide(Request(0, null, [new(initialColumn, true)]));
        using CancellationTokenSource cancellation = new();
        KeyCounter cancelledCounter = new() { CancelOnFirstSelection = cancellation };
        var cancelledColumn = Column(GridSort<Event>.ByAscending(evt => cancelledCounter.Index(evt)));
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await source.Provide(Request(0, null, [new(cancelledColumn, true)], cancellation.Token)));
        Assert.That(cancelledCounter.Calls, Is.EqualTo(events.Length), "Cancellation is raised deterministically by the sort selector.");

        var retained = await source.Provide(Request(0, null, [new(initialColumn, true)]));
        Assert.That(retained.Items.Select(evt => evt.Index), Is.EqualTo(new[] { 1, 2, 0 }));
        Assert.That(initialCounter.Calls, Is.EqualTo(events.Length), "Cancelled sort must leave the previous cache intact.");
        cancelledCounter.CancelOnFirstSelection = null;
        var retried = await source.Provide(Request(0, null, [new(cancelledColumn, true)]));
        Assert.Multiple(() =>
        {
            Assert.That(retried.Items.Select(evt => evt.Index), Is.EqualTo(new[] { 0, 1, 2 }));
            Assert.That(cancelledCounter.Calls, Is.EqualTo(2 * events.Length), "The cancelled cache must be rebuilt, not reused.");
        });
    }

    private static TemplateColumn<Event> Column(GridSort<Event> sort) => new() { SortBy = sort };

    private static GridItemsProviderRequest<Event> Request(int start, int? count,
        IReadOnlyList<DataGridSortColumn<Event>>? sorts = null, CancellationToken cancellation = default) => new()
        {
            StartIndex = start,
            Count = count,
            SortColumns = sorts ?? Array.Empty<DataGridSortColumn<Event>>(),
            CancellationToken = cancellation
        };

    private sealed class KeyCounter
    {
        public int Calls { get; private set; }
        public CancellationTokenSource? CancelOnFirstSelection { get; set; }
        public long Timestamp(Event evt) { Selected(); return evt.TimeStamp; }
        public long Thread(Event evt) { Selected(); return evt.ThreadId; }
        public int Index(Event evt) { Selected(); return evt.Index; }
        private void Selected()
        {
            Calls++;
            if (Calls == 1) CancelOnFirstSelection?.Cancel();
        }
    }

    private sealed class IndexedOnlyEvents(Event[] events) : IReadOnlyList<Event>
    {
        public int IndexReads { get; private set; }
        public int Count => events.Length;
        public Event this[int index] { get { IndexReads++; return events[index]; } }
        public IEnumerator<Event> GetEnumerator() => throw new AssertionException("Unsorted paging must not enumerate the source.");
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
