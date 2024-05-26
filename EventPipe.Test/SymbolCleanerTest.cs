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
                "System.Collections.Concurrent.ConcurrentDictionary<T, T>.TryAddInternal(T, System.Nullable<int>, T, bool, bool, T&)",
        };
        yield return new TestCaseData(
            "System.Threading.ExecutionContext",
            "RunInternal",
            "void (class System.Threading.ExecutionContext,class System.Threading.ContextCallback,class System.Object)")
        {
            TestName = "Normal method",
            ExpectedResult =
                "System.Threading.ExecutionContext.RunInternal(System.Threading.ExecutionContext, System.Threading.ContextCallback, object)",
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
                "System.Diagnostics.Tracing.EventPipeEventProvider.EventWriteTransfer(System.Diagnostics.Tracing.EventDescriptor&, int, System.Guid*, System.Guid*, int, EventData*)",
        };
        yield return new TestCaseData(
            "Contoso.Cookies.CookieEntry",
            "TryGet",
            "instance generic value class Contoso.Cookies.EntryGetResult (!!0&,class System.Exception&,int32,bool)")
        {
            TestName = "Generic method with !!0",
            ExpectedResult = "Contoso.Cookies.CookieEntry.TryGet(T&, System.Exception&, int, bool)",
        };
        yield return new TestCaseData(
            "System.Collections.Immutable.ImmutableDictionary`2[Contoso.Banners.Configuration.Inventory.Sources.InventoryAvailabilityKey,Contoso.Banners.Configuration.Inventory.Sources.AvailableBanner]",
            "Wrap",
            "class System.Collections.Immutable.ImmutableDictionary`2<!0,!1>  (class System.Collections.Immutable.SortedInt32KeyNode`1<value class HashBucket<!0,!1>>,class Comparers<!0,!1>,int32)")
        {
            TestName = "Generic arguments using angle brackets and no tilde",
            ExpectedResult = "System.Collections.Immutable.ImmutableDictionary<Contoso.Banners.Configuration.Inventory.Sources.InventoryAvailabilityKey, Contoso.Banners.Configuration.Inventory.Sources.AvailableBanner>.Wrap(System.Collections.Immutable.SortedInt32KeyNode<HashBucket<T, T>>, Comparers<T, T>, int)",
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
            ExpectedResult =
                "Ghost.Core.ComponentMiddleware<T, T, T, T>.Invoke<T, T, T, Contoso.RTB.Common.SerializedData>()",
        };
        yield return new TestCaseData(
            "Ghost.Transports.Http.Common.Middlewares.HttpExceptionMiddleware",
            "Invoke",
            "instance class System.Threading.Tasks.Task`1<value class Ghost.Contract.InvocationResult> (class Ghost.Transports.Http.Contract.IHttpRequest,class Ghost.Transports.Http.Contract.IHttpResponse,value class System.Threading.CancellationToken)")
        {
            TestName = "Mix of class and struct arguments",
            ExpectedResult =
                "Ghost.Transports.Http.Common.Middlewares.HttpExceptionMiddleware.Invoke(Ghost.Transports.Http.Contract.IHttpRequest, Ghost.Transports.Http.Contract.IHttpResponse, System.Threading.CancellationToken)",
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
            ExpectedResult =
                "Contoso.API.Versatile.Functions.FlattenedFunction.Evaluate(System.Collections.Generic.IEnumerable<Contoso.API.Versatile.EnumerableVariant>)",
        };
        yield return new TestCaseData(
            "Program",
            "InstanceMethod",
            "instance int32 (value class System.ValueTuple`5<int32,int16,float32,class System.String,value class System.Guid>)")
        {
            TestName = "Value tuple argument",
            ExpectedResult =
                "Program.InstanceMethod(System.ValueTuple<int, short, float, string, System.Guid>)",
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
            "System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1+AsyncStateMachineBox`1[System.Threading.Tasks.VoidTaskResult,Contoso.ThirdPartyDemand.Bidding.Framework.DspSpecificCompositeStep+<>c__DisplayClass3_0+<<Process>g__OnDspState|0>d]",
            "MoveNext",
            "instance void  (class System.Threading.Thread)")
        {
            TestName = "Lambda of local method",
            ExpectedResult = "System.Runtime.CompilerServices.AsyncTaskMethodBuilder<T>+AsyncStateMachineBox<System.Threading.Tasks.VoidTaskResult, Contoso.ThirdPartyDemand.Bidding.Framework.DspSpecificCompositeStep.()=>{}.Process.OnDspState>(System.Threading.Thread)",
        };
        yield return new TestCaseData(
            "System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1+AsyncStateMachineBox`1[System.Threading.Tasks.VoidTaskResult,JetBrains.TextControl.DocumentMarkup.GutterMarksBackendServices+<>c__DisplayClass3_0+<<-ctor>g__BeginCreateForTextControlIfAllowed|1>d]",
            "MoveNext",
            "instance void  (class System.Threading.Thread)")
        {
            TestName = "Lambda of a local method in a constructor",
            ExpectedResult = "System.Runtime.CompilerServices.AsyncTaskMethodBuilder<T>+AsyncStateMachineBox<System.Threading.Tasks.VoidTaskResult, JetBrains.TextControl.DocumentMarkup.GutterMarksBackendServices.()=>{}.-ctor.BeginCreateForTextControlIfAllowed>(System.Threading.Thread)",
        };
        yield return new TestCaseData(
            "System.Threading.Tasks.Task+DelayPromiseWithCancellation+<>c",
            "<.ctor>b__1_0",
            "instance void (class System.Object,value class System.Threading.CancellationToken)")
        {
            TestName = "Lambda in constructor",
            ExpectedResult =
                "System.Threading.Tasks.Task+DelayPromiseWithCancellation.()=>{}.<.ctor>b__1_0(object, System.Threading.CancellationToken)",
        };
        yield return new TestCaseData(
            "System.Net.Http.HttpConnectionPool+<>c",
            "<CheckForHttp11ConnectionInjection>b__77_0",
            "instance void  (value class System.ValueTuple`2<class System.Net.Http.HttpConnectionPool,value class QueueItem<class System.Net.Http.HttpConnection>>)")
        {
            TestName = "Short lambda notation",
            ExpectedResult = "System.Net.Http.HttpConnectionPool.()=>{}.<CheckForHttp11ConnectionInjection>b__77_0(System.ValueTuple<System.Net.Http.HttpConnectionPool, QueueItem<System.Net.Http.HttpConnection>>)",
        };
        yield return new TestCaseData(
            "System.Threading.Tasks.ValueTask`1+ValueTaskSourceAsTask+<>c[System.Int32]",
            "<.cctor>b__4_0",
            "instance void  (class System.Object)")
        {
            TestName = "Generic lambda",
            ExpectedResult = "System.Threading.Tasks.ValueTask<T>+ValueTaskSourceAsTask.()=>{}<int>.<.cctor>b__4_0(object)",
        };
        yield return new TestCaseData(
            "Program",
            "<.ctor>g__LocalMethod|1_1",
            "void ()")
        {
            TestName = "Local method in constructor",
            ExpectedResult = "Program.<.ctor>g__LocalMethod|1_1()",
        };
        yield return new TestCaseData(
            "Program",
            "<InstanceMethod>g__LocalMethod|2_1",
            "void ()")
        {
            TestName = "Local method",
            ExpectedResult = "Program.<InstanceMethod>g__LocalMethod|2_1()",
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
            "Microsoft.EntityFrameworkCore.Query.Internal.SplitQueryingEnumerable`1+AsyncEnumerator+<>c[System.Int32]",
            "<MoveNextAsync>b__19_0",
            "instance class System.Threading.Tasks.Task`1<bool>  (class Microsoft.EntityFrameworkCore.DbContext,class AsyncEnumerator<!0>,value class System.Threading.CancellationToken)")
        {
            TestName = "Async enumerable method",
            ExpectedResult = "Microsoft.EntityFrameworkCore.Query.Internal.SplitQueryingEnumerable<T>+AsyncEnumerator.()=>{}<int>.<MoveNextAsync>b__19_0(Microsoft.EntityFrameworkCore.DbContext, AsyncEnumerator<T>, System.Threading.CancellationToken)",
        };
        yield return new TestCaseData(
            "Program+<>c",
            "<Main>b__7_0",
            "instance void (int32,class System.Object)")
        {
            TestName = "Lambda",
            ExpectedResult = "Program.()=>{}.<Main>b__7_0(int, object)",
        };
        yield return new TestCaseData(
            "Contoso.CookiesUpdater.PublisherCookiesUpdater+<>c__DisplayClass10_0+<<OnBidRequest>b__0>d",
            "MoveNext",
            "instance void  ()")
        {
            TestName = "Async lambda",
            ExpectedResult = "Contoso.CookiesUpdater.PublisherCookiesUpdater.()=>{}.OnBidRequest()",
        };
        yield return new TestCaseData(
            "Program+<>c__DisplayClass7_0",
            "<Main>b__1",
            "instance void (class System.String)")
        {
            TestName = "Closure",
            ExpectedResult = "Program.()=>{}.<Main>b__1(string)",
            RunState = RunState.Ignored,
        };
    }
}