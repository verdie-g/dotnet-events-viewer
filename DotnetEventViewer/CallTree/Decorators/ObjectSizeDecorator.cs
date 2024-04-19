using EventPipe;

namespace DotnetEventViewer.CallTree.Decorators;

internal class ObjectSizeDecorator : ICallTreeNodeDecorator
{
    public ISet<string>? CompatibleEventNames { get; } = new HashSet<string> { "GCAllocationTick" };

    public string Format(long count)
    {
        return count switch
        {
            > 1_000_000 => $"{count / 1_000_000f:0.00} MB",
            > 1_000 => $"{count / 1_000f:0.00} KB",
            _ => $"{count} B",
        };
    }

    public void ProcessEvent(Event evt)
    {
    }

    public void UpdateLeafNode(ref long count, Event evt)
    {
        if (evt.Payload.TryGetValue("ObjectSize", out object? objectSize))
        {
            count += (long)(ulong)objectSize;
        }
    }
}