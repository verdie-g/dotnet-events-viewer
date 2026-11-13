using System.Collections;
using DotnetEventsViewer.CallTree;
using DotnetEventsViewer.CallTree.CountAggregators;
using NetTrace;
using NUnit.Framework;
using StackTrace = NetTrace.StackTrace;

namespace DotnetEventsViewer.Test;

[TestFixture]
public class CallTreeNodeTest
{
    [TestCase(false)]
    [TestCase(true)]
    public void SharedStacksStillProcessAndUpdateEveryEventInReverseOrder(bool bottomUp)
    {
        var stack = SyntheticEvents.Stack(7, SyntheticEvents.Method("Leaf", 1), SyntheticEvents.Method("Caller", 2));
        Event[] events = Enumerable.Range(0, 5).Select(i => SyntheticEvents.Event(i, i, stack: stack)).ToArray();
        RecordingProcessor processor = new();

        var root = CallTreeNode.Create(events, processor, bottomUp);

        Assert.Multiple(() =>
        {
            Assert.That(processor.Calls, Is.EqualTo(new[] { "process:4", "update:4", "process:3", "update:3",
                "process:2", "update:2", "process:1", "update:1", "process:0", "update:0" }));
            Assert.That(root.Count, Is.EqualTo(15));
            Assert.That(root.Children!.Keys, Is.EqualTo(new ulong[] { bottomUp ? 1UL : 2UL }));
            Assert.That(root.Children.Values.Single().Count, Is.EqualTo(15));
            Assert.That(root.Children.Values.Single().Children!.Values.Single().Count, Is.EqualTo(15));
        });
    }

    [TestCase(false)]
    [TestCase(true)]
    public void EqualStackIndexesWithDifferentResolvedIdentitiesRemainSeparate(bool bottomUp)
    {
        var first = SyntheticEvents.Stack(7, SyntheticEvents.Method("ProcessOne", 1));
        var second = SyntheticEvents.Stack(7, SyntheticEvents.Method("ProcessTwo", 2));
        Event[] events = [SyntheticEvents.Event(0, stack: first), SyntheticEvents.Event(1, stack: second),
            SyntheticEvents.Event(2, stack: first), SyntheticEvents.Event(3, stack: second)];

        var root = CallTreeNode.Create(events, EventsCountAggregator.Instance.CreateProcessor(), bottomUp);

        Assert.Multiple(() =>
        {
            Assert.That(root.Count, Is.EqualTo(4));
            Assert.That(root.Children!.Keys, Is.EquivalentTo(new ulong[] { 1, 2 }));
            Assert.That(root.Children[1].Count, Is.EqualTo(2));
            Assert.That(root.Children[2].Count, Is.EqualTo(2));
            Assert.That(root.Children[1].MethodDescription, Is.SameAs(first.Frames[0]));
            Assert.That(root.Children[2].MethodDescription, Is.SameAs(second.Frames[0]));
        });
    }

    [TestCase(false)]
    [TestCase(true)]
    public void LeafCacheIsLocalToEachBuildAndTreeDirection(bool bottomUp)
    {
        var stack = SyntheticEvents.Stack(1, SyntheticEvents.Method("Leaf", 1), SyntheticEvents.Method("Caller", 2));
        Event[] events = [SyntheticEvents.Event(0, stack: stack), SyntheticEvents.Event(1, stack: stack)];
        var first = CallTreeNode.Create(events, EventsCountAggregator.Instance.CreateProcessor(), bottomUp);
        var second = CallTreeNode.Create(events, EventsCountAggregator.Instance.CreateProcessor(), !bottomUp);

        Assert.Multiple(() =>
        {
            Assert.That(first.Count, Is.EqualTo(2));
            Assert.That(second.Count, Is.EqualTo(2));
            Assert.That(first.Children!.Keys.Single(), Is.EqualTo(bottomUp ? 1UL : 2UL));
            Assert.That(second.Children!.Keys.Single(), Is.EqualTo(bottomUp ? 2UL : 1UL));
            Assert.That(first.Children.Values.Single(), Is.Not.SameAs(second.Children.Values.Single()));
        });
    }

    [TestCase(false)]
    [TestCase(true)]
    public void EmptyStacksContributeToRootAlongsideFramedEvents(bool bottomUp)
    {
        var stack = SyntheticEvents.Stack(1, SyntheticEvents.Method("Leaf", 1));
        Event[] events = [SyntheticEvents.Event(0), SyntheticEvents.Event(1, stack: stack), SyntheticEvents.Event(2)];
        var root = CallTreeNode.Create(events, EventsCountAggregator.Instance.CreateProcessor(), bottomUp);
        var empty = CallTreeNode.Create(Array.Empty<Event>(), EventsCountAggregator.Instance.CreateProcessor(), bottomUp);

        Assert.Multiple(() =>
        {
            Assert.That(root.Count, Is.EqualTo(3));
            Assert.That(root.Children!.Values.Single().Count, Is.EqualTo(1));
            Assert.That(empty.Count, Is.Zero);
            Assert.That(empty.Children, Is.Null.Or.Empty);
        });
    }

    [TestCase(false)]
    [TestCase(true)]
    public void TenThousandEventsOnTwoSameIndexStackIdentitiesPreserveEveryLeafUpdate(bool bottomUp)
    {
        const int eventCount = 10_000;
        var first = SyntheticEvents.Stack(7, SyntheticEvents.Method("First", 1));
        var second = SyntheticEvents.Stack(7, SyntheticEvents.Method("Second", 2));
        Event[] events = Enumerable.Range(0, eventCount)
            .Select(i => SyntheticEvents.Event(i, i, stack: i % 2 == 0 ? first : second)).ToArray();
        RecordingProcessor processor = new();

        var root = CallTreeNode.Create(events, processor, bottomUp);

        Assert.Multiple(() =>
        {
            Assert.That(processor.Calls, Is.EqualTo(Enumerable.Range(0, eventCount).Reverse()
                .SelectMany(i => new[] { $"process:{i}", $"update:{i}" })));
            Assert.That(root.Count, Is.EqualTo(50_005_000));
            Assert.That(root.Children!.Count, Is.EqualTo(2));
            Assert.That(root.Children[1].Count, Is.EqualTo(25_000_000));
            Assert.That(root.Children[2].Count, Is.EqualTo(25_005_000));
        });
        TestContext.Out.WriteLine($"Leaf workload: {eventCount:N0} events; 2 resolved identities both Index=7; all reverse callbacks; total {root.Count:N0}.");
    }

    [Test]
    public void ReadOnlyListIsVisitedByDescendingIndexWithoutEnumeration()
    {
        IndexedEvents events = new([SyntheticEvents.Event(0), SyntheticEvents.Event(1), SyntheticEvents.Event(2)]);
        RecordingProcessor processor = new();

        var root = CallTreeNode.Create(events, processor, false);

        Assert.Multiple(() =>
        {
            Assert.That(events.VisitedIndexes, Is.EqualTo(new[] { 2, 1, 0 }));
            Assert.That(processor.Calls, Is.EqualTo(new[] { "process:2", "update:2", "process:1", "update:1", "process:0", "update:0" }));
            Assert.That(root.Count, Is.EqualTo(6));
        });
    }

    [Test]
    public void EnumerableFallbackEnumeratesOnceAndPreservesReverseProcessing()
    {
        int enumerations = 0;
        RecordingProcessor processor = new();

        var root = CallTreeNode.Create(Events(), processor, false);

        Assert.Multiple(() =>
        {
            Assert.That(enumerations, Is.EqualTo(1));
            Assert.That(root.Count, Is.EqualTo(6));
            Assert.That(processor.Calls, Is.EqualTo(new[] { "process:2", "update:2", "process:1", "update:1", "process:0", "update:0" }));
        });

        IEnumerable<Event> Events()
        {
            enumerations++;
            for (int i = 0; i < 3; i++) yield return SyntheticEvents.Event(i);
        }
    }

    public static IEnumerable<TestCaseData> Aggregators()
    {
        ICallTreeCountAggregator[] aggregators = [EventsCountAggregator.Instance, AllocationSizeAggregator.Instance,
            ContentionDurationAggregator.Instance, WaitHandleWaitDurationAggregator.Instance,
            TaskDurationAggregator.Instance, TaskWaitDurationAggregator.Instance];
        foreach (var aggregator in aggregators)
        foreach (bool bottomUp in new[] { false, true })
            yield return new TestCaseData(aggregator, bottomUp).SetName($"Aggregates_{aggregator.GetType().Name}_{bottomUp}");
    }

    [TestCaseSource(nameof(Aggregators))]
    public void AllAggregatorsPreserveCountsOnRepeatedStacks(ICallTreeCountAggregator aggregator, bool bottomUp)
    {
        var firstStack = SyntheticEvents.Stack(1, SyntheticEvents.Method("First", 1), SyntheticEvents.Method("Caller", 3));
        // Equal numeric indexes intentionally represent distinct resolved stacks (e.g. different processes).
        var secondStack = SyntheticEvents.Stack(1, SyntheticEvents.Method("Second", 2), firstStack.Frames[1]);
        var stopStack = SyntheticEvents.Stack(3, SyntheticEvents.Method("Stop", 4));
        List<Event> events = [];
        long firstExpected;
        long secondExpected;
        if (aggregator.StopEventKey == null)
        {
            events.Add(SyntheticEvents.Event(0, 10, stack: firstStack, payload: new Dictionary<string, object> { ["ObjectSize"] = 10UL }));
            events.Add(SyntheticEvents.Event(1, 20, stack: secondStack, payload: new Dictionary<string, object> { ["ObjectSize"] = 20UL }));
            events.Add(SyntheticEvents.Event(2, 30, stack: firstStack, payload: new Dictionary<string, object> { ["ObjectSize"] = 30UL }));
            events.Add(SyntheticEvents.Event(3, 40, stack: firstStack));
            firstExpected = aggregator is AllocationSizeAggregator ? 40 : 3;
            secondExpected = aggregator is AllocationSizeAggregator ? 20 : 1;
        }
        else
        {
            // Interleaved threads/task ids, reused correlation and unmatched boundaries detect forward processing
            // and make the two matched intervals on the same cached leaf accumulate independently.
            Add(0, 5, firstStack, false, 1, 1);
            Add(1, 10, firstStack, true, 1, 1);
            Add(2, 20, secondStack, true, 2, 2);
            Add(3, 40, stopStack, false, 2, 2);
            Add(4, 50, stopStack, false, 1, 1);
            Add(5, 60, firstStack, true, 1, 1);
            Add(6, 90, stopStack, false, 1, 1);
            Add(7, 100, secondStack, true, 2, 2);
            firstExpected = 70;
            secondExpected = 20;
        }

        var root = CallTreeNode.Create(events, aggregator.CreateProcessor(), bottomUp);
        var firstLeaf = bottomUp ? root.Children![1].Children![3] : root.Children![3].Children![1];
        var secondLeaf = bottomUp ? root.Children![2].Children![3] : root.Children![3].Children![2];
        Assert.Multiple(() =>
        {
            Assert.That(root.Count, Is.EqualTo(firstExpected + secondExpected));
            Assert.That(firstLeaf.Count, Is.EqualTo(firstExpected));
            Assert.That(secondLeaf.Count, Is.EqualTo(secondExpected));
            if (aggregator.StopEventKey != null)
                Assert.That(root.Children![4].Count, Is.Zero, "Duration belongs to the start event's stack.");
        });

        void Add(int index, long time, StackTrace stack, bool start, long thread, int task) =>
            events.Add(SyntheticEvents.Event(index, time, thread, stack,
                start ? aggregator.StartEventKey : aggregator.StopEventKey,
                new Dictionary<string, object> { ["TaskID"] = task }));
    }

    private sealed class IndexedEvents(Event[] events) : IReadOnlyList<Event>
    {
        public List<int> VisitedIndexes { get; } = [];
        public int Count => events.Length;
        public Event this[int index] { get { VisitedIndexes.Add(index); return events[index]; } }
        public IEnumerator<Event> GetEnumerator() => throw new AssertionException("The IReadOnlyList path must not enumerate/buffer events.");
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private sealed class RecordingProcessor : ICallTreeCountAggregatorProcessor
    {
        public List<string> Calls { get; } = [];
        public void ProcessEvent(Event evt) => Calls.Add($"process:{evt.Index}");
        public void UpdateLeafNode(ref long count, Event evt)
        {
            Calls.Add($"update:{evt.Index}");
            count += evt.Index + 1;
        }
    }
}
