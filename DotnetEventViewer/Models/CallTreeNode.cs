using EventPipe;

namespace DotnetEventViewer.Models;

internal class CallTreeNode
{
    public static CallTreeNode Create(IEnumerable<StackTrace> stackTraces)
    {
        CallTreeNode root = new(new MethodDescription("all", ""));

        foreach (var st in stackTraces)
        {
            var currentNode = root;
            for (int i = st.Frames.Length - 1; i >= 0; i -= 1)
            {
                var frame = st.Frames[i];

                currentNode.Children ??= new Dictionary<ulong, CallTreeNode>();

                if (!currentNode.Children.TryGetValue(frame.Address, out var childNode))
                {
                    childNode = new CallTreeNode(frame);
                    currentNode.Children[frame.Address] = childNode;
                }

                currentNode = childNode;
            }
        }

        return root;
    }

    private CallTreeNode(MethodDescription methodDescription)
    {
        MethodDescription = methodDescription;
    }

    public Dictionary<ulong, CallTreeNode>? Children { get; private set; }
    public MethodDescription MethodDescription { get; }

    public override string ToString()
    {
        return MethodDescription.ToString();
    }
}
