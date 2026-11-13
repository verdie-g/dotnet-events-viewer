using System.Reflection;
using DotnetEventsViewer.CallTree;
using DotnetEventsViewer.CallTree.CountAggregators;
using DotnetEventsViewer.Models;
using Microsoft.FluentUI.AspNetCore.Components;
using NetTrace;
using NUnit.Framework;

namespace DotnetEventsViewer.Test;

[TestFixture]
public class CallTreeViewTest
{
    [Test]
    public void ConstructionFormatsOnlyRootAndExpansionLoadsOnlyImmediateChildren()
    {
        var root = BranchedTree();
        CountingAggregator aggregator = new();

        CallTreeView view = new(root, aggregator);

        Assert.Multiple(() =>
        {
            Assert.That(aggregator.FormattedCounts, Is.EqualTo(new long[] { 6 }));
            Assert.That(view.Root.Id, Is.EqualTo("0"));
            Assert.That(view.Root.Expanded, Is.False);
            Assert.That(view.Root.Items!.Count(), Is.EqualTo(1), "Collapsed root has a marker, not its actual children.");
        });
        var rootMarker = view.Root.Items;
        view.Expand(view.Root);
        var children = view.Root.Items!.ToArray();
        Assert.Multiple(() =>
        {
            Assert.That(aggregator.FormattedCounts, Is.EqualTo(new long[] { 6, 4, 2 }));
            Assert.That(children.Select(item => item.Id), Is.EqualTo(root.Children!.Values.OrderByDescending(node => node.Count).Select(node => node.Id.ToString())));
            Assert.That(children.Select(item => item.Expanded), Is.All.False, "Sibling branches must stay collapsed.");
            Assert.That(children[0].Items, Is.SameAs(children[1].Items), "Collapsed siblings share a marker without constructing descendants.");
            Assert.That(children[0].Items, Is.SameAs(rootMarker));
        });

        var untouchedMarker = children[1].Items;
        view.Expand(children[0]);
        Assert.Multiple(() =>
        {
            Assert.That(aggregator.FormattedCounts, Is.EqualTo(new long[] { 6, 4, 2, 3, 1 }));
            Assert.That(children[0].Items!.Select(item => item.Expanded), Is.All.False);
            Assert.That(children[1].Items, Is.SameAs(untouchedMarker), "Expanding one branch must not load its sibling.");
        });
    }

    [Test]
    public void ReexpansionRetainsChildCollectionsAndItemIdentitiesWithoutReformatting()
    {
        CountingAggregator aggregator = new();
        CallTreeView view = new(BranchedTree(), aggregator);
        view.Expand(view.Root);
        var rootItems = view.Root.Items;
        var branch = rootItems!.First();
        view.Expand(branch);
        var branchItems = branch.Items;
        var grandchild = branchItems!.First();
        int formatted = aggregator.FormattedCounts.Count;
        branch.Expanded = false;
        view.Root.Expanded = false;

        view.Expand(view.Root);
        view.Expand(branch);

        Assert.Multiple(() =>
        {
            Assert.That(view.Root.Items, Is.SameAs(rootItems));
            Assert.That(view.Root.Items!.First(), Is.SameAs(branch));
            Assert.That(branch.Items, Is.SameAs(branchItems));
            Assert.That(branch.Items!.First(), Is.SameAs(grandchild));
            Assert.That(aggregator.FormattedCounts.Count, Is.EqualTo(formatted));
        });
    }

    [Test]
    public void NewViewHasFreshExpansionStateAndUsesItsOwnAggregator()
    {
        var root = BranchedTree();
        CountingAggregator firstAggregator = new();
        CallTreeView first = new(root, firstAggregator);
        first.Expand(first.Root);
        first.Expand(first.Root.Items!.First());
        int firstCalls = firstAggregator.FormattedCounts.Count;
        CountingAggregator secondAggregator = new();

        CallTreeView second = new(root, secondAggregator);
        Assert.Multiple(() =>
        {
            Assert.That(second.Root, Is.Not.SameAs(first.Root));
            Assert.That(second.Root.Expanded, Is.False);
            Assert.That(secondAggregator.FormattedCounts, Is.EqualTo(new long[] { 6 }));
        });
        second.Expand(second.Root);
        Assert.Multiple(() =>
        {
            Assert.That(second.Root.Items, Is.Not.SameAs(first.Root.Items));
            Assert.That(second.Root.Items!.First(), Is.Not.SameAs(first.Root.Items!.First()));
            Assert.That(second.Root.Items!.Select(item => item.Expanded), Is.All.False);
            Assert.That(secondAggregator.FormattedCounts, Is.EqualTo(new long[] { 6, 4, 2 }));
            Assert.That(firstAggregator.FormattedCounts.Count, Is.EqualTo(firstCalls));
        });
    }

    [Test]
    public void SingleChildChainsAutoexpandThroughLastLeafOnlyAfterExplicitExpansion()
    {
        var stack = SyntheticEvents.Stack(1, SyntheticEvents.Method("Leaf", 1),
            SyntheticEvents.Method("Middle", 2), SyntheticEvents.Method("Outer", 3));
        var root = CallTreeNode.Create([SyntheticEvents.Event(0, stack: stack)],
            EventsCountAggregator.Instance.CreateProcessor(), false);
        CountingAggregator aggregator = new();
        CallTreeView view = new(root, aggregator);
        Assert.That(aggregator.FormattedCounts.Count, Is.EqualTo(1));

        view.Expand(view.Root);
        List<ITreeViewItem> chain = [view.Root];
        while (chain[^1].Items?.Any() == true) chain.Add(chain[^1].Items!.Single());

        Assert.Multiple(() =>
        {
            Assert.That(chain.Count, Is.EqualTo(4));
            Assert.That(chain.Select(item => item.Expanded), Is.All.True, "Prior behavior includes the last leaf.");
            Assert.That(aggregator.FormattedCounts, Is.EqualTo(new long[] { 1, 1, 1, 1 }));
            Assert.That(chain[^1].Items, Is.Null.Or.Empty);
        });
        view.Expand(view.Root);
        Assert.That(aggregator.FormattedCounts.Count, Is.EqualTo(4));
    }

    [Test]
    public void SingleChildAutoexpansionStopsAtBranchWithoutLoadingGrandchildren()
    {
        var common = SyntheticEvents.Method("Common", 10);
        var first = SyntheticEvents.Stack(1, SyntheticEvents.Method("First", 1), common);
        var second = SyntheticEvents.Stack(2, SyntheticEvents.Method("Second", 2), common);
        var root = CallTreeNode.Create([SyntheticEvents.Event(0, stack: first), SyntheticEvents.Event(1, stack: second)],
            EventsCountAggregator.Instance.CreateProcessor(), false);
        CountingAggregator aggregator = new();
        CallTreeView view = new(root, aggregator);

        view.Expand(view.Root);
        var commonItem = view.Root.Items!.Single();

        Assert.Multiple(() =>
        {
            Assert.That(commonItem.Expanded, Is.True);
            Assert.That(commonItem.Items!.Count(), Is.EqualTo(2));
            Assert.That(commonItem.Items!.Select(item => item.Expanded), Is.All.False);
            Assert.That(aggregator.FormattedCounts.Count, Is.EqualTo(4));
        });
    }

    [Test]
    public void SiblingOrderingIsStableDescendingByCountAndLabelsUseRootRatio()
    {
        var first = SyntheticEvents.Stack(1, SyntheticEvents.Method("First", 1));
        var second = SyntheticEvents.Stack(2, SyntheticEvents.Method("Second", 2));
        var larger = SyntheticEvents.Stack(3, SyntheticEvents.Method("Larger", 3));
        // Reverse event traversal inserts Larger, Second, First; equal counts must keep that insertion order.
        Event[] events = [SyntheticEvents.Event(0, stack: first), SyntheticEvents.Event(1, stack: second),
            SyntheticEvents.Event(2, stack: larger), SyntheticEvents.Event(3, stack: larger)];
        var root = CallTreeNode.Create(events, EventsCountAggregator.Instance.CreateProcessor(), false);
        CallTreeView view = new(root, new CountingAggregator());

        view.Expand(view.Root);
        var actual = view.Root.Items!.ToArray();
        var ordered = root.Children!.Values.OrderByDescending(node => node.Count).ToArray();
        Assert.Multiple(() =>
        {
            Assert.That(actual.Select(item => item.Id), Is.EqualTo(ordered.Select(node => node.Id.ToString())));
            Assert.That(ordered.Select(node => node.MethodDescription.Name), Is.EqualTo(new[] { "Larger", "Second", "First" }));
            for (int i = 0; i < ordered.Length; i++)
            {
                float ratio = ordered[i].Count / (float)root.Count;
                Assert.That(actual[i].Text, Is.EqualTo($"({100 * ratio:0.00}% - {ordered[i].Count} units) {ordered[i].MethodDescription}"));
            }
        });
    }

    [Test]
    public void EmptyRootLabelIsFiniteAndExpansionDoesNotCreateChildren()
    {
        var root = CallTreeNode.Create(Array.Empty<Event>(), EventsCountAggregator.Instance.CreateProcessor(), false);
        CountingAggregator aggregator = new();
        CallTreeView view = new(root, aggregator);
        view.Expand(view.Root);
        view.Expand(view.Root);
        Assert.Multiple(() =>
        {
            Assert.That(view.Root.Text, Is.EqualTo($"({0f:0.00}% - 0 units) root"));
            Assert.That(view.Root.Text, Does.Not.Contain("NaN").And.Not.Contain("Infinity"));
            Assert.That(view.Root.Items, Is.Null.Or.Empty);
            Assert.That(aggregator.FormattedCounts, Is.EqualTo(new long[] { 0 }));
        });
    }

    [Test]
    public async Task ExpansionCallbackLoadsChildrenButCollapseDoesNot()
    {
        CountingAggregator aggregator = new();
        CallTreeView view = new(BranchedTree(), aggregator);
        var marker = view.Root.Items;
        Assert.That(view.Root.OnExpandedAsync, Is.Not.Null);

        await view.Root.OnExpandedAsync!(ExpansionArgs(view.Root, false));
        Assert.That(view.Root.Items, Is.SameAs(marker));
        Assert.That(aggregator.FormattedCounts.Count, Is.EqualTo(1));
        await view.Root.OnExpandedAsync!(ExpansionArgs(view.Root, true));
        Assert.That(view.Root.Items!.Count(), Is.EqualTo(2));
        Assert.That(aggregator.FormattedCounts.Count, Is.EqualTo(3));
    }

    private static TreeViewItemExpandedEventArgs ExpansionArgs(ITreeViewItem item, bool expanded) =>
        (TreeViewItemExpandedEventArgs)typeof(TreeViewItemExpandedEventArgs).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic, null, [typeof(ITreeViewItem), typeof(bool)], null)!
            .Invoke([item, expanded]);

    private static CallTreeNode BranchedTree()
    {
        var left = SyntheticEvents.Method("Left", 10);
        var right = SyntheticEvents.Method("Right", 20);
        var leftFirst = SyntheticEvents.Stack(1, SyntheticEvents.Method("LeftFirst", 1), left);
        var leftSecond = SyntheticEvents.Stack(2, SyntheticEvents.Method("LeftSecond", 2), left);
        var rightFirst = SyntheticEvents.Stack(3, SyntheticEvents.Method("RightFirst", 3), right);
        var rightSecond = SyntheticEvents.Stack(4, SyntheticEvents.Method("RightSecond", 4), right);
        Event[] events = [SyntheticEvents.Event(0, stack: leftFirst), SyntheticEvents.Event(1, stack: leftFirst),
            SyntheticEvents.Event(2, stack: leftFirst), SyntheticEvents.Event(3, stack: leftSecond),
            SyntheticEvents.Event(4, stack: rightFirst), SyntheticEvents.Event(5, stack: rightSecond)];
        return CallTreeNode.Create(events, EventsCountAggregator.Instance.CreateProcessor(), false);
    }

    private sealed class CountingAggregator : ICallTreeCountAggregator
    {
        public List<long> FormattedCounts { get; } = [];
        public string Name => "Test counts";
        public EventKey? StartEventKey => null;
        public EventKey? StopEventKey => null;
        public string Format(long count) { FormattedCounts.Add(count); return $"{count} units"; }
        public ICallTreeCountAggregatorProcessor CreateProcessor() => EventsCountAggregator.Instance.CreateProcessor();
    }
}
