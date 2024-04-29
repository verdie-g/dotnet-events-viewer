using EventPipe;

namespace DotnetEventViewer.CallTree.CountAggregators;

internal class AllocationSizeAggregator : ICallTreeCountAggregator
{
    public static AllocationSizeAggregator Instance { get; } = new();

    public string Name => "Allocation Size";

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

    public ICallTreeCountAggregatorProcessor CreateProcessor()
    {
        return new Processor();
    }

    private class Processor : ICallTreeCountAggregatorProcessor
    {
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
}