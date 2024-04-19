using EventPipe;

namespace DotnetEventViewer.CallTree.Decorators;

internal class EventsCountDecorator : ICallTreeNodeDecorator
{
    public static EventsCountDecorator Instance { get; } = new();

    public string Name => "Events Count";

    public ISet<string>? CompatibleEventNames => null;

    public string Format(long count)
    {
        return count + " events";
    }

    public void ProcessEvent(Event evt)
    {
    }

    public void UpdateLeafNode(ref long count, Event evt)
    {
        count += 1;
    }
}