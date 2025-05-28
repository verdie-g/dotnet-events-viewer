using DotnetEventsViewer.CallTree.CountAggregators;
using NetTrace;

namespace DotnetEventsViewer.CallTree;

public class CallTreeNode
{
    public static CallTreeNode Create(
        IEnumerable<Event> events,
        ICallTreeCountAggregatorProcessor processor,
        bool bottomUp)
    {
        CallTreeNode root = new(0, new MethodDescription("root", ""));

        int id = 1;
        // Process in reverse order so that ProcessEvent observes a stop event before its associated start event.
        foreach (var evt in events.Reverse())
        {
            processor.ProcessEvent(evt);

            var currentNode = root;

            if (bottomUp)
            {
                for (int i = 0; i < evt.StackTrace.Frames.Length; i += 1)
                {
                    AddCallToNode(evt.StackTrace.Frames[i], ref currentNode, ref id);
                }
            }
            else
            {
                for (int i = evt.StackTrace.Frames.Length - 1; i >= 0; i -= 1)
                {
                    AddCallToNode(evt.StackTrace.Frames[i], ref currentNode, ref id);
                }
            }

            processor.UpdateLeafNode(ref currentNode._count, evt);
        }

        root.PropagateCount();

        return root;

        static void AddCallToNode(MethodDescription frame, ref CallTreeNode node, ref int id)
        {
            node.Children ??= new Dictionary<ulong, CallTreeNode>();
            if (!node.Children.TryGetValue(frame.Address, out var childNode))
            {
                childNode = new CallTreeNode(id, frame);
                node.Children[frame.Address] = childNode;
                id += 1;
            }

            node = childNode;
        }
    }

    private long _count;

    private CallTreeNode(int id, MethodDescription methodDescription)
    {
        Id = id;
        MethodDescription = methodDescription;
    }

    public int Id { get; }
    public Dictionary<ulong, CallTreeNode>? Children { get; private set; }
    public MethodDescription MethodDescription { get; }
    public long Count => _count;

    public override string ToString()
    {
        return MethodDescription.ToString();
    }

    private long PropagateCount()
    {
        if (Children != null)
        {
            foreach (var child in Children)
            {
                _count += child.Value.PropagateCount();
            }
        }

        return _count;
    }
}