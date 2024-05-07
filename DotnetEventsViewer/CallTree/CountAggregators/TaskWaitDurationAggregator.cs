using DotnetEventsViewer.Models;

namespace DotnetEventsViewer.CallTree.CountAggregators;

internal class TaskWaitDurationAggregator : FieldCorrelationDurationAggregator
{
    public static TaskWaitDurationAggregator Instance { get; } = new();

    public override string Name => "Task Wait Duration";

    public override EventKey StartEventKey { get; } = new(KnownProviders.Tpl, "TaskWaitBegin", 10);

    public override EventKey StopEventKey { get; } = new(KnownProviders.Tpl, "TaskWaitEnd", 11);

    public override string CorrelationFieldName => "TaskID";
}