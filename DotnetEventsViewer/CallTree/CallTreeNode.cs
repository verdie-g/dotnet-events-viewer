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
        var eventList = events as IReadOnlyList<Event> ?? events.ToArray();
        Dictionary<NetTrace.StackTrace, CallTreeNode> leaves = new(ReferenceEqualityComparer.Instance);
        // Process in reverse order so that ProcessEvent observes a stop event before its associated start event.
        for (int eventIndex = eventList.Count - 1; eventIndex >= 0; eventIndex -= 1)
        {
            var evt = eventList[eventIndex];
            processor.ProcessEvent(evt);

            if (!leaves.TryGetValue(evt.StackTrace, out var currentNode))
            {
                currentNode = root;
                var frames = evt.StackTrace.Frames;
                if (bottomUp)
                {
                    for (int i = 0; i < frames.Length; i += 1)
                        AddCallToNode(frames[i], ref currentNode, ref id);
                }
                else
                {
                    for (int i = frames.Length - 1; i >= 0; i -= 1)
                        AddCallToNode(frames[i], ref currentNode, ref id);
                }
                leaves.Add(evt.StackTrace, currentNode);
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