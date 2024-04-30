namespace DotnetEventViewer.CallTree.CountAggregators;

internal class ContentionDurationAggregator : SynchronousDurationAggregator
{
    public static ContentionDurationAggregator Instance { get; } = new();

    private ContentionDurationAggregator()
        : base(81, 91)
    {
    }

    public override string Name => "Contention Duration";

    public override ISet<string>? CompatibleEventNames { get; } = new HashSet<string> { "ContentionStart" };
}