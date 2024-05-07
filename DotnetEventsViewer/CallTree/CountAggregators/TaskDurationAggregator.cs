using DotnetEventsViewer.Models;

namespace DotnetEventsViewer.CallTree.CountAggregators;

internal class TaskDurationAggregator : FieldCorrelationDurationAggregator
{
    public static TaskDurationAggregator Instance { get; } = new();

    public override string Name => "Task Duration";

    public override EventKey StartEventKey { get; } = new(KnownProviders.Tpl, "TaskStarted", 8);

    public override EventKey StopEventKey { get; } = new(KnownProviders.Tpl, "TaskCompleted", 9);

    public override string CorrelationFieldName => "TaskID";
}