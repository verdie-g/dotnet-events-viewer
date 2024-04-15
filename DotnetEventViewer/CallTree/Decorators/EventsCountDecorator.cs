using EventPipe;

namespace DotnetEventViewer.CallTree.Decorators;

internal class EventsCountDecorator : ICallTreeNodeDecorator
{
    public IFormatProvider? Format { get; }

    public string Unit => "events";

    public void ProcessEvent(Event evt)
    {
    }

    public void UpdateLeafNode(ref long count, Event evt)
    {
        count += 1;
    }
}