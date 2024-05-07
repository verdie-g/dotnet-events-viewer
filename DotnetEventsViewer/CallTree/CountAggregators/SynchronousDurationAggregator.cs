using DotnetEventsViewer.Models;
using EventPipe;

namespace DotnetEventsViewer.CallTree.CountAggregators;

internal abstract class SynchronousDurationAggregator : ICallTreeCountAggregator
{
    public abstract string Name { get; }

    public abstract EventKey StartEventKey { get; }

    public abstract EventKey StopEventKey { get; }

    public string Format(long count)
    {
        return count switch
        {
            > 1_000_000_000 => $"{count / 1_000_000_000f:0.00} s",
            > 1_000_000 => $"{count / 1_000_000f:0.00} ms",
            > 1_000 => $"{count / 1_000f:0.00} μs",
            _ => $"{count} ns",
        };
    }

    public ICallTreeCountAggregatorProcessor CreateProcessor()
    {
        return new Processor(StartEventKey, StopEventKey);
    }

    private class Processor(EventKey startEventKey, EventKey stopEventKey) : ICallTreeCountAggregatorProcessor
    {
        private readonly Dictionary<long, Event> _threadToStopEvent = new();
        private readonly Dictionary<int, long> _countByEventIndex = new();

        public void ProcessEvent(Event evt)
        {
            if (stopEventKey.Matches(evt))
            {
                _threadToStopEvent[evt.ThreadId] = evt;
            }
            else if (startEventKey.Matches(evt))
            {
                if (_threadToStopEvent.Remove(evt.ThreadId, out var stopEvt))
                {
                    _countByEventIndex.TryGetValue(evt.Index, out long count);
                    _countByEventIndex[evt.Index] = count - evt.TimeStamp + stopEvt.TimeStamp;
                }
            }
        }

        public void UpdateLeafNode(ref long count, Event evt)
        {
            if (_countByEventIndex.TryGetValue(evt.Index, out long c))
            {
                count += c;
            }
        }
    }
}