using EventPipe;

namespace DotnetEventViewer.CallTree.Decorators;

/// <summary>
/// Adds metadata (e.g. events count) to a <see cref="CallTreeNode"/>.
/// </summary>
public interface ICallTreeNodeDecorator
{
    string Name { get; }

    /// <summary>
    /// The events this decorator is compatible with. Use null to indicates it's compatible with all events.
    /// </summary>
    ISet<string>? CompatibleEventNames { get; }

    string Format(long count);

    /// <summary>
    /// A method called for each event. Can be used to build correlations between Start and Stop events.
    /// </summary>
    void ProcessEvent(Event evt);

    /// <summary>
    /// A method called for each event on leaf nodes. This method is called only after all events were processed
    /// with <see cref="ProcessEvent"/>.
    /// </summary>
    void UpdateLeafNode(ref long count, Event evt);
}
