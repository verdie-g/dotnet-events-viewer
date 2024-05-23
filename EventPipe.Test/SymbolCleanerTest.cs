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
            TestName = "Generic method",
            ExpectedResult =
                "System.Collections.Concurrent.ConcurrentDictionary<T, T>.TryAddInternal(T, System.Nullable<int32>, T, bool, bool, T&)",
        };
        yield return new TestCaseData(
            "System.Threading.ExecutionContext",
            "RunInternal",
            "void (class System.Threading.ExecutionContext,class System.Threading.ContextCallback,class System.Object)")
        {
            TestName = "Normal method",
            ExpectedResult =
                "System.Threading.ExecutionContext.RunInternal(System.Threading.ExecutionContext, System.Threading.ContextCallback, System.Object)",
        };
        yield return new TestCaseData(
            "System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1+AsyncStateMachineBox`1[Contoso.Cookies.Cookies.EntryFetched,Contoso.Cookies.CookieSet+<GetEntryAsync>d__24]",
            "MoveNext",
            "instance void (class System.Threading.Thread)")
        {
            TestName = "Async state machine",
            ExpectedResult =
                "System.Runtime.CompilerServices.AsyncTaskMethodBuilder<T>+AsyncStateMachineBox<Contoso.Cookies.Cookies.EntryFetched, Contoso.Cookies.CookieSet.GetEntryAsync>(System.Threading.Thread)",
        };
        yield return new TestCaseData(
            "System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1+AsyncStateMachineBox`1[System.ValueTuple`2[Sdk.Interfaces.KeyValueStore.ReturnCode,System.__Canon],Contoso.KeyValueStore.Storage.BucketStorage+<Sdk-Interfaces-KeyValueStore-IAsyncStorage-GetAsync>d__17]",
            "MoveNext",
            "instance void (class System.Threading.Thread)")
        {
            TestName = "Weird dashed name",
            ExpectedResult =
                "System.Runtime.CompilerServices.AsyncTaskMethodBuilder<T>+AsyncStateMachineBox<System.ValueTuple<Sdk.Interfaces.KeyValueStore.ReturnCode, T>, Contoso.KeyValueStore.Storage.BucketStorage.Sdk-Interfaces-KeyValueStore-IAsyncStorage-GetAsync>(System.Threading.Thread)",
        };
        yield return new TestCaseData(
            "System.Threading.PortableThreadPool+WorkerThread",
            "WorkerThreadStart",
            "void ()")
        {
            TestName = "Subclass",
            ExpectedResult = "System.Threading.PortableThreadPool+WorkerThread.WorkerThreadStart()",
        };
        yield return new TestCaseData(
            "Contoso.Features.Data.FeatureAggregationCookieService",
            ".ctor",
            "instance void (class Contoso.ConfigAsCode.IConfigAsCodeService)")
        {
            TestName = "Constructor",
            ExpectedResult =
                "new Contoso.Features.Data.FeatureAggregationCookieService(Contoso.ConfigAsCode.IConfigAsCodeService)",
        };
        yield return new TestCaseData(
            "System.Diagnostics.Tracing.EventPipeEventProvider",
            "EventWriteTransfer",
            "instance value class WriteEventErrorCode (required_modifier System.Runtime.InteropServices.InAttribute value class System.Diagnostics.Tracing.EventDescriptor&,int,value class System.Guid*,value class System.Guid*,int32,value class EventData*)")
        {
            TestName = "ref/in/ptr parameters",
            ExpectedResult =
                "System.Diagnostics.Tracing.EventPipeEventProvider.EventWriteTransfer(System.Diagnostics.Tracing.EventDescriptor&, int, System.Guid*, System.Guid*, int32, EventData*)",
        };
        yield return new TestCaseData(
            "Contoso.Cookies.CookieEntry",
            "TryGet",
            "instance generic value class Contoso.Cookies.EntryGetResult (!!0&,class System.Exception&,int32,bool)")
        {
            TestName = "Generic method with !!0",
            ExpectedResult = "Contoso.Cookies.CookieEntry.TryGet(T&, System.Exception&, int32, bool)",
        };
        yield return new TestCaseData(
            "System.Threading.Tasks.Task+DelayPromiseWithCancellation+<>c",
            "<.ctor>b__1_0",
            "instance void (class System.Object,value class System.Threading.CancellationToken)")
        {
            ExpectedResult = "System.Threading.Tasks.Task+DelayPromiseWithCancellation+<>c.<.ctor>b__1_0(System.Object, System.Threading.CancellationToken)",
            RunState = RunState.Ignored,
        };
        yield return new TestCaseData(
            "Sdk.Connection.GhostClientBase`2+<Process>d__42[System.__Canon,System.__Canon]",
            "MoveNext",
            "instance void ()")
        {
            TestName = "Generic async method",
            ExpectedResult = "Sdk.Connection.GhostClientBase<T, T>.Process<T, T>()",
        };
        yield return new TestCaseData(
            "Ghost.Core.ComponentMiddleware`4+<Invoke>d__5[System.__Canon,System.__Canon,System.__Canon,Contoso.RTB.Common.SerializedData]",
            "MoveNext",
            "instance void ()")
        {
            TestName = "Mix of T and specified type",
            ExpectedResult = "Ghost.Core.ComponentMiddleware<T, T, T, T>.Invoke<T, T, T, Contoso.RTB.Common.SerializedData>()",
        };
        yield return new TestCaseData(
            "Ghost.Transports.Http.Common.Middlewares.HttpExceptionMiddleware",
            "Invoke",
            "instance class System.Threading.Tasks.Task`1<value class Ghost.Contract.InvocationResult> (class Ghost.Transports.Http.Contract.IHttpRequest,class Ghost.Transports.Http.Contract.IHttpResponse,value class System.Threading.CancellationToken)")
        {
            TestName = "Mix of class and struct arguments",
            ExpectedResult = "Ghost.Transports.Http.Common.Middlewares.HttpExceptionMiddleware.Invoke(Ghost.Transports.Http.Contract.IHttpRequest, Ghost.Transports.Http.Contract.IHttpResponse, System.Threading.CancellationToken)",
        };
        yield return new TestCaseData(
            "Program",
            "ProcessArrays",
            "instance void (int32[],int16[][],unsigned int8[][][],float32[,],float64[,,])")
        {
            TestName = "Array arguments",
            ExpectedResult = "Program.ProcessArrays(int32[], int16[][], unsigned int8[][][], float32[,], float64[,,])",
        };
        yield return new TestCaseData(
            "Contoso.API.Versatile.Functions.FlattenedFunction",
            "Evaluate",
            "instance value class Contoso.API.Versatile.EnumerableVariant  (class System.Collections.Generic.IEnumerable`1<value class Contoso.API.Versatile.EnumerableVariant>)")
        {
            TestName = "Generic argument",
            ExpectedResult = "Contoso.API.Versatile.Functions.FlattenedFunction.Evaluate(System.Collections.Generic.IEnumerable<Contoso.API.Versatile.EnumerableVariant>)",
        };
        yield return new TestCaseData(
            "Program",
            "InstanceMethod",
            "instance int32 (value class System.ValueTuple`5<int32,int16,float32,class System.String,value class System.Guid>)")
        {
            TestName = "Value tuple argument",
            ExpectedResult = "Program.InstanceMethod(System.ValueTuple<int32, int16, float32, System.String, System.Guid>)",
        };
        yield return new TestCaseData(
            "Program+<InstanceMethodAsync>d__3",
            "MoveNext",
            "instance void ()")
        {
            TestName = "Async method",
            ExpectedResult = "Program.InstanceMethodAsync()",
        };
        yield return new TestCaseData(
            "Program+<<InstanceMethodAsync>g__LocalMethodAsync|3_1>d",
            "MoveNext",
            "instance void ()")
        {
            TestName = "Local async method in async method",
            ExpectedResult = "Program.InstanceMethodAsync.LocalMethodAsync()",
        };
        yield return new TestCaseData(
            "Program+<<GenericInstanceMethodAsync>g__LocalGenericMethodAsync|4_1>d`2[System.__Canon,System.__Canon]",
            "MoveNext",
            "instance void ()")
        {
            TestName = "Local generic async method in generic async method",
            ExpectedResult = "Program.GenericInstanceMethodAsync.LocalGenericMethodAsync<T, T>()",
        };
        yield return new TestCaseData(
            "Program",
            "<.ctor>g__LocalMethod|1_1",
            "void ()")
        {
            TestName = "Local method in constructor",
            ExpectedResult = "Program..ctor.LocalMethod",
            RunState = RunState.Ignored,
        };
        yield return new TestCaseData(
            "Program",
            "<InstanceMethod>g__LocalMethod|2_1",
            "void ()")
        {
            TestName = "Local method",
            ExpectedResult = "Program.InstanceMethod.LocalMethod",
            RunState = RunState.Ignored,
        };
        yield return new TestCaseData(
            "Program+<MyEnumerableMethod>d__4",
            "MoveNext",
            "instance bool ()")
        {
            TestName = "Enumerable Method",
            ExpectedResult = "Program.MyEnumerableMethod()",
        };
        yield return new TestCaseData(
            "Program+<>c",
            "<Main>b__7_0",
            "Signature=instance void (int32,class System.Object)")
        {
            TestName = "Lambda",
            ExpectedResult = "Program.Main.lambda(int, System.Object)",
            RunState = RunState.Ignored,
        };
        yield return new TestCaseData(
            "Contoso.CookiesUpdater.PublisherCookiesUpdater+<>c__DisplayClass10_0+<<OnBidRequest>b__0>d",
            "MoveNext",
            "instance void  ()")
        {
            TestName = "Async lambda",
            ExpectedResult = "Contoso.CookiesUpdater.PublisherCookiesUpdater.OnBidRequest()",
        };
        yield return new TestCaseData(
            "Program+<>c__DisplayClass7_0",
            "<Main>b__1",
            "instance void (class System.String)")
        {
            TestName = "Closure",
            ExpectedResult = "Program.Main.lambda(System.String)",
            RunState = RunState.Ignored,
        };
    }
}