namespace DotnetEventViewer.CallTree.CountAggregators;

internal class WaitHandleWaitDurationAggregator : SynchronousDurationAggregator
{
    public static WaitHandleWaitDurationAggregator Instance { get; } = new();

    private WaitHandleWaitDurationAggregator()
        : base(301, 302)
    {
    }

    public override string Name => "Wait Duration";

    public override ISet<string>? CompatibleEventNames { get; } = new HashSet<string> { "WaitHandleWaitStart" };
}