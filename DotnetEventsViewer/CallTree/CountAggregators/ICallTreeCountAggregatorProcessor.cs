using NetTrace;

namespace DotnetEventsViewer.CallTree.CountAggregators;

public interface ICallTreeCountAggregatorProcessor
{
    /// <summary>
    /// A method called for each event ordered by descending timestamp. It can be used to build correlations between
    /// Start and Stop events.
    /// </summary>
    /// <param name="evt"></param>
    void ProcessEvent(Event evt);

    /// <summary>
    /// A method called for each event on leaf nodes.
    /// </summary>
    void UpdateLeafNode(ref long count, Event evt);
}
