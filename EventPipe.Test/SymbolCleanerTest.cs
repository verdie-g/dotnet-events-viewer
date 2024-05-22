using NUnit.Framework.Interfaces;

namespace EventPipe.Test;

public class SymbolCleanerTest
{
    [TestCaseSource(nameof(TestCaseSource))]
    public string Test(string ns, string name, string signature)
    {
        return SymbolCleaner.Clean(ns, name, signature);
    }

    private static IEnumerable<TestCaseData> TestCaseSource()
    {
        yield return new TestCaseData(
            "System.Collections.Concurrent.ConcurrentDictionary`2[System.__Canon,System.__Canon]",
            "TryAddInternal",
            "instance bool (!0,value class System.Nullable`1<int32>,!1,bool,bool,!1&)")
        {
            ExpectedResult =
                "System.Collections.Concurrent.ConcurrentDictionary<T, T>.TryAddInternal(T, System.Nullable<int32>, T, bool, bool, T&)",
        };
        yield return new TestCaseData(
            "Contoso.Cookies.CookieSet+<GetEntryAsync>d__24",
            "MoveNext",
            "instance void ()")
        {
            ExpectedResult = "Contoso.Cookies.CookieSet+<GetEntryAsync>d__24.MoveNext()",
        };
        yield return new TestCaseData(
            "System.Threading.ExecutionContext",
            "RunInternal",
            "void (class System.Threading.ExecutionContext,class System.Threading.ContextCallback,class System.Object)")
        {
            ExpectedResult =
                "System.Threading.ExecutionContext.RunInternal(System.Threading.ExecutionContext, System.Threading.ContextCallback, System.Object)",
        };
        yield return new TestCaseData(
            "System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1+AsyncStateMachineBox`1[Contoso.Cookies.Cookies.EntryFetched,Contoso.Cookies.CookieSet+<GetEntryAsync>d__24]",
            "MoveNext",
            "instance void (class System.Threading.Thread)")
        {
            ExpectedResult =
                "System.Runtime.CompilerServices.AsyncTaskMethodBuilder<T>+AsyncStateMachineBox<Contoso.Cookies.Cookies.EntryFetched, Contoso.Cookies.CookieSet+<GetEntryAsync>d__24>.MoveNext(System.Threading.Thread)",
        };
        yield return new TestCaseData(
            "System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1+AsyncStateMachineBox`1[System.ValueTuple`2[Sdk.Interfaces.KeyValueStore.ReturnCode,System.__Canon],Contoso.KeyValueStore.Storage.BucketStorage+<Sdk-Interfaces-KeyValueStore-IAsyncStorage-GetAsync>d__17]",
            "MoveNext",
            "instance void (class System.Threading.Thread)")
        {
            ExpectedResult =
                "System.Runtime.CompilerServices.AsyncTaskMethodBuilder<T>+AsyncStateMachineBox<System.ValueTuple<Sdk.Interfaces.KeyValueStore.ReturnCode, T>, Contoso.KeyValueStore.Storage.BucketStorage+<Sdk-Interfaces-KeyValueStore-IAsyncStorage-GetAsync>d__17>.MoveNext(System.Threading.Thread)",
        };
        yield return new TestCaseData(
            "System.Threading.Tasks.AwaitTaskContinuation",
            "RunOrScheduleAction",
            "void (class System.Runtime.CompilerServices.IAsyncStateMachineBox,bool)")
        {
            ExpectedResult =
                "System.Threading.Tasks.AwaitTaskContinuation.RunOrScheduleAction(System.Runtime.CompilerServices.IAsyncStateMachineBox, bool)",
        };
        yield return new TestCaseData(
            "System.Threading.PortableThreadPool+WorkerThread",
            "WorkerThreadStart",
            "void ()")
        {
            ExpectedResult = "System.Threading.PortableThreadPool+WorkerThread.WorkerThreadStart()",
        };
        yield return new TestCaseData(
            "Contoso.Features.Data.FeatureAggregationCookieService",
            ".ctor",
            "instance void (class Contoso.ConfigAsCode.IConfigAsCodeService)")
        {
            ExpectedResult =
                "Contoso.Features.Data.FeatureAggregationCookieService..ctor(Contoso.ConfigAsCode.IConfigAsCodeService)",
        };
        yield return new TestCaseData(
            "System.Diagnostics.Tracing.EventPipeEventProvider",
            "EventWriteTransfer",
            "instance value class WriteEventErrorCode (required_modifier System.Runtime.InteropServices.InAttribute value class System.Diagnostics.Tracing.EventDescriptor&,int,value class System.Guid*,value class System.Guid*,int32,value class EventData*)")
        {
            ExpectedResult =
                "System.Diagnostics.Tracing.EventPipeEventProvider.EventWriteTransfer(System.Diagnostics.Tracing.EventDescriptor&, int, System.Guid*, System.Guid*, int32, EventData*)",
        };
        yield return new TestCaseData(
            "Contoso.Cookies.CookieEntry",
            "TryGet",
            "instance generic value class Contoso.Cookies.EntryGetResult (!!0&,class System.Exception&,int32,bool)")
        {
            ExpectedResult = "Contoso.Cookies.CookieEntry.TryGet(T&, System.Exception&, int32, bool)",
        };
        yield return new TestCaseData(
            "System.Threading.Tasks.Task+DelayPromiseWithCancellation+<>c",
            "<.ctor>b__1_0",
            "instance void (class System.Object,value class System.Threading.CancellationToken)")
        {
            ExpectedResult = "System.Threading.Tasks.Task+DelayPromiseWithCancellation+<>c.<.ctor>b__1_0(System.Object, System.Threading.CancellationToken)",
        };
        yield return new TestCaseData(
            "System.Net.Http.HttpClient+<<SendAsync>g__Core|83_0>d",
            "MoveNext",
            "instance void ()")
        {
            ExpectedResult = "System.Net.Http.HttpClient+<<SendAsync>g__Core|83_0>d.MoveNext()",
        };
        yield return new TestCaseData(
            "Sdk.Connection.GhostClientBase`2+<Process>d__42[System.__Canon,System.__Canon]",
            "MoveNext",
            "instance void ()")
        {
            ExpectedResult = "Sdk.Connection.GhostClientBase<T, T>+<Process>d__42<T, T>.MoveNext()",
            RunState = RunState.Ignored,
        };
        yield return new TestCaseData(
            "Ghost.Core.ComponentMiddleware`4+<Invoke>d__5[System.__Canon,System.__Canon,System.__Canon,Contoso.RTB.Common.SerializedData]",
            "MoveNext",
            "instance void ()")
        {
            ExpectedResult = "Ghost.Core.ComponentMiddleware<T, T, T, T>+<Invoke>d__5<T, T, T, T>.MoveNext()",
            RunState = RunState.Ignored,
        };
        yield return new TestCaseData(
            "Ghost.Transports.Http.Common.Middlewares.HttpExceptionMiddleware",
            "Invoke",
            "instance class System.Threading.Tasks.Task`1<value class Ghost.Contract.InvocationResult> (class Ghost.Transports.Http.Contract.IHttpRequest,class Ghost.Transports.Http.Contract.IHttpResponse,value class System.Threading.CancellationToken)")
        {
            ExpectedResult = "Ghost.Transports.Http.Common.Middlewares.HttpExceptionMiddleware.Invoke(Ghost.Transports.Http.Contract.IHttpRequest, Ghost.Transports.Http.Contract.IHttpResponse, System.Threading.CancellationToken)",
        };
        yield return new TestCaseData(
            "LZ4.LZ4Stream",
            "Write",
            "instance void  (unsigned int8[],int32,int32)")
        {
            ExpectedResult = "LZ4.LZ4Stream.Write(unsigned int8[], int32, int32)",
        };
        yield return new TestCaseData(
            "Contoso.API.Versatile.Functions.FlattenedFunction",
            "Evaluate",
            "instance value class Criteo.API.Versatile.EnumerableVariant  (class System.Collections.Generic.IEnumerable`1<value class Criteo.API.Versatile.EnumerableVariant>)")
        {
            ExpectedResult = "Contoso.API.Versatile.Functions.FlattenedFunction.Evaluate(System.Collections.Generic.IEnumerable<Criteo.API.Versatile.EnumerableVariant>)",
        };
    }
}