using DotnetEventViewer.Models;

namespace DotnetEventViewer.CallTree.CountAggregators;

internal class WaitHandleWaitDurationAggregator : SynchronousDurationAggregator
{
    public static WaitHandleWaitDurationAggregator Instance { get; } = new();

    public override string Name => "Wait Duration";

    public override EventKey StartEventKey { get; } = new(KnownProviders.Runtime, "WaitHandleWaitStart", 301);

    public override EventKey StopEventKey { get; } = new(KnownProviders.Runtime, "WaitHandleWaitStop", 302);
}