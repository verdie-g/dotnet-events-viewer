using DotnetEventViewer.Models;

namespace DotnetEventViewer.CallTree.CountAggregators;

internal class ContentionDurationAggregator : SynchronousDurationAggregator
{
    public static ContentionDurationAggregator Instance { get; } = new();

    public override string Name => "Contention Duration";

    public override EventKey StartEventKey { get; } = new(KnownProviders.Runtime, "ContentionStart", 81);

    public override EventKey StopEventKey { get; } = new(KnownProviders.Runtime, "ContentionStop", 91);
}