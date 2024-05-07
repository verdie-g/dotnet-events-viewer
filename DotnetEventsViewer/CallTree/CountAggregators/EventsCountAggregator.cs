using DotnetEventsViewer.Models;
using EventPipe;

namespace DotnetEventsViewer.CallTree.CountAggregators;

internal class EventsCountAggregator : ICallTreeCountAggregator
{
    public static EventsCountAggregator Instance { get; } = new();

    public string Name => "Events Count";

    public EventKey? StartEventKey => null;

    public EventKey? StopEventKey => null;

    public string Format(long count)
    {
        return count + " events";
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
            count += 1;
        }
    }
}