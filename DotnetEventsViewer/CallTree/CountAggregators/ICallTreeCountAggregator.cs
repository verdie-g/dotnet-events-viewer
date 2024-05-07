using DotnetEventViewer.Models;

namespace DotnetEventViewer.CallTree.CountAggregators;

/// <summary>
/// Adds metadata (e.g. events count) to a <see cref="CallTreeNode"/>.
/// </summary>
public interface ICallTreeCountAggregator
{
    string Name { get; }

    EventKey? StartEventKey { get; }
    EventKey? StopEventKey { get; }

    string Format(long count);

    ICallTreeCountAggregatorProcessor CreateProcessor();
}
