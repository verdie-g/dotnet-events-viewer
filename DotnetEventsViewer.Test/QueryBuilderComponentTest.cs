using DotnetEventsViewer.CallTree.CountAggregators;
using DotnetEventsViewer.Components;
using DotnetEventsViewer.Querying;
using DotnetEventsViewer.State;
using Microsoft.AspNetCore.Components;
using NUnit.Framework;

// Parameters are supplied to standalone components; only the real submit method is invoked.
#pragma warning disable BL0005

namespace DotnetEventsViewer.Test;

[TestFixture]
public class QueryBuilderComponentTest
{
    [Test]
    public async Task ValidSubmitReturnsIncompleteCallbackTaskAndPreservesUnfilteredQueryResult()
    {
        var trace = SyntheticEvents.Trace(SyntheticEvents.Event(0), SyntheticEvents.Event(1));
        Query query = new()
        {
            SelectedColumnFields = new[] { Field.ThreadIdField },
            SelectedAggregator = EventsCountAggregator.Instance,
            BottomUpTree = false
        };
        QueryBuilder builder = Create(new StateContainer { Trace = trace, Query = query }, query);
        TaskCompletionSource completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        QueryResult? received = null;
        int callbackCount = 0;
        builder.OnQuery = new EventCallbackFactory().Create<QueryResult>(new object(), result =>
        {
            received = result;
            callbackCount++;
            return completion.Task;
        });

        try
        {
            Task submitted = ComponentTestAccess.Invoke<Task>(builder, "HandleValidSubmit");

            Assert.Multiple(() =>
            {
                Assert.That(submitted, Is.SameAs(completion.Task), "Submit must return the callback task, not discard it.");
                Assert.That(submitted.IsCompleted, Is.False, "An unfinished callback must keep submit unfinished.");
                Assert.That(callbackCount, Is.EqualTo(1));
                Assert.That(received, Is.Not.Null);
                Assert.That(received!.FilteredEvents, Is.SameAs(trace.Events));
                Assert.That(received.ColumnFields, Is.SameAs(query.SelectedColumnFields));
                Assert.That(received.CallTreeAggregator, Is.SameAs(query.SelectedAggregator));
                Assert.That(received.BottomUpTree, Is.False);
            });

            completion.SetResult();
            await submitted;
            Assert.That(submitted.IsCompletedSuccessfully, Is.True);
        }
        finally
        {
            completion.TrySetResult();
        }
    }

    [Test]
    public async Task ValidSubmitPropagatesAsynchronousCallbackException()
    {
        Query query = new();
        QueryBuilder builder = Create(new StateContainer { Trace = SyntheticEvents.Trace(), Query = query }, query);
        TaskCompletionSource completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        builder.OnQuery = new EventCallbackFactory().Create<QueryResult>(new object(), _ => completion.Task);
        InvalidOperationException expected = new("query callback failed");

        Task submitted = ComponentTestAccess.Invoke<Task>(builder, "HandleValidSubmit");
        try
        {
            Assert.That(submitted.IsCompleted, Is.False);
            completion.SetException(expected);

            var actual = await Assert.ThrowsAsync<InvalidOperationException>(async () => await submitted);
            Assert.That(actual, Is.SameAs(expected), "The caller must observe the original asynchronous callback failure.");
        }
        finally
        {
            completion.TrySetResult();
        }
    }

    private static QueryBuilder Create(StateContainer state, Query query)
    {
        QueryBuilder builder = new() { QueryType = QueryType.List };
        ComponentTestAccess.SetProperty(builder, "State", state);
        ComponentTestAccess.SetProperty(builder, "Query", query);
        return builder;
    }
}
