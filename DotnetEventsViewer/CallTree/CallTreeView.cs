using DotnetEventsViewer.CallTree.CountAggregators;
using Microsoft.FluentUI.AspNetCore.Components;

namespace DotnetEventsViewer.CallTree;

/// <summary>Creates presentation nodes only for expanded branches, retaining them on collapse.</summary>
internal sealed class CallTreeView
{
    private readonly CallTreeNode _root;
    private readonly ICallTreeCountAggregator _aggregator;

    public CallTreeView(CallTreeNode root, ICallTreeCountAggregator aggregator)
    {
        _root = root;
        _aggregator = aggregator;
        Root = CreateItem(root);
    }

    public ITreeViewItem Root { get; }

    public void Expand(ITreeViewItem item)
    {
        var current = (NodeItem)item;
        current.Expanded = true;
        while (true)
        {
            LoadChildren(current);
            if (current.Items is not NodeItem[] { Length: 1 } children) break;
            current = children[0];
            current.Expanded = true;
        }
    }

    private NodeItem CreateItem(CallTreeNode node)
    {
        float ratio = _root.Count == 0 ? 0 : node.Count / (float)_root.Count;
        return new NodeItem(node)
        {
            Id = node.Id.ToString(),
            Text = $"({100 * ratio:0.00}% - {_aggregator.Format(node.Count)}) {node.MethodDescription}",
            OnExpandedAsync = e =>
            {
                if (e.Expanded) Expand(e.CurrentItem);
                return Task.CompletedTask;
            }
        };
    }

    private void LoadChildren(NodeItem item)
    {
        if (item.ChildrenLoaded) return;
        item.Items = item.Node.Children?.Values.OrderByDescending(node => node.Count).Select(CreateItem).ToArray();
        item.ChildrenLoaded = true;
    }

    private sealed class NodeItem : TreeViewItem
    {
        // FluentTreeView requires a non-empty Items collection to show a collapsed chevron.
        // LazyLoadItems prevents this marker from rendering; expansion replaces it before rendering children.
        private static readonly ITreeViewItem[] UnloadedChildren = [new TreeViewItem()];

        public NodeItem(CallTreeNode node)
        {
            Node = node;
            Items = node.Children is { Count: > 0 } ? UnloadedChildren : null;
        }

        public CallTreeNode Node { get; }
        public bool ChildrenLoaded { get; set; }
    }
}
