namespace DotnetEventViewer.CallTree.CountAggregators;

/// <summary>
/// Adds metadata (e.g. events count) to a <see cref="CallTreeNode"/>.
/// </summary>
public interface ICallTreeCountAggregator
{
    string Name { get; }

    /// <summary>
    /// The events this aggregator is compatible with. Use null to indicates it is compatible with all events.
    /// </summary>
    ISet<string>? CompatibleEventNames { get; }

    string Format(long count);

    ICallTreeCountAggregatorProcessor CreateProcessor();
}
