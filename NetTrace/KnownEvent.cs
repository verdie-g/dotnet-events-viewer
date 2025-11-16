using System.Collections;
using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Tracing;
using NetTrace.FastSerializer;

namespace NetTrace;

/// <summary>
/// Some Microsoft-Windows-DotNETRuntime events have incomplete metadata in the trace (https://github.com/dotnet/runtime/issues/96365)
/// so they have to be hardcoded here. Also, it allows to have custom parsers for them which greatly improve the
/// performance and memory footprint.
/// </summary>
internal class KnownEvent
{
    public const string EventPipeProvider = "Microsoft-DotNETCore-EventPipe";
    public const string RuntimeProvider = "Microsoft-Windows-DotNETRuntime";
    public const string RundownProvider = "Microsoft-Windows-DotNETRuntimeRundown";
    public const string SampleProfilerProvider = "Microsoft-DotNETCore-SampleProfiler";
    public const string TplProvider = "System.Threading.Tasks.TplEventSource";
    public const string ArrayPoolProvider = "System.Buffers.ArrayPoolEventSource";
    public const string HttpClientProvider = "System.Net.Http";
    public const string SocketsProvider = "System.Net.Sockets";
    public const string DnsProvider = "System.Net.NameResolution";
    public const string DependencyInjectionProvider = "Microsoft-Extensions-DependencyInjection";
    public const string RecyclableMemoryStreamProvider = "Microsoft-IO-RecyclableMemoryStream";

    private static readonly IReadOnlyDictionary<string, object> EmptyDictionary = new Dictionary<string, object>();

    public static readonly FrozenDictionary<Key, KnownEvent> All = new Dictionary<Key, KnownEvent>
    {
        [new Key(EventPipeProvider, 1, 1)] = new("ProcessInfo", 0, ProcessInfoPayload.FieldDefinitions, ProcessInfoPayload.Parse),
        [new Key(RuntimeProvider, 1, 2)] = new("GCStart", null, GcStartV2Payload.FieldDefinitions, GcStartV2Payload.Parse),
        [new Key(RuntimeProvider, 2, 1)] = new("GCEnd", null, GcEndV1Payload.FieldDefinitions, GcEndV1Payload.Parse),
        [new Key(RuntimeProvider, 3, 1)] = new("GCRestartEEEnd", null, GcNoUserDataPayload.FieldDefinitions, GcNoUserDataPayload.Parse),
        [new Key(RuntimeProvider, 4, 2)] = new("GCHeapStats", null, GcHeapStatsV2Payload.FieldDefinitions, GcHeapStatsV2Payload.Parse),
        [new Key(RuntimeProvider, 7, 1)] = new("GCRestartEEBegin", null, GcNoUserDataPayload.FieldDefinitions, GcNoUserDataPayload.Parse),
        [new Key(RuntimeProvider, 8, 1)] = new("GCSuspendEEEnd", null, GcNoUserDataPayload.FieldDefinitions, GcNoUserDataPayload.Parse),
        [new Key(RuntimeProvider, 9, 1)] = new("GCSuspendEEBegin", null, GcSuspendEeV1Payload.FieldDefinitions, GcSuspendEeV1Payload.Parse),
        [new Key(RuntimeProvider, 10, 4)] = new("GCAllocationTick", null, GcAllocationTickV4Payload.FieldDefinitions, GcAllocationTickV4Payload.Parse),
        [new Key(RuntimeProvider, 13, 1)] = new("GCFinalizersEnd", null, GcFinalizersEndV1Payload.FieldDefinitions, GcFinalizersEndV1Payload.Parse),
        [new Key(RuntimeProvider, 14, 1)] = new("GCFinalizersBegin", null, GcNoUserDataPayload.FieldDefinitions, GcNoUserDataPayload.Parse),
        [new Key(RuntimeProvider, 23, 0)] = new("GCGenerationRange", null, GcGenerationRangePayload.FieldDefinitions, GcGenerationRangePayload.Parse),
        [new Key(RuntimeProvider, 29, 0)] = new("FinalizeObject", null, FinalizeObjectPayload.FieldDefinitions, FinalizeObjectPayload.Parse),
        [new Key(RuntimeProvider, 30, 0)] = new("SetGCHandle", null, SetGcHandlePayload.FieldDefinitions, SetGcHandlePayload.Parse),
        [new Key(RuntimeProvider, 31, 0)] = new("DestroyGCHandle", null, DestroyGcHandlePayload.FieldDefinitions, DestroyGcHandlePayload.Parse),
        [new Key(RuntimeProvider, 33, 0)] = new("PinObjectAtGCTime", null, PinObjectAtGcTimePayload.FieldDefinitions, PinObjectAtGcTimePayload.Parse),
        [new Key(RuntimeProvider, 35, 0)] = new("GCTriggered", null, GcTriggeredPayload.FieldDefinitions, GcTriggeredPayload.Parse),
        [new Key(RuntimeProvider, 54, 0)] = new("ThreadPoolWorkerThreadAdjustmentSample", null, ThreadPoolWorkerThreadAdjustmentSamplePayload.FieldDefinitions, ThreadPoolWorkerThreadAdjustmentSamplePayload.Parse),
        [new Key(RuntimeProvider, 55, 0)] = new("ThreadPoolWorkerThreadAdjustmentAdjustment", null, ThreadPoolWorkerThreadAdjustmentAdjustmentPayload.FieldDefinitions, ThreadPoolWorkerThreadAdjustmentAdjustmentPayload.Parse),
        [new Key(RuntimeProvider, 56, 0)] = new("ThreadPoolWorkerThreadAdjustmentStats", null, ThreadPoolWorkerThreadAdjustmentStatsPayload.FieldDefinitions, ThreadPoolWorkerThreadAdjustmentStatsPayload.Parse),
        [new Key(RuntimeProvider, 57, 0)] = new("ThreadPoolWorkerThreadWait", null, ThreadPoolWorkerThreadPayload.FieldDefinitions, ThreadPoolWorkerThreadPayload.Parse),
        [new Key(RuntimeProvider, 58, 0)] = new("YieldProcessorMeasurement", null, YieldProcessorMeasurementPayload.FieldDefinitions, YieldProcessorMeasurementPayload.Parse),
        [new Key(RuntimeProvider, 80, 1)] = new("ExceptionThrown", null, ExceptionPayload.FieldDefinitions, ExceptionPayload.Parse),
        [new Key(RuntimeProvider, 81, 1)] = new("ContentionStart", null, ContentionStartPayload.FieldDefinitions, ContentionStartPayload.Parse),
        [new Key(RuntimeProvider, 81, 2)] = new("ContentionStart", null, ContentionStartV2Payload.FieldDefinitions, ContentionStartV2Payload.Parse),
        [new Key(RuntimeProvider, 85, 0)] = new("ThreadCreated", null, ThreadCreatedPayload.FieldDefinitions, ThreadCreatedPayload.Parse),
        [new Key(RuntimeProvider, 88, 0)] = new("ILStubGenerated", null, IlStubGeneratedPayload.FieldDefinitions, IlStubGeneratedPayload.Parse),
        [new Key(RuntimeProvider, 91, 1)] = new("ContentionStop", null, ContentionStopV1Payload.FieldDefinitions, ContentionStopV1Payload.Parse),
        [new Key(RuntimeProvider, 143, 1)] = new("MethodLoadVerbose", null, MethodLoadUnloadVerboseV1Payload.FieldDefinitions, MethodLoadUnloadVerboseV1Payload.Parse),
        [new Key(RuntimeProvider, 143, 2)] = new("MethodLoadVerbose", null, MethodLoadUnloadVerboseV2Payload.FieldDefinitions, MethodLoadUnloadVerboseV2Payload.Parse),
        [new Key(RuntimeProvider, 145, 1)] = new("MethodJittingStarted", null, MethodJittingStartedV1Payload.FieldDefinitions, MethodJittingStartedV1Payload.Parse),
        [new Key(RuntimeProvider, 146, 0)] = new("MethodJitMemoryAllocatedForCode", null, MethodJitMemoryAllocatedForCodePayload.FieldDefinitions, MethodJitMemoryAllocatedForCodePayload.Parse),
        [new Key(RuntimeProvider, 185, 0)] = new("MethodJitInliningSucceeded", null, MethodJitInliningSucceededPayload.FieldDefinitions, MethodJitInliningSucceededPayload.Parse),
        [new Key(RuntimeProvider, 188, 0)] = new("MethodJitTailCallSucceeded", null, MethodJitTailCallSucceededPayload.FieldDefinitions, MethodJitTailCallSucceededPayload.Parse),
        [new Key(RuntimeProvider, 192, 0)] = new("MethodJitInliningFailed", null, MethodJitInliningFailedPayload.FieldDefinitions, MethodJitInliningFailedPayload.Parse),
        [new Key(RuntimeProvider, 202, 0)] = new("GCMarkWithType", null, GcMarkWithTypePayload.FieldDefinitions, GcMarkWithTypePayload.Parse),
        [new Key(RuntimeProvider, 203, 2)] = new("GCJoin", null, GcJoinV2Payload.FieldDefinitions, GcJoinV2Payload.Parse),
        [new Key(RuntimeProvider, 250, 0)] = new("ExceptionCatchStart", null, ExceptionHandlingPayload.FieldDefinitions, ExceptionHandlingPayload.Parse),
        [new Key(RuntimeProvider, 252, 0)] = new("ExceptionFinallyStart", null, ExceptionHandlingPayload.FieldDefinitions, ExceptionHandlingPayload.Parse),
        [new Key(RuntimeProvider, 254, 0)] = new("ExceptionFilterStart", null, ExceptionHandlingPayload.FieldDefinitions, ExceptionHandlingPayload.Parse),
        [new Key(RuntimeProvider, 301, 0)] = new("WaitHandleWaitStart", null, WaitHandleWaitStartPayload.FieldDefinitions, WaitHandleWaitStartPayload.Parse),
        [new Key(RuntimeProvider, 302, 0)] = new("WaitHandleWaitStop", null, WaitHandleWaitStopPayload.FieldDefinitions, WaitHandleWaitStopPayload.Parse),
        [new Key(RundownProvider, 144, 1)] = new("MethodDCEndVerbose", null, MethodLoadUnloadRundownVerboseV1Payload.FieldDefinitions, MethodLoadUnloadRundownVerboseV1Payload.Parse),
        [new Key(RundownProvider, 144, 2)] = new("MethodDCEndVerbose", null, MethodLoadUnloadRundownVerboseV2Payload.FieldDefinitions, MethodLoadUnloadRundownVerboseV2Payload.Parse),
        [new Key(RundownProvider, 146, 1)] = new("DCEndComplete", null, DcStartEndPayload.FieldDefinitions, DcStartEndPayload.Parse),
        [new Key(RundownProvider, 148, 1)] = new("DCEndInit", null, DcStartEndPayload.FieldDefinitions, DcStartEndPayload.Parse),
        [new Key(RundownProvider, 152, 1)] = new("DomainModuleDCEnd", null, DomainModuleLoadUnloadRundownV1Payload.FieldDefinitions, DomainModuleLoadUnloadRundownV1Payload.Parse),
        [new Key(RundownProvider, 154, 2)] = new("ModuleDCEnd", null, ModuleLoadUnloadRundownV2Payload.FieldDefinitions, ModuleLoadUnloadRundownV2Payload.Parse),
        [new Key(RundownProvider, 156, 1)] = new("AssemblyDCEnd", null, AssemblyLoadUnloadRundownV1Payload.FieldDefinitions, AssemblyLoadUnloadRundownV1Payload.Parse),
        [new Key(RundownProvider, 158, 1)] = new("AppDomainDCEnd", null, AppDomainLoadUnloadRundownV1Payload.FieldDefinitions, AppDomainLoadUnloadRundownV1Payload.Parse),
        [new Key(RundownProvider, 187, 0)] = new("RuntimeInformationDCStart", null, RuntimeInformationRundownPayload.FieldDefinitions, RuntimeInformationRundownPayload.Parse),
        [new Key(SampleProfilerProvider, 0, 0)] = new("ThreadSample", null, ThreadSamplePayload.FieldDefinitions, ThreadSamplePayload.Parse),
        [new Key(TplProvider, 7, 1)] = new("TaskScheduled", EventOpcode.Send, TaskScheduledPayload.FieldDefinitions, TaskScheduledPayload.Parse),
        [new Key(TplProvider, 8, 0)] = new("TaskStarted", null, TaskStartedPayload.FieldDefinitions, TaskStartedPayload.Parse),
        [new Key(TplProvider, 9, 1)] = new("TaskCompleted", null, TaskCompletedPayload.FieldDefinitions, TaskCompletedPayload.Parse),
        [new Key(TplProvider, 10, 3)] = new("TaskWaitBegin", EventOpcode.Send, TaskWaitBeginPayload.FieldDefinitions, TaskWaitBeginPayload.Parse),
        [new Key(TplProvider, 11, 0)] = new("TaskWaitEnd", null, TaskWaitEndPayload.FieldDefinitions, TaskWaitEndPayload.Parse),
        [new Key(TplProvider, 12, 0)] = new("AwaitTaskContinuationScheduled", EventOpcode.Send, AwaitTaskContinuationScheduledPayload.FieldDefinitions, AwaitTaskContinuationScheduledPayload.Parse),
        [new Key(TplProvider, 13, 0)] = new("TaskWaitContinuationComplete", null, TaskWaitContinuationPayload.FieldDefinitions, TaskWaitContinuationPayload.Parse),
        [new Key(TplProvider, 14, 1)] = new("TraceOperationStart", null, TraceOperationStartPayload.FieldDefinitions, TraceOperationStartPayload.Parse),
        [new Key(TplProvider, 15, 1)] = new("TraceOperationStop", null, TraceOperationStopPayload.FieldDefinitions, TraceOperationStopPayload.Parse),
        [new Key(TplProvider, 16, 1)] = new("TraceOperationRelation", null, TraceOperationRelationPayload.FieldDefinitions, TraceOperationRelationPayload.Parse),
        [new Key(TplProvider, 19, 0)] = new("TaskWaitContinuationStarted", null, TaskWaitContinuationPayload.FieldDefinitions, TaskWaitContinuationPayload.Parse),
        [new Key(ArrayPoolProvider, 1, 0)] = new("BufferRented", null, BufferRentedPayload.FieldDefinitions, BufferRentedPayload.Parse),
        [new Key(ArrayPoolProvider, 2, 0)] = new("BufferAllocated", null, BufferAllocatedPayload.FieldDefinitions, BufferAllocatedPayload.Parse),
        [new Key(ArrayPoolProvider, 3, 0)] = new("BufferReturned", null, BufferReturnedPayload.FieldDefinitions, BufferReturnedPayload.Parse),
        [new Key(ArrayPoolProvider, 4, 0)] = new("BufferTrimmed", null, BufferTrimmedPayload.FieldDefinitions, BufferTrimmedPayload.Parse),
        [new Key(ArrayPoolProvider, 5, 0)] = new("BufferTrimPoll", null, BufferTrimPollPayload.FieldDefinitions, BufferTrimPollPayload.Parse),
        [new Key(ArrayPoolProvider, 6, 0)] = new("BufferDropped", null, BufferDroppedPayload.FieldDefinitions, BufferDroppedPayload.Parse),
        [new Key(HttpClientProvider, 1, 0)] = new("RequestStart", EventOpcode.Start, RequestStartPayload.FieldDefinitions, RequestStartPayload.Parse),
        [new Key(HttpClientProvider, 2, 1)] = new("RequestStop", EventOpcode.Stop, RequestStopPayload.FieldDefinitions, RequestStopPayload.Parse),
        [new Key(HttpClientProvider, 3, 1)] = new("RequestFailed", null, RequestFailedPayload.FieldDefinitions, RequestFailedPayload.Parse),
        [new Key(HttpClientProvider, 4, 0)] = new("ConnectionEstablished", null, ConnectionEstablishedPayload.FieldDefinitions, ConnectionEstablishedPayload.Parse),
        [new Key(HttpClientProvider, 5, 0)] = new("ConnectionClosed", null, ConnectionClosedPayload.FieldDefinitions, ConnectionClosedPayload.Parse),
        [new Key(HttpClientProvider, 6, 0)] = new("RequestLeftQueue", null, RequestLeftQueuePayload.FieldDefinitions, RequestLeftQueuePayload.Parse),
        [new Key(HttpClientProvider, 7, 1)] = new("RequestHeaderStart", EventOpcode.Start, RequestHeaderStartPayload.FieldDefinitions, RequestHeaderStartPayload.Parse),
        [new Key(HttpClientProvider, 8, 0)] = CreateEmpty("RequestHeaderStop", EventOpcode.Stop),
        [new Key(HttpClientProvider, 9, 0)] = CreateEmpty("RequestContentStart", EventOpcode.Start),
        [new Key(HttpClientProvider, 10, 0)] = new("RequestContentStop", EventOpcode.Stop, RequestContentStopPayload.FieldDefinitions, RequestContentStopPayload.Parse),
        [new Key(HttpClientProvider, 11, 0)] = CreateEmpty("ResponseHeadersStart", EventOpcode.Start),
        [new Key(HttpClientProvider, 12, 1)] = new("ResponseHeadersStop", EventOpcode.Stop, ResponseHeadersStopPayload.FieldDefinitions, ResponseHeadersStopPayload.Parse),
        [new Key(HttpClientProvider, 13, 0)] = CreateEmpty("ResponseContentStart", EventOpcode.Start),
        [new Key(HttpClientProvider, 14, 0)] = CreateEmpty("ResponseContentStop", EventOpcode.Stop),
        [new Key(HttpClientProvider, 15, 0)] = new("RequestFailedDetailed", null, RequestFailedDetailedPayload.FieldDefinitions, RequestFailedDetailedPayload.Parse),
        [new Key(HttpClientProvider, 16, 0)] = new("Redirect", null, RedirectPayload.FieldDefinitions, RedirectPayload.Parse),
        [new Key(SocketsProvider, 1, 0)] = new("ConnectStart", EventOpcode.Start, ConnectStartPayload.FieldDefinitions, ConnectStartPayload.Parse),
        [new Key(SocketsProvider, 2, 0)] = CreateEmpty("ConnectStop", EventOpcode.Stop),
        [new Key(SocketsProvider, 3, 0)] = new("ConnectFailed", null, SocketErrorPayload.FieldDefinitions, SocketErrorPayload.Parse),
        [new Key(SocketsProvider, 4, 0)] = new("AcceptStart", EventOpcode.Start, AcceptStartPayload.FieldDefinitions, AcceptStartPayload.Parse),
        [new Key(SocketsProvider, 5, 0)] = CreateEmpty("AcceptStop", EventOpcode.Stop),
        [new Key(SocketsProvider, 6, 0)] = new("AcceptFailed", null, SocketErrorPayload.FieldDefinitions, SocketErrorPayload.Parse),
        [new Key(DnsProvider, 1, 0)] = new("ResolutionStart", EventOpcode.Start, ResolutionStartPayload.FieldDefinitions, ResolutionStartPayload.Parse),
        [new Key(DnsProvider, 2, 0)] = CreateEmpty("ResolutionStop", EventOpcode.Stop),
        [new Key(DnsProvider, 3, 0)] = CreateEmpty("ResolutionFailed", null),
        [new Key(DependencyInjectionProvider, 156, 0)] = new("ServiceProviderBuilt", null, ServiceProviderBuiltPayload.FieldDefinitions, ServiceProviderBuiltPayload.Parse),
        [new Key(DependencyInjectionProvider, 157, 0)] = new("ServiceProviderDescriptors", null, ServiceProviderDescriptorsPayload.FieldDefinitions, ServiceProviderDescriptorsPayload.Parse),
        [new Key(DependencyInjectionProvider, 158, 0)] = new("ServiceResolved", null, ServiceResolvedPayload.FieldDefinitions, ServiceResolvedPayload.Parse),
        [new Key(DependencyInjectionProvider, 159, 0)] = new("ScopeDisposed", null, ScopeDisposedPayload.FieldDefinitions, ScopeDisposedPayload.Parse),
        [new Key(RecyclableMemoryStreamProvider, 1, 2)] = new("MemoryStreamCreated", null, MemoryStreamCreatedPayload.FieldDefinitions, MemoryStreamCreatedPayload.Parse),
        [new Key(RecyclableMemoryStreamProvider, 2, 3)] = new("MemoryStreamDisposed", null, MemoryStreamDisposedPayload.FieldDefinitions, MemoryStreamDisposedPayload.Parse),
        [new Key(RecyclableMemoryStreamProvider, 3, 0)] = new("MemoryStreamDoubleDispose", null, MemoryStreamDoubleDisposePayload.FieldDefinitions, MemoryStreamDoubleDisposePayload.Parse),
        [new Key(RecyclableMemoryStreamProvider, 4, 0)] = new("MemoryStreamFinalized", null, MemoryStreamFinalizedPayload.FieldDefinitions, MemoryStreamFinalizedPayload.Parse),
        [new Key(RecyclableMemoryStreamProvider, 5, 0)] = new("MemoryStreamToArray", null, MemoryStreamToArrayPayload.FieldDefinitions, MemoryStreamToArrayPayload.Parse),
        [new Key(RecyclableMemoryStreamProvider, 5, 2)] = new("MemoryStreamToArray", null, MemoryStreamToArrayPayload.FieldDefinitions, MemoryStreamToArrayPayload.Parse),
        [new Key(RecyclableMemoryStreamProvider, 6, 0)] = new("MemoryStreamManagerInitialized", null, MemoryStreamManagerInitializedPayload.FieldDefinitions, MemoryStreamManagerInitializedPayload.Parse),
        [new Key(RecyclableMemoryStreamProvider, 7, 2)] = new("MemoryStreamNewBlockCreated", null, MemoryStreamNewBlockCreatedPayload.FieldDefinitions, MemoryStreamNewBlockCreatedPayload.Parse),
        [new Key(RecyclableMemoryStreamProvider, 8, 3)] = new("MemoryStreamNewLargeBufferCreated", null, MemoryStreamNewLargeBufferCreatedPayload.FieldDefinitions, MemoryStreamNewLargeBufferCreatedPayload.Parse),
        [new Key(RecyclableMemoryStreamProvider, 9, 3)] = new("MemoryStreamNonPooledLargeBufferCreated", null, MemoryStreamNonPooledLargeBufferCreatedPayload.FieldDefinitions, MemoryStreamNonPooledLargeBufferCreatedPayload.Parse),
        [new Key(RecyclableMemoryStreamProvider, 10, 2)] = new("MemoryStreamDiscardBuffer", null, MemoryStreamDiscardBufferPayload.FieldDefinitions, MemoryStreamDiscardBufferPayload.Parse),
        [new Key(RecyclableMemoryStreamProvider, 11, 3)] = new("MemoryStreamOverCapacity", null, MemoryStreamOverCapacityPayload.FieldDefinitions, MemoryStreamOverCapacityPayload.Parse),
    }.ToFrozenDictionary();

    private static KnownEvent CreateEmpty(string name, EventOpcode? opcode)
    {
        return new KnownEvent(name, opcode, [], GetEmptyDictionary);

        static IReadOnlyDictionary<string, object> GetEmptyDictionary(ref FastSerializerSequenceReader _)
        {
            return EmptyDictionary;
        }
    }

    public string EventName { get; }
    public EventOpcode? Opcode { get; }
    public IReadOnlyList<EventFieldDefinition> FieldDefinitions { get; }
    public EventParser Parse { get; }

    private KnownEvent(
        string eventName,
        EventOpcode? opcode,
        IReadOnlyList<EventFieldDefinition> fieldDefinitions,
        EventParser parse)
    {
        EventName = eventName;
        Opcode = opcode;
        FieldDefinitions = fieldDefinitions;
        Parse = parse;
    }

    public readonly struct Key : IEquatable<Key>
    {
        public readonly string ProviderName;
        public readonly int EventId;
        public readonly int Version;

        public Key(string providerName, int eventId, int version)
        {
            EventId = eventId;
            Version = version;
            ProviderName = providerName;
        }

        public override bool Equals(object? obj)
        {
            return obj is Key other && Equals(other);
        }

        public bool Equals(Key other)
        {
            return EventId == other.EventId && Version == other.Version && ProviderName == other.ProviderName;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(EventId, Version, ProviderName);
        }
    }

    public delegate IReadOnlyDictionary<string, object> EventParser(ref FastSerializerSequenceReader reader);

    private sealed class ProcessInfoPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("CommandLine", NetTraceTypeCode.NullTerminatedUtf16String),
            new("OSInformation", NetTraceTypeCode.NullTerminatedUtf16String),
            new("ArchInformation", NetTraceTypeCode.NullTerminatedUtf16String),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new ProcessInfoPayload(
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String());
        }

        private readonly string _commandLine;
        private readonly string _oSInformation;
        private readonly string _archInformation;

        private ProcessInfoPayload(string commandLine, string oSInformation, string archInformation)
        {
            _commandLine = commandLine;
            _oSInformation = oSInformation;
            _archInformation = archInformation;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "CommandLine":
                    value = _commandLine;
                    return true;
                case "OSInformation":
                    value = _oSInformation;
                    return true;
                case "ArchInformation":
                    value = _archInformation;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("CommandLine", _commandLine);
            yield return new KeyValuePair<string, object>("OSInformation", _oSInformation);
            yield return new KeyValuePair<string, object>("ArchInformation", _archInformation);
        }
    }

    private sealed class GcStartV2Payload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("Count", NetTraceTypeCode.UInt32),
            new("Depth", NetTraceTypeCode.UInt32),
            new("Reason", NetTraceTypeCode.UInt32),
            new("Type", NetTraceTypeCode.UInt32),
            new("ClrInstanceID", NetTraceTypeCode.UInt16),
            new("ClientSequenceNumber", NetTraceTypeCode.UInt64),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new GcStartV2Payload(
                reader.ReadUInt32(),
                reader.ReadUInt32(),
                reader.ReadUInt32(),
                reader.ReadUInt32(),
                reader.ReadUInt16(),
                reader.ReadUInt64());
        }

        private readonly uint _count;
        private readonly uint _depth;
        private readonly uint _reason;
        private readonly uint _type;
        private readonly ushort _clrInstanceId;
        private readonly ulong _clientSequenceNumber;

        private GcStartV2Payload(uint count, uint depth, uint reason, uint type, ushort clrInstanceId,
            ulong clientSequenceNumber)
        {
            _count = count;
            _depth = depth;
            _reason = reason;
            _type = type;
            _clrInstanceId = clrInstanceId;
            _clientSequenceNumber = clientSequenceNumber;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "Count":
                    value = _count;
                    return true;
                case "Depth":
                    value = _depth;
                    return true;
                case "Reason":
                    value = _reason;
                    return true;
                case "Type":
                    value = _type;
                    return true;
                case "ClrInstanceID":
                    value = _clrInstanceId;
                    return true;
                case "ClientSequenceNumber":
                    value = _clientSequenceNumber;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("Count", _count);
            yield return new KeyValuePair<string, object>("Depth", _depth);
            yield return new KeyValuePair<string, object>("Reason", _reason);
            yield return new KeyValuePair<string, object>("Type", _type);
            yield return new KeyValuePair<string, object>("ClrInstanceID", _clrInstanceId);
            yield return new KeyValuePair<string, object>("ClientSequenceNumber", _clientSequenceNumber);
        }
    }

    private sealed class GcEndV1Payload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("Count", NetTraceTypeCode.UInt32),
            new("Depth", NetTraceTypeCode.UInt32),
            new("ClrInstanceID", NetTraceTypeCode.UInt16),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new GcEndV1Payload(
                reader.ReadUInt32(),
                reader.ReadUInt32(),
                reader.ReadUInt16());
        }

        private readonly uint _count;
        private readonly uint _depth;
        private readonly ushort _clrInstanceId;

        private GcEndV1Payload(uint count, uint depth, ushort clrInstanceId)
        {
            _count = count;
            _depth = depth;
            _clrInstanceId = clrInstanceId;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "Count":
                    value = _count;
                    return true;
                case "Depth":
                    value = _depth;
                    return true;
                case "ClrInstanceID":
                    value = _clrInstanceId;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("Count", _count);
            yield return new KeyValuePair<string, object>("Depth", _depth);
            yield return new KeyValuePair<string, object>("ClrInstanceID", _clrInstanceId);
        }
    }

    private sealed class GcNoUserDataPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("ClrInstanceID", NetTraceTypeCode.UInt16),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new GcNoUserDataPayload(
                reader.ReadUInt16());
        }

        private readonly ushort _clrInstanceId;

        private GcNoUserDataPayload(ushort clrInstanceId)
        {
            _clrInstanceId = clrInstanceId;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "ClrInstanceID":
                    value = _clrInstanceId;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("ClrInstanceID", _clrInstanceId);
        }
    }

    private sealed class GcHeapStatsV2Payload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("GenerationSize0", NetTraceTypeCode.UInt64),
            new("TotalPromotedSize0", NetTraceTypeCode.UInt64),
            new("GenerationSize1", NetTraceTypeCode.UInt64),
            new("TotalPromotedSize1", NetTraceTypeCode.UInt64),
            new("GenerationSize2", NetTraceTypeCode.UInt64),
            new("TotalPromotedSize2", NetTraceTypeCode.UInt64),
            new("GenerationSize3", NetTraceTypeCode.UInt64),
            new("TotalPromotedSize3", NetTraceTypeCode.UInt64),
            new("FinalizationPromotedSize", NetTraceTypeCode.UInt64),
            new("FinalizationPromotedCount", NetTraceTypeCode.UInt64),
            new("PinnedObjectCount", NetTraceTypeCode.UInt32),
            new("SinkBlockCount", NetTraceTypeCode.UInt32),
            new("GCHandleCount", NetTraceTypeCode.UInt32),
            new("ClrInstanceID", NetTraceTypeCode.UInt16),
            new("GenerationSize4", NetTraceTypeCode.UInt64),
            new("TotalPromotedSize4", NetTraceTypeCode.UInt64),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new GcHeapStatsV2Payload(
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadUInt32(),
                reader.ReadUInt32(),
                reader.ReadUInt32(),
                reader.ReadUInt16(),
                reader.ReadUInt64(),
                reader.ReadUInt64());
        }

        private readonly ulong _generationSize0;
        private readonly ulong _totalPromotedSize0;
        private readonly ulong _generationSize1;
        private readonly ulong _totalPromotedSize1;
        private readonly ulong _generationSize2;
        private readonly ulong _totalPromotedSize2;
        private readonly ulong _generationSize3;
        private readonly ulong _totalPromotedSize3;
        private readonly ulong _finalizationPromotedSize;
        private readonly ulong _finalizationPromotedCount;
        private readonly uint _pinnedObjectCount;
        private readonly uint _sinkBlockCount;
        private readonly uint _gcHandleCount;
        private readonly ushort _clrInstanceId;
        private readonly ulong _generationSize4;
        private readonly ulong _totalPromotedSize4;

        private GcHeapStatsV2Payload(ulong generationSize0, ulong totalPromotedSize0, ulong generationSize1,
            ulong totalPromotedSize1, ulong generationSize2, ulong totalPromotedSize2, ulong generationSize3,
            ulong totalPromotedSize3, ulong finalizationPromotedSize, ulong finalizationPromotedCount,
            uint pinnedObjectCount, uint sinkBlockCount, uint gcHandleCount, ushort clrInstanceId,
            ulong generationSize4, ulong totalPromotedSize4)
        {
            _generationSize0 = generationSize0;
            _totalPromotedSize0 = totalPromotedSize0;
            _generationSize1 = generationSize1;
            _totalPromotedSize1 = totalPromotedSize1;
            _generationSize2 = generationSize2;
            _totalPromotedSize2 = totalPromotedSize2;
            _generationSize3 = generationSize3;
            _totalPromotedSize3 = totalPromotedSize3;
            _finalizationPromotedSize = finalizationPromotedSize;
            _finalizationPromotedCount = finalizationPromotedCount;
            _pinnedObjectCount = pinnedObjectCount;
            _sinkBlockCount = sinkBlockCount;
            _gcHandleCount = gcHandleCount;
            _clrInstanceId = clrInstanceId;
            _generationSize4 = generationSize4;
            _totalPromotedSize4 = totalPromotedSize4;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "GenerationSize0":
                    value = _generationSize0;
                    return true;
                case "TotalPromotedSize0":
                    value = _totalPromotedSize0;
                    return true;
                case "GenerationSize1":
                    value = _generationSize1;
                    return true;
                case "TotalPromotedSize1":
                    value = _totalPromotedSize1;
                    return true;
                case "GenerationSize2":
                    value = _generationSize2;
                    return true;
                case "TotalPromotedSize2":
                    value = _totalPromotedSize2;
                    return true;
                case "GenerationSize3":
                    value = _generationSize3;
                    return true;
                case "TotalPromotedSize3":
                    value = _totalPromotedSize3;
                    return true;
                case "FinalizationPromotedSize":
                    value = _finalizationPromotedSize;
                    return true;
                case "FinalizationPromotedCount":
                    value = _finalizationPromotedCount;
                    return true;
                case "PinnedObjectCount":
                    value = _pinnedObjectCount;
                    return true;
                case "SinkBlockCount":
                    value = _sinkBlockCount;
                    return true;
                case "GCHandleCount":
                    value = _gcHandleCount;
                    return true;
                case "ClrInstanceID":
                    value = _clrInstanceId;
                    return true;
                case "GenerationSize4":
                    value = _generationSize4;
                    return true;
                case "TotalPromotedSize4":
                    value = _totalPromotedSize4;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("GenerationSize0", _generationSize0);
            yield return new KeyValuePair<string, object>("TotalPromotedSize0", _totalPromotedSize0);
            yield return new KeyValuePair<string, object>("GenerationSize1", _generationSize1);
            yield return new KeyValuePair<string, object>("TotalPromotedSize1", _totalPromotedSize1);
            yield return new KeyValuePair<string, object>("GenerationSize2", _generationSize2);
            yield return new KeyValuePair<string, object>("TotalPromotedSize2", _totalPromotedSize2);
            yield return new KeyValuePair<string, object>("GenerationSize3", _generationSize3);
            yield return new KeyValuePair<string, object>("TotalPromotedSize3", _totalPromotedSize3);
            yield return new KeyValuePair<string, object>("FinalizationPromotedSize", _finalizationPromotedSize);
            yield return new KeyValuePair<string, object>("FinalizationPromotedCount", _finalizationPromotedCount);
            yield return new KeyValuePair<string, object>("PinnedObjectCount", _pinnedObjectCount);
            yield return new KeyValuePair<string, object>("SinkBlockCount", _sinkBlockCount);
            yield return new KeyValuePair<string, object>("GCHandleCount", _gcHandleCount);
            yield return new KeyValuePair<string, object>("ClrInstanceID", _clrInstanceId);
            yield return new KeyValuePair<string, object>("GenerationSize4", _generationSize4);
            yield return new KeyValuePair<string, object>("TotalPromotedSize4", _totalPromotedSize4);
        }
    }

    private sealed class GcSuspendEeV1Payload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("Reason", NetTraceTypeCode.UInt32),
            new("Count", NetTraceTypeCode.UInt32),
            new("ClrInstanceID", NetTraceTypeCode.UInt16),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new GcSuspendEeV1Payload(
                reader.ReadUInt32(),
                reader.ReadUInt32(),
                reader.ReadUInt16());
        }

        private readonly uint _reason;
        private readonly uint _count;
        private readonly ushort _clrInstanceId;

        private GcSuspendEeV1Payload(uint reason, uint count, ushort clrInstanceId)
        {
            _reason = reason;
            _count = count;
            _clrInstanceId = clrInstanceId;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "Reason":
                    value = _reason;
                    return true;
                case "Count":
                    value = _count;
                    return true;
                case "ClrInstanceID":
                    value = _clrInstanceId;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("Reason", _reason);
            yield return new KeyValuePair<string, object>("Count", _count);
            yield return new KeyValuePair<string, object>("ClrInstanceID", _clrInstanceId);
        }
    }

    private sealed class GcAllocationTickV4Payload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("AllocationAmount", NetTraceTypeCode.UInt32),
            new("AllocationKind", NetTraceTypeCode.UInt32),
            new("ClrInstanceID", NetTraceTypeCode.UInt16),
            new("AllocationAmount64", NetTraceTypeCode.UInt64),
            new("TypeID", NetTraceTypeCode.UInt64),
            new("TypeName", NetTraceTypeCode.NullTerminatedUtf16String),
            new("HeapIndex", NetTraceTypeCode.UInt32),
            new("Address", NetTraceTypeCode.UInt64),
            new("ObjectSize", NetTraceTypeCode.UInt64),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new GcAllocationTickV4Payload(
                reader.ReadUInt32(),
                reader.ReadUInt32(),
                reader.ReadUInt16(),
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadUInt32(),
                reader.ReadUInt64(),
                reader.ReadUInt64());
        }

        private readonly uint _allocationAmount;
        private readonly uint _allocationKind;
        private readonly ushort _clrInstanceId;
        private readonly ulong _allocationAmount64;
        private readonly ulong _typeId;
        private readonly string _typeName;
        private readonly uint _heapIndex;
        private readonly ulong _address;
        private readonly ulong _objectSize;

        private GcAllocationTickV4Payload(uint allocationAmount, uint allocationKind, ushort clrInstanceId,
            ulong allocationAmount64, ulong typeId, string typeName, uint heapIndex, ulong address, ulong objectSize)
        {
            _allocationAmount = allocationAmount;
            _allocationKind = allocationKind;
            _clrInstanceId = clrInstanceId;
            _allocationAmount64 = allocationAmount64;
            _typeId = typeId;
            _typeName = typeName;
            _heapIndex = heapIndex;
            _address = address;
            _objectSize = objectSize;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "AllocationAmount":
                    value = _allocationAmount;
                    return true;
                case "AllocationKind":
                    value = _allocationKind;
                    return true;
                case "ClrInstanceID":
                    value = _clrInstanceId;
                    return true;
                case "AllocationAmount64":
                    value = _allocationAmount64;
                    return true;
                case "TypeID":
                    value = _typeId;
                    return true;
                case "TypeName":
                    value = _typeName;
                    return true;
                case "HeapIndex":
                    value = _heapIndex;
                    return true;
                case "Address":
                    value = _address;
                    return true;
                case "ObjectSize":
                    value = _objectSize;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("AllocationAmount", _allocationAmount);
            yield return new KeyValuePair<string, object>("AllocationKind", _allocationKind);
            yield return new KeyValuePair<string, object>("ClrInstanceID", _clrInstanceId);
            yield return new KeyValuePair<string, object>("AllocationAmount64", _allocationAmount64);
            yield return new KeyValuePair<string, object>("TypeID", _typeId);
            yield return new KeyValuePair<string, object>("TypeName", _typeName);
            yield return new KeyValuePair<string, object>("HeapIndex", _heapIndex);
            yield return new KeyValuePair<string, object>("Address", _address);
            yield return new KeyValuePair<string, object>("ObjectSize", _objectSize);
        }
    }

    private sealed class GcFinalizersEndV1Payload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("Count", NetTraceTypeCode.UInt32),
            new("ClrInstanceID", NetTraceTypeCode.UInt16),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new GcFinalizersEndV1Payload(
                reader.ReadUInt32(),
                reader.ReadUInt16());
        }

        private readonly uint _count;
        private readonly ushort _clrInstanceId;

        private GcFinalizersEndV1Payload(uint count, ushort clrInstanceId)
        {
            _count = count;
            _clrInstanceId = clrInstanceId;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "Count":
                    value = _count;
                    return true;
                case "ClrInstanceID":
                    value = _clrInstanceId;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("Count", _count);
            yield return new KeyValuePair<string, object>("ClrInstanceID", _clrInstanceId);
        }
    }

    private sealed class GcGenerationRangePayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("Generation", NetTraceTypeCode.Byte),
            new("RangeStart", NetTraceTypeCode.UInt64),
            new("RangeUsedLength", NetTraceTypeCode.UInt64),
            new("RangeReservedLength", NetTraceTypeCode.UInt64),
            new("ClrInstanceID", NetTraceTypeCode.UInt16),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new GcGenerationRangePayload(
                reader.ReadByte(),
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadUInt16());
        }

        private readonly byte _generation;
        private readonly ulong _rangeStart;
        private readonly ulong _rangeUsedLength;
        private readonly ulong _rangeReservedLength;
        private readonly ushort _clrInstanceId;

        private GcGenerationRangePayload(byte generation, ulong rangeStart, ulong rangeUsedLength,
            ulong rangeReservedLength, ushort clrInstanceId)
        {
            _generation = generation;
            _rangeStart = rangeStart;
            _rangeUsedLength = rangeUsedLength;
            _rangeReservedLength = rangeReservedLength;
            _clrInstanceId = clrInstanceId;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "Generation":
                    value = _generation;
                    return true;
                case "RangeStart":
                    value = _rangeStart;
                    return true;
                case "RangeUsedLength":
                    value = _rangeUsedLength;
                    return true;
                case "RangeReservedLength":
                    value = _rangeReservedLength;
                    return true;
                case "ClrInstanceID":
                    value = _clrInstanceId;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("Generation", _generation);
            yield return new KeyValuePair<string, object>("RangeStart", _rangeStart);
            yield return new KeyValuePair<string, object>("RangeUsedLength", _rangeUsedLength);
            yield return new KeyValuePair<string, object>("RangeReservedLength", _rangeReservedLength);
            yield return new KeyValuePair<string, object>("ClrInstanceID", _clrInstanceId);
        }
    }

    private sealed class FinalizeObjectPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("TypeID", NetTraceTypeCode.UInt64),
            new("ObjectID", NetTraceTypeCode.UInt64),
            new("ClrInstanceID", NetTraceTypeCode.UInt16),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new FinalizeObjectPayload(
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadUInt16());
        }

        private readonly ulong _typeId;
        private readonly ulong _objectId;
        private readonly ushort _clrInstanceId;

        private FinalizeObjectPayload(ulong typeId, ulong objectId, ushort clrInstanceId)
        {
            _typeId = typeId;
            _objectId = objectId;
            _clrInstanceId = clrInstanceId;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "TypeID":
                    value = _typeId;
                    return true;
                case "ObjectID":
                    value = _objectId;
                    return true;
                case "ClrInstanceID":
                    value = _clrInstanceId;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("TypeID", _typeId);
            yield return new KeyValuePair<string, object>("ObjectID", _objectId);
            yield return new KeyValuePair<string, object>("ClrInstanceID", _clrInstanceId);
        }
    }

    private sealed class SetGcHandlePayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("HandleID", NetTraceTypeCode.UInt64),
            new("ObjectID", NetTraceTypeCode.UInt64),
            new("Kind", NetTraceTypeCode.UInt32),
            new("Generation", NetTraceTypeCode.UInt32),
            new("AppDomainID", NetTraceTypeCode.UInt64),
            new("ClrInstanceID", NetTraceTypeCode.UInt16),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new SetGcHandlePayload(
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadUInt32(),
                reader.ReadUInt32(),
                reader.ReadUInt64(),
                reader.ReadUInt16());
        }

        private readonly ulong _handleId;
        private readonly ulong _objectId;
        private readonly uint _kind;
        private readonly uint _generation;
        private readonly ulong _appDomainId;
        private readonly ushort _clrInstanceId;

        private SetGcHandlePayload(ulong handleId, ulong objectId, uint kind, uint generation, ulong appDomainId,
            ushort clrInstanceId)
        {
            _handleId = handleId;
            _objectId = objectId;
            _kind = kind;
            _generation = generation;
            _appDomainId = appDomainId;
            _clrInstanceId = clrInstanceId;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "HandleID":
                    value = _handleId;
                    return true;
                case "ObjectID":
                    value = _objectId;
                    return true;
                case "Kind":
                    value = _kind;
                    return true;
                case "Generation":
                    value = _generation;
                    return true;
                case "AppDomainID":
                    value = _appDomainId;
                    return true;
                case "ClrInstanceID":
                    value = _clrInstanceId;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("HandleID", _handleId);
            yield return new KeyValuePair<string, object>("ObjectID", _objectId);
            yield return new KeyValuePair<string, object>("Kind", _kind);
            yield return new KeyValuePair<string, object>("Generation", _generation);
            yield return new KeyValuePair<string, object>("AppDomainID", _appDomainId);
            yield return new KeyValuePair<string, object>("ClrInstanceID", _clrInstanceId);
        }
    }

    private sealed class DestroyGcHandlePayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("HandleID", NetTraceTypeCode.UInt64),
            new("ClrInstanceID", NetTraceTypeCode.UInt16),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new DestroyGcHandlePayload(
                reader.ReadUInt64(),
                reader.ReadUInt16());
        }

        private readonly ulong _handleId;
        private readonly ushort _clrInstanceId;

        private DestroyGcHandlePayload(ulong handleId, ushort clrInstanceId)
        {
            _handleId = handleId;
            _clrInstanceId = clrInstanceId;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "HandleID":
                    value = _handleId;
                    return true;
                case "ClrInstanceID":
                    value = _clrInstanceId;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("HandleID", _handleId);
            yield return new KeyValuePair<string, object>("ClrInstanceID", _clrInstanceId);
        }
    }

    private sealed class PinObjectAtGcTimePayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("HandleID", NetTraceTypeCode.UInt64),
            new("ObjectID", NetTraceTypeCode.UInt64),
            new("ObjectSize", NetTraceTypeCode.UInt64),
            new("TypeName", NetTraceTypeCode.NullTerminatedUtf16String),
            new("ClrInstanceID", NetTraceTypeCode.UInt16),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new PinObjectAtGcTimePayload(
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadUInt16());
        }

        private readonly ulong _handleId;
        private readonly ulong _objectId;
        private readonly ulong _objectSize;
        private readonly string _typeName;
        private readonly ushort _clrInstanceId;

        private PinObjectAtGcTimePayload(ulong handleId, ulong objectId, ulong objectSize, string typeName,
            ushort clrInstanceId)
        {
            _handleId = handleId;
            _objectId = objectId;
            _objectSize = objectSize;
            _typeName = typeName;
            _clrInstanceId = clrInstanceId;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "HandleID":
                    value = _handleId;
                    return true;
                case "ObjectID":
                    value = _objectId;
                    return true;
                case "ObjectSize":
                    value = _objectSize;
                    return true;
                case "TypeName":
                    value = _typeName;
                    return true;
                case "ClrInstanceID":
                    value = _clrInstanceId;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("HandleID", _handleId);
            yield return new KeyValuePair<string, object>("ObjectID", _objectId);
            yield return new KeyValuePair<string, object>("ObjectSize", _objectSize);
            yield return new KeyValuePair<string, object>("TypeName", _typeName);
            yield return new KeyValuePair<string, object>("ClrInstanceID", _clrInstanceId);
        }
    }

    private sealed class GcTriggeredPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("Reason", NetTraceTypeCode.UInt32),
            new("ClrInstanceID", NetTraceTypeCode.UInt16),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new GcTriggeredPayload(
                reader.ReadUInt32(),
                reader.ReadUInt16());
        }

        private readonly uint _reason;
        private readonly ushort _clrInstanceId;

        private GcTriggeredPayload(uint reason, ushort clrInstanceId)
        {
            _reason = reason;
            _clrInstanceId = clrInstanceId;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "Reason":
                    value = _reason;
                    return true;
                case "ClrInstanceID":
                    value = _clrInstanceId;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("Reason", _reason);
            yield return new KeyValuePair<string, object>("ClrInstanceID", _clrInstanceId);
        }
    }

    private sealed class ThreadPoolWorkerThreadAdjustmentSamplePayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("Throughput", NetTraceTypeCode.Double),
            new("ClrInstanceID", NetTraceTypeCode.UInt16),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new ThreadPoolWorkerThreadAdjustmentSamplePayload(
                reader.ReadDouble(),
                reader.ReadUInt16());
        }

        private readonly double _throughput;
        private readonly ushort _clrInstanceId;

        private ThreadPoolWorkerThreadAdjustmentSamplePayload(double throughput, ushort clrInstanceId)
        {
            _throughput = throughput;
            _clrInstanceId = clrInstanceId;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "Throughput":
                    value = _throughput;
                    return true;
                case "ClrInstanceID":
                    value = _clrInstanceId;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("Throughput", _throughput);
            yield return new KeyValuePair<string, object>("ClrInstanceID", _clrInstanceId);
        }
    }

    private sealed class ThreadPoolWorkerThreadAdjustmentAdjustmentPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("AverageThroughput", NetTraceTypeCode.Double),
            new("NewWorkerThreadCount", NetTraceTypeCode.UInt32),
            new("Reason", NetTraceTypeCode.UInt32),
            new("ClrInstanceID", NetTraceTypeCode.UInt16),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new ThreadPoolWorkerThreadAdjustmentAdjustmentPayload(
                reader.ReadDouble(),
                reader.ReadUInt32(),
                reader.ReadUInt32(),
                reader.ReadUInt16());
        }

        private readonly double _averageThroughput;
        private readonly uint _newWorkerThreadCount;
        private readonly uint _reason;
        private readonly ushort _clrInstanceId;

        private ThreadPoolWorkerThreadAdjustmentAdjustmentPayload(double averageThroughput, uint newWorkerThreadCount,
            uint reason, ushort clrInstanceId)
        {
            _averageThroughput = averageThroughput;
            _newWorkerThreadCount = newWorkerThreadCount;
            _reason = reason;
            _clrInstanceId = clrInstanceId;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "AverageThroughput":
                    value = _averageThroughput;
                    return true;
                case "NewWorkerThreadCount":
                    value = _newWorkerThreadCount;
                    return true;
                case "Reason":
                    value = _reason;
                    return true;
                case "ClrInstanceID":
                    value = _clrInstanceId;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("AverageThroughput", _averageThroughput);
            yield return new KeyValuePair<string, object>("NewWorkerThreadCount", _newWorkerThreadCount);
            yield return new KeyValuePair<string, object>("Reason", _reason);
            yield return new KeyValuePair<string, object>("ClrInstanceID", _clrInstanceId);
        }
    }

    private sealed class ThreadPoolWorkerThreadAdjustmentStatsPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("Duration", NetTraceTypeCode.Double),
            new("Throughput", NetTraceTypeCode.Double),
            new("ThreadWave", NetTraceTypeCode.Double),
            new("ThroughputWave", NetTraceTypeCode.Double),
            new("ThroughputErrorEstimate", NetTraceTypeCode.Double),
            new("AverageThroughputErrorEstimate", NetTraceTypeCode.Double),
            new("ThroughputRatio", NetTraceTypeCode.Double),
            new("Confidence", NetTraceTypeCode.Double),
            new("NewControlSetting", NetTraceTypeCode.Double),
            new("NewThreadWaveMagnitude", NetTraceTypeCode.UInt16),
            new("ClrInstanceID", NetTraceTypeCode.UInt16),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new ThreadPoolWorkerThreadAdjustmentStatsPayload(
                reader.ReadDouble(),
                reader.ReadDouble(),
                reader.ReadDouble(),
                reader.ReadDouble(),
                reader.ReadDouble(),
                reader.ReadDouble(),
                reader.ReadDouble(),
                reader.ReadDouble(),
                reader.ReadDouble(),
                reader.ReadUInt16(),
                reader.ReadUInt16());
        }

        private readonly double _duration;
        private readonly double _throughput;
        private readonly double _threadWave;
        private readonly double _throughputWave;
        private readonly double _throughputErrorEstimate;
        private readonly double _averageThroughputErrorEstimate;
        private readonly double _throughputRatio;
        private readonly double _confidence;
        private readonly double _newControlSetting;
        private readonly ushort _newThreadWaveMagnitude;
        private readonly ushort _clrInstanceId;

        private ThreadPoolWorkerThreadAdjustmentStatsPayload(double duration, double throughput, double threadWave,
            double throughputWave, double throughputErrorEstimate, double averageThroughputErrorEstimate,
            double throughputRatio, double confidence, double newControlSetting, ushort newThreadWaveMagnitude,
            ushort clrInstanceId)
        {
            _duration = duration;
            _throughput = throughput;
            _threadWave = threadWave;
            _throughputWave = throughputWave;
            _throughputErrorEstimate = throughputErrorEstimate;
            _averageThroughputErrorEstimate = averageThroughputErrorEstimate;
            _throughputRatio = throughputRatio;
            _confidence = confidence;
            _newControlSetting = newControlSetting;
            _newThreadWaveMagnitude = newThreadWaveMagnitude;
            _clrInstanceId = clrInstanceId;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "Duration":
                    value = _duration;
                    return true;
                case "Throughput":
                    value = _throughput;
                    return true;
                case "ThreadWave":
                    value = _threadWave;
                    return true;
                case "ThroughputWave":
                    value = _throughputWave;
                    return true;
                case "ThroughputErrorEstimate":
                    value = _throughputErrorEstimate;
                    return true;
                case "AverageThroughputErrorEstimate":
                    value = _averageThroughputErrorEstimate;
                    return true;
                case "ThroughputRatio":
                    value = _throughputRatio;
                    return true;
                case "Confidence":
                    value = _confidence;
                    return true;
                case "NewControlSetting":
                    value = _newControlSetting;
                    return true;
                case "NewThreadWaveMagnitude":
                    value = _newThreadWaveMagnitude;
                    return true;
                case "ClrInstanceID":
                    value = _clrInstanceId;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("Duration", _duration);
            yield return new KeyValuePair<string, object>("Throughput", _throughput);
            yield return new KeyValuePair<string, object>("ThreadWave", _threadWave);
            yield return new KeyValuePair<string, object>("ThroughputWave", _throughputWave);
            yield return new KeyValuePair<string, object>("ThroughputErrorEstimate", _throughputErrorEstimate);
            yield return new KeyValuePair<string, object>("AverageThroughputErrorEstimate", _averageThroughputErrorEstimate);
            yield return new KeyValuePair<string, object>("ThroughputRatio", _throughputRatio);
            yield return new KeyValuePair<string, object>("Confidence", _confidence);
            yield return new KeyValuePair<string, object>("NewControlSetting", _newControlSetting);
            yield return new KeyValuePair<string, object>("NewThreadWaveMagnitude", _newThreadWaveMagnitude);
            yield return new KeyValuePair<string, object>("ClrInstanceID", _clrInstanceId);
        }
    }

    private sealed class ThreadPoolWorkerThreadPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("ActiveWorkerThreadCount", NetTraceTypeCode.UInt32),
            new("RetiredWorkerThreadCount", NetTraceTypeCode.UInt32),
            new("ClrInstanceID", NetTraceTypeCode.UInt16),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new ThreadPoolWorkerThreadPayload(
                reader.ReadUInt32(),
                reader.ReadUInt32(),
                reader.ReadUInt16());
        }

        private readonly uint _activeWorkerThreadCount;
        private readonly uint _retiredWorkerThreadCount;
        private readonly ushort _clrInstanceId;

        private ThreadPoolWorkerThreadPayload(uint activeWorkerThreadCount, uint retiredWorkerThreadCount, ushort clrInstanceId)
        {
            _activeWorkerThreadCount = activeWorkerThreadCount;
            _retiredWorkerThreadCount = retiredWorkerThreadCount;
            _clrInstanceId = clrInstanceId;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "ActiveWorkerThreadCount":
                    value = _activeWorkerThreadCount;
                    return true;
                case "RetiredWorkerThreadCount":
                    value = _retiredWorkerThreadCount;
                    return true;
                case "ClrInstanceID":
                    value = _clrInstanceId;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("ActiveWorkerThreadCount", _activeWorkerThreadCount);
            yield return new KeyValuePair<string, object>("RetiredWorkerThreadCount", _retiredWorkerThreadCount);
            yield return new KeyValuePair<string, object>("ClrInstanceID", _clrInstanceId);
        }
    }

    private sealed class YieldProcessorMeasurementPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("ClrInstanceID", NetTraceTypeCode.UInt16),
            new("NsPerYield", NetTraceTypeCode.Double),
            new("EstablishedNsPerYield", NetTraceTypeCode.Double),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new YieldProcessorMeasurementPayload(
                reader.ReadUInt16(),
                reader.ReadDouble(),
                reader.ReadDouble());
        }

        private readonly ushort _clrInstanceId;
        private readonly double _nsPerYield;
        private readonly double _establishedNsPerYield;

        private YieldProcessorMeasurementPayload(ushort clrInstanceId, double nsPerYield, double establishedNsPerYield)
        {
            _clrInstanceId = clrInstanceId;
            _nsPerYield = nsPerYield;
            _establishedNsPerYield = establishedNsPerYield;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "ClrInstanceID":
                    value = _clrInstanceId;
                    return true;
                case "NsPerYield":
                    value = _nsPerYield;
                    return true;
                case "EstablishedNsPerYield":
                    value = _establishedNsPerYield;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("ClrInstanceID", _clrInstanceId);
            yield return new KeyValuePair<string, object>("NsPerYield", _nsPerYield);
            yield return new KeyValuePair<string, object>("EstablishedNsPerYield", _establishedNsPerYield);
        }
    }

    private sealed class ExceptionPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("ExceptionType", NetTraceTypeCode.NullTerminatedUtf16String),
            new("ExceptionMessage", NetTraceTypeCode.NullTerminatedUtf16String),
            new("ExceptionEIP", NetTraceTypeCode.UInt64),
            new("ExceptionHRESULT", NetTraceTypeCode.UInt32),
            new("ExceptionFlags", NetTraceTypeCode.UInt16),
            new("ClrInstanceID", NetTraceTypeCode.UInt16),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new ExceptionPayload(
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadUInt64(),
                reader.ReadUInt32(),
                reader.ReadUInt16(),
                reader.ReadUInt16());
        }

        private readonly string _exceptionType;
        private readonly string _exceptionMessage;
        private readonly ulong _exceptionEip;
        private readonly uint _exceptionHResult;
        private readonly ushort _exceptionFlags;
        private readonly ushort _clrInstanceId;

        private ExceptionPayload(string exceptionType, string exceptionMessage, ulong exceptionEip,
            uint exceptionHResult, ushort exceptionFlags, ushort clrInstanceId)
        {
            _exceptionType = exceptionType;
            _exceptionMessage = exceptionMessage;
            _exceptionEip = exceptionEip;
            _exceptionHResult = exceptionHResult;
            _exceptionFlags = exceptionFlags;
            _clrInstanceId = clrInstanceId;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "ExceptionType":
                    value = _exceptionType;
                    return true;
                case "ExceptionMessage":
                    value = _exceptionMessage;
                    return true;
                case "ExceptionEIP":
                    value = _exceptionEip;
                    return true;
                case "ExceptionHRESULT":
                    value = _exceptionHResult;
                    return true;
                case "ExceptionFlags":
                    value = _exceptionFlags;
                    return true;
                case "ClrInstanceID":
                    value = _clrInstanceId;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("ExceptionType", _exceptionType);
            yield return new KeyValuePair<string, object>("ExceptionMessage", _exceptionMessage);
            yield return new KeyValuePair<string, object>("ExceptionEIP", _exceptionEip);
            yield return new KeyValuePair<string, object>("ExceptionHRESULT", _exceptionHResult);
            yield return new KeyValuePair<string, object>("ExceptionFlags", _exceptionFlags);
            yield return new KeyValuePair<string, object>("ClrInstanceID", _clrInstanceId);
        }
    }

    private sealed class ContentionStartPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("ContentionFlags", NetTraceTypeCode.Byte),
            new("ClrInstanceID", NetTraceTypeCode.UInt16),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new ContentionStartPayload(
                reader.ReadByte(),
                reader.ReadUInt16());
        }

        private readonly byte _contentionFlags;
        private readonly ushort _clrInstanceId;

        private ContentionStartPayload(byte contentionFlags, ushort clrInstanceId)
        {
            _contentionFlags = contentionFlags;
            _clrInstanceId = clrInstanceId;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "ContentionFlags":
                    value = _contentionFlags;
                    return true;
                case "ClrInstanceID":
                    value = _clrInstanceId;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("ContentionFlags", _contentionFlags);
            yield return new KeyValuePair<string, object>("ClrInstanceID", _clrInstanceId);
        }
    }

    private sealed class ContentionStartV2Payload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("ContentionFlags", NetTraceTypeCode.Byte),
            new("ClrInstanceID", NetTraceTypeCode.UInt16),
            new("LockID", NetTraceTypeCode.UInt64),
            new("AssociatedObjectID", NetTraceTypeCode.UInt64),
            new("LockOwnerThreadID", NetTraceTypeCode.UInt64),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new ContentionStartV2Payload(
                reader.ReadByte(),
                reader.ReadUInt16(),
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadUInt64());
        }

        private readonly byte _contentionFlags;
        private readonly ushort _clrInstanceId;
        private readonly ulong _lockId;
        private readonly ulong _associatedObjectId;
        private readonly ulong _lockOwnerThreadId;

        private ContentionStartV2Payload(byte contentionFlags, ushort clrInstanceId, ulong lockId, ulong associatedObjectId,
            ulong lockOwnerThreadId)
        {
            _contentionFlags = contentionFlags;
            _clrInstanceId = clrInstanceId;
            _lockId = lockId;
            _associatedObjectId = associatedObjectId;
            _lockOwnerThreadId = lockOwnerThreadId;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "ContentionFlags":
                    value = _contentionFlags;
                    return true;
                case "ClrInstanceID":
                    value = _clrInstanceId;
                    return true;
                case "LockID":
                    value = _lockId;
                    return true;
                case "AssociatedObjectID":
                    value = _associatedObjectId;
                    return true;
                case "LockOwnerThreadID":
                    value = _lockOwnerThreadId;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("ContentionFlags", _contentionFlags);
            yield return new KeyValuePair<string, object>("ClrInstanceID", _clrInstanceId);
            yield return new KeyValuePair<string, object>("LockID", _lockId);
            yield return new KeyValuePair<string, object>("AssociatedObjectID", _associatedObjectId);
            yield return new KeyValuePair<string, object>("LockOwnerThreadID", _lockOwnerThreadId);
        }
    }

    private sealed class ThreadCreatedPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("ManagedThreadID", NetTraceTypeCode.UInt64),
            new("AppDomainID", NetTraceTypeCode.UInt64),
            new("Flags", NetTraceTypeCode.UInt32),
            new("ManagedThreadIndex", NetTraceTypeCode.UInt32),
            new("OSThreadID", NetTraceTypeCode.UInt32),
            new("ClrInstanceID", NetTraceTypeCode.UInt16),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new ThreadCreatedPayload(
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadUInt32(),
                reader.ReadUInt32(),
                reader.ReadUInt32(),
                reader.ReadUInt16());
        }

        private readonly ulong _managedThreadId;
        private readonly ulong _appDomainId;
        private readonly uint _flags;
        private readonly uint _managedThreadIndex;
        private readonly uint _osThreadId;
        private readonly ushort _clrInstanceId;

        private ThreadCreatedPayload(ulong managedThreadId, ulong appDomainId, uint flags, uint managedThreadIndex,
            uint osThreadId, ushort clrInstanceId)
        {
            _managedThreadId = managedThreadId;
            _appDomainId = appDomainId;
            _flags = flags;
            _managedThreadIndex = managedThreadIndex;
            _osThreadId = osThreadId;
            _clrInstanceId = clrInstanceId;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "ManagedThreadID":
                    value = _managedThreadId;
                    return true;
                case "AppDomainID":
                    value = _appDomainId;
                    return true;
                case "Flags":
                    value = _flags;
                    return true;
                case "ManagedThreadIndex":
                    value = _managedThreadIndex;
                    return true;
                case "OSThreadID":
                    value = _osThreadId;
                    return true;
                case "ClrInstanceID":
                    value = _clrInstanceId;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("ManagedThreadID", _managedThreadId);
            yield return new KeyValuePair<string, object>("AppDomainID", _appDomainId);
            yield return new KeyValuePair<string, object>("Flags", _flags);
            yield return new KeyValuePair<string, object>("ManagedThreadIndex", _managedThreadIndex);
            yield return new KeyValuePair<string, object>("OSThreadID", _osThreadId);
            yield return new KeyValuePair<string, object>("ClrInstanceID", _clrInstanceId);
        }
    }

    private sealed class IlStubGeneratedPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("ClrInstanceID", NetTraceTypeCode.UInt16),
            new("ModuleID", NetTraceTypeCode.UInt64),
            new("StubMethodID", NetTraceTypeCode.UInt64),
            new("StubFlags", NetTraceTypeCode.UInt32),
            new("ManagedInteropMethodToken", NetTraceTypeCode.UInt32),
            new("ManagedInteropMethodNamespace", NetTraceTypeCode.NullTerminatedUtf16String),
            new("ManagedInteropMethodName", NetTraceTypeCode.NullTerminatedUtf16String),
            new("ManagedInteropMethodSignature", NetTraceTypeCode.NullTerminatedUtf16String),
            new("NativeMethodSignature", NetTraceTypeCode.NullTerminatedUtf16String),
            new("StubMethodSignature", NetTraceTypeCode.NullTerminatedUtf16String),
            new("StubMethodILCode", NetTraceTypeCode.NullTerminatedUtf16String),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new IlStubGeneratedPayload(
                reader.ReadUInt16(),
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadUInt32(),
                reader.ReadUInt32(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String());
        }

        private readonly ushort _clrInstanceId;
        private readonly ulong _moduleId;
        private readonly ulong _stubMethodId;
        private readonly uint _stubFlags;
        private readonly uint _managedInteropMethodToken;
        private readonly string _managedInteropMethodNamespace;
        private readonly string _managedInteropMethodName;
        private readonly string _managedInteropMethodSignature;
        private readonly string _nativeMethodSignature;
        private readonly string _stubMethodSignature;
        private readonly string _stubMethodIlCode;

        private IlStubGeneratedPayload(ushort clrInstanceId, ulong moduleId, ulong stubMethodId, uint stubFlags,
            uint managedInteropMethodToken, string managedInteropMethodNamespace, string managedInteropMethodName,
            string managedInteropMethodSignature, string nativeMethodSignature, string stubMethodSignature,
            string stubMethodIlCode)
        {
            _clrInstanceId = clrInstanceId;
            _moduleId = moduleId;
            _stubMethodId = stubMethodId;
            _stubFlags = stubFlags;
            _managedInteropMethodToken = managedInteropMethodToken;
            _managedInteropMethodNamespace = managedInteropMethodNamespace;
            _managedInteropMethodName = managedInteropMethodName;
            _managedInteropMethodSignature = managedInteropMethodSignature;
            _nativeMethodSignature = nativeMethodSignature;
            _stubMethodSignature = stubMethodSignature;
            _stubMethodIlCode = stubMethodIlCode;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "ClrInstanceID":
                    value = _clrInstanceId;
                    return true;
                case "ModuleID":
                    value = _moduleId;
                    return true;
                case "StubMethodID":
                    value = _stubMethodId;
                    return true;
                case "StubFlags":
                    value = _stubFlags;
                    return true;
                case "ManagedInteropMethodToken":
                    value = _managedInteropMethodToken;
                    return true;
                case "ManagedInteropMethodNamespace":
                    value = _managedInteropMethodNamespace;
                    return true;
                case "ManagedInteropMethodName":
                    value = _managedInteropMethodName;
                    return true;
                case "ManagedInteropMethodSignature":
                    value = _managedInteropMethodSignature;
                    return true;
                case "NativeMethodSignature":
                    value = _nativeMethodSignature;
                    return true;
                case "StubMethodSignature":
                    value = _stubMethodSignature;
                    return true;
                case "StubMethodILCode":
                    value = _stubMethodIlCode;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("ClrInstanceID", _clrInstanceId);
            yield return new KeyValuePair<string, object>("ModuleID", _moduleId);
            yield return new KeyValuePair<string, object>("StubMethodID", _stubMethodId);
            yield return new KeyValuePair<string, object>("StubFlags", _stubFlags);
            yield return new KeyValuePair<string, object>("ManagedInteropMethodToken", _managedInteropMethodToken);
            yield return new KeyValuePair<string, object>("ManagedInteropMethodNamespace", _managedInteropMethodNamespace);
            yield return new KeyValuePair<string, object>("ManagedInteropMethodName", _managedInteropMethodName);
            yield return new KeyValuePair<string, object>("ManagedInteropMethodSignature", _managedInteropMethodSignature);
            yield return new KeyValuePair<string, object>("NativeMethodSignature", _nativeMethodSignature);
            yield return new KeyValuePair<string, object>("StubMethodSignature", _stubMethodSignature);
            yield return new KeyValuePair<string, object>("StubMethodILCode", _stubMethodIlCode);
        }
    }

    private sealed class ContentionStopV1Payload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("ContentionFlags", NetTraceTypeCode.Byte),
            new("ClrInstanceID", NetTraceTypeCode.UInt16),
            new("DurationNs", NetTraceTypeCode.Double),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new ContentionStopV1Payload(
                reader.ReadByte(),
                reader.ReadUInt16(),
                reader.ReadDouble());
        }

        private readonly byte _contentionFlags;
        private readonly ushort _clrInstanceId;
        private readonly double _durationNs;

        private ContentionStopV1Payload(byte contentionFlags, ushort clrInstanceId, double durationNs)
        {
            _contentionFlags = contentionFlags;
            _clrInstanceId = clrInstanceId;
            _durationNs = durationNs;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "ContentionFlags":
                    value = _contentionFlags;
                    return true;
                case "ClrInstanceID":
                    value = _clrInstanceId;
                    return true;
                case "DurationNs":
                    value = _durationNs;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("ContentionFlags", _contentionFlags);
            yield return new KeyValuePair<string, object>("ClrInstanceID", _clrInstanceId);
            yield return new KeyValuePair<string, object>("DurationNs", _durationNs);
        }
    }

    private sealed class MethodLoadUnloadVerboseV1Payload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("MethodID", NetTraceTypeCode.UInt64),
            new("ModuleID", NetTraceTypeCode.UInt64),
            new("MethodStartAddress", NetTraceTypeCode.UInt64),
            new("MethodSize", NetTraceTypeCode.UInt32),
            new("MethodToken", NetTraceTypeCode.UInt32),
            new("MethodFlags", NetTraceTypeCode.UInt32),
            new("MethodNamespace", NetTraceTypeCode.NullTerminatedUtf16String),
            new("MethodName", NetTraceTypeCode.NullTerminatedUtf16String),
            new("MethodSignature", NetTraceTypeCode.NullTerminatedUtf16String),
            new("ClrInstanceID", NetTraceTypeCode.UInt16),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new MethodLoadUnloadVerboseV1Payload(
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadUInt32(),
                reader.ReadUInt32(),
                reader.ReadUInt32(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadUInt16());
        }

        private readonly ulong _methodId;
        private readonly ulong _moduleId;
        private readonly ulong _methodStartAddress;
        private readonly uint _methodSize;
        private readonly uint _methodToken;
        private readonly uint _methodFlags;
        private readonly string _methodNamespace;
        private readonly string _methodName;
        private readonly string _methodSignature;
        private readonly ushort _clrInstanceId;

        private MethodLoadUnloadVerboseV1Payload(ulong methodId, ulong moduleId, ulong methodStartAddress,
            uint methodSize, uint methodToken, uint methodFlags, string methodNamespace, string methodName,
            string methodSignature, ushort clrInstanceId)
        {
            _methodId = methodId;
            _moduleId = moduleId;
            _methodStartAddress = methodStartAddress;
            _methodSize = methodSize;
            _methodToken = methodToken;
            _methodFlags = methodFlags;
            _methodNamespace = methodNamespace;
            _methodName = methodName;
            _methodSignature = methodSignature;
            _clrInstanceId = clrInstanceId;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "MethodID":
                    value = _methodId;
                    return true;
                case "ModuleID":
                    value = _moduleId;
                    return true;
                case "MethodStartAddress":
                    value = _methodStartAddress;
                    return true;
                case "MethodSize":
                    value = _methodSize;
                    return true;
                case "MethodToken":
                    value = _methodToken;
                    return true;
                case "MethodFlags":
                    value = _methodFlags;
                    return true;
                case "MethodNamespace":
                    value = _methodNamespace;
                    return true;
                case "MethodName":
                    value = _methodName;
                    return true;
                case "MethodSignature":
                    value = _methodSignature;
                    return true;
                case "ClrInstanceID":
                    value = _clrInstanceId;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("MethodID", _methodId);
            yield return new KeyValuePair<string, object>("ModuleID", _moduleId);
            yield return new KeyValuePair<string, object>("MethodStartAddress", _methodStartAddress);
            yield return new KeyValuePair<string, object>("MethodSize", _methodSize);
            yield return new KeyValuePair<string, object>("MethodToken", _methodToken);
            yield return new KeyValuePair<string, object>("MethodFlags", _methodFlags);
            yield return new KeyValuePair<string, object>("MethodNamespace", _methodNamespace);
            yield return new KeyValuePair<string, object>("MethodName", _methodName);
            yield return new KeyValuePair<string, object>("MethodSignature", _methodSignature);
            yield return new KeyValuePair<string, object>("ClrInstanceID", _clrInstanceId);
        }
    }

    private sealed class MethodLoadUnloadVerboseV2Payload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("MethodID", NetTraceTypeCode.UInt64),
            new("ModuleID", NetTraceTypeCode.UInt64),
            new("MethodStartAddress", NetTraceTypeCode.UInt64),
            new("MethodSize", NetTraceTypeCode.UInt32),
            new("MethodToken", NetTraceTypeCode.UInt32),
            new("MethodFlags", NetTraceTypeCode.UInt32),
            new("MethodNamespace", NetTraceTypeCode.NullTerminatedUtf16String),
            new("MethodName", NetTraceTypeCode.NullTerminatedUtf16String),
            new("MethodSignature", NetTraceTypeCode.NullTerminatedUtf16String),
            new("ClrInstanceID", NetTraceTypeCode.UInt16),
            new("ReJITID", NetTraceTypeCode.UInt64),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new MethodLoadUnloadVerboseV2Payload(
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadUInt32(),
                reader.ReadUInt32(),
                reader.ReadUInt32(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadUInt16(),
                reader.ReadUInt64());
        }

        private readonly ulong _methodId;
        private readonly ulong _moduleId;
        private readonly ulong _methodStartAddress;
        private readonly uint _methodSize;
        private readonly uint _methodToken;
        private readonly uint _methodFlags;
        private readonly string _methodNamespace;
        private readonly string _methodName;
        private readonly string _methodSignature;
        private readonly ushort _clrInstanceId;
        private readonly ulong _reJitId;

        private MethodLoadUnloadVerboseV2Payload(ulong methodId, ulong moduleId, ulong methodStartAddress,
            uint methodSize, uint methodToken, uint methodFlags, string methodNamespace, string methodName,
            string methodSignature, ushort clrInstanceId, ulong reJitId)
        {
            _methodId = methodId;
            _moduleId = moduleId;
            _methodStartAddress = methodStartAddress;
            _methodSize = methodSize;
            _methodToken = methodToken;
            _methodFlags = methodFlags;
            _methodNamespace = methodNamespace;
            _methodName = methodName;
            _methodSignature = methodSignature;
            _clrInstanceId = clrInstanceId;
            _reJitId = reJitId;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "MethodID":
                    value = _methodId;
                    return true;
                case "ModuleID":
                    value = _moduleId;
                    return true;
                case "MethodStartAddress":
                    value = _methodStartAddress;
                    return true;
                case "MethodSize":
                    value = _methodSize;
                    return true;
                case "MethodToken":
                    value = _methodToken;
                    return true;
                case "MethodFlags":
                    value = _methodFlags;
                    return true;
                case "MethodNamespace":
                    value = _methodNamespace;
                    return true;
                case "MethodName":
                    value = _methodName;
                    return true;
                case "MethodSignature":
                    value = _methodSignature;
                    return true;
                case "ClrInstanceID":
                    value = _clrInstanceId;
                    return true;
                case "ReJITID":
                    value = _reJitId;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("MethodID", _methodId);
            yield return new KeyValuePair<string, object>("ModuleID", _moduleId);
            yield return new KeyValuePair<string, object>("MethodStartAddress", _methodStartAddress);
            yield return new KeyValuePair<string, object>("MethodSize", _methodSize);
            yield return new KeyValuePair<string, object>("MethodToken", _methodToken);
            yield return new KeyValuePair<string, object>("MethodFlags", _methodFlags);
            yield return new KeyValuePair<string, object>("MethodNamespace", _methodNamespace);
            yield return new KeyValuePair<string, object>("MethodName", _methodName);
            yield return new KeyValuePair<string, object>("MethodSignature", _methodSignature);
            yield return new KeyValuePair<string, object>("ClrInstanceID", _clrInstanceId);
            yield return new KeyValuePair<string, object>("ReJITID", _reJitId);
        }
    }

    private sealed class MethodJittingStartedV1Payload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("MethodID", NetTraceTypeCode.UInt64),
            new("ModuleID", NetTraceTypeCode.UInt64),
            new("MethodToken", NetTraceTypeCode.UInt32),
            new("MethodILSize", NetTraceTypeCode.UInt32),
            new("MethodNamespace", NetTraceTypeCode.NullTerminatedUtf16String),
            new("MethodName", NetTraceTypeCode.NullTerminatedUtf16String),
            new("MethodSignature", NetTraceTypeCode.NullTerminatedUtf16String),
            new("ClrInstanceID", NetTraceTypeCode.UInt16),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new MethodJittingStartedV1Payload(
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadUInt32(),
                reader.ReadUInt32(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadUInt16());
        }

        private readonly ulong _methodId;
        private readonly ulong _moduleId;
        private readonly uint _methodToken;
        private readonly uint _methodIlSize;
        private readonly string _methodNamespace;
        private readonly string _methodName;
        private readonly string _methodSignature;
        private readonly ushort _clrInstanceId;

        private MethodJittingStartedV1Payload(ulong methodId, ulong moduleId, uint methodToken, uint methodIlSize,
            string methodNamespace, string methodName, string methodSignature, ushort clrInstanceId)
        {
            _methodId = methodId;
            _moduleId = moduleId;
            _methodToken = methodToken;
            _methodIlSize = methodIlSize;
            _methodNamespace = methodNamespace;
            _methodName = methodName;
            _methodSignature = methodSignature;
            _clrInstanceId = clrInstanceId;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "MethodID":
                    value = _methodId;
                    return true;
                case "ModuleID":
                    value = _moduleId;
                    return true;
                case "MethodToken":
                    value = _methodToken;
                    return true;
                case "MethodILSize":
                    value = _methodIlSize;
                    return true;
                case "MethodNamespace":
                    value = _methodNamespace;
                    return true;
                case "MethodName":
                    value = _methodName;
                    return true;
                case "MethodSignature":
                    value = _methodSignature;
                    return true;
                case "ClrInstanceID":
                    value = _clrInstanceId;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("MethodID", _methodId);
            yield return new KeyValuePair<string, object>("ModuleID", _moduleId);
            yield return new KeyValuePair<string, object>("MethodToken", _methodToken);
            yield return new KeyValuePair<string, object>("MethodILSize", _methodIlSize);
            yield return new KeyValuePair<string, object>("MethodNamespace", _methodNamespace);
            yield return new KeyValuePair<string, object>("MethodName", _methodName);
            yield return new KeyValuePair<string, object>("MethodSignature", _methodSignature);
            yield return new KeyValuePair<string, object>("ClrInstanceID", _clrInstanceId);
        }
    }

    private sealed class MethodJitMemoryAllocatedForCodePayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("MethodID", NetTraceTypeCode.UInt64),
            new("ModuleID", NetTraceTypeCode.UInt64),
            new("JitHotCodeRequestSize", NetTraceTypeCode.UInt64),
            new("JitRODataRequestSize", NetTraceTypeCode.UInt64),
            new("AllocatedSizeForJitCode", NetTraceTypeCode.UInt64),
            new("JitAllocFlag", NetTraceTypeCode.UInt32),
            new("ClrInstanceID", NetTraceTypeCode.UInt16),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new MethodJitMemoryAllocatedForCodePayload(
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadUInt32(),
                reader.ReadUInt16());
        }

        private readonly ulong _methodId;
        private readonly ulong _moduleId;
        private readonly ulong _jitHotCodeRequestSize;
        private readonly ulong _jitRoDataRequestSize;
        private readonly ulong _allocatedSizeForJitCode;
        private readonly uint _jitAllocFlag;
        private readonly ushort _clrInstanceId;

        private MethodJitMemoryAllocatedForCodePayload(ulong methodId, ulong moduleId, ulong jitHotCodeRequestSize,
            ulong jitRoDataRequestSize, ulong allocatedSizeForJitCode, uint jitAllocFlag, ushort clrInstanceId)
        {
            _methodId = methodId;
            _moduleId = moduleId;
            _jitHotCodeRequestSize = jitHotCodeRequestSize;
            _jitRoDataRequestSize = jitRoDataRequestSize;
            _allocatedSizeForJitCode = allocatedSizeForJitCode;
            _jitAllocFlag = jitAllocFlag;
            _clrInstanceId = clrInstanceId;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "MethodID":
                    value = _methodId;
                    return true;
                case "ModuleID":
                    value = _moduleId;
                    return true;
                case "JitHotCodeRequestSize":
                    value = _jitHotCodeRequestSize;
                    return true;
                case "JitRODataRequestSize":
                    value = _jitRoDataRequestSize;
                    return true;
                case "AllocatedSizeForJitCode":
                    value = _allocatedSizeForJitCode;
                    return true;
                case "JitAllocFlag":
                    value = _jitAllocFlag;
                    return true;
                case "ClrInstanceID":
                    value = _clrInstanceId;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("MethodID", _methodId);
            yield return new KeyValuePair<string, object>("ModuleID", _moduleId);
            yield return new KeyValuePair<string, object>("JitHotCodeRequestSize", _jitHotCodeRequestSize);
            yield return new KeyValuePair<string, object>("JitRODataRequestSize", _jitRoDataRequestSize);
            yield return new KeyValuePair<string, object>("AllocatedSizeForJitCode", _allocatedSizeForJitCode);
            yield return new KeyValuePair<string, object>("JitAllocFlag", _jitAllocFlag);
            yield return new KeyValuePair<string, object>("ClrInstanceID", _clrInstanceId);
        }
    }

    private sealed class MethodJitInliningSucceededPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("MethodBeingCompiledNamespace", NetTraceTypeCode.NullTerminatedUtf16String),
            new("MethodBeingCompiledName", NetTraceTypeCode.NullTerminatedUtf16String),
            new("MethodBeingCompiledNameSignature", NetTraceTypeCode.NullTerminatedUtf16String),
            new("InlinerNamespace", NetTraceTypeCode.NullTerminatedUtf16String),
            new("InlinerName", NetTraceTypeCode.NullTerminatedUtf16String),
            new("InlinerNameSignature", NetTraceTypeCode.NullTerminatedUtf16String),
            new("InlineeNamespace", NetTraceTypeCode.NullTerminatedUtf16String),
            new("InlineeName", NetTraceTypeCode.NullTerminatedUtf16String),
            new("InlineeNameSignature", NetTraceTypeCode.NullTerminatedUtf16String),
            new("ClrInstanceID", NetTraceTypeCode.UInt16),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new MethodJitInliningSucceededPayload(
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadUInt16());
        }

        private readonly string _methodBeingCompiledNamespace;
        private readonly string _methodBeingCompiledName;
        private readonly string _methodBeingCompiledNameSignature;
        private readonly string _inlinerNamespace;
        private readonly string _inlinerName;
        private readonly string _inlinerNameSignature;
        private readonly string _inlineeNamespace;
        private readonly string _inlineeName;
        private readonly string _inlineeNameSignature;
        private readonly ushort _clrInstanceId;

        private MethodJitInliningSucceededPayload(string methodBeingCompiledNamespace, string methodBeingCompiledName,
            string methodBeingCompiledNameSignature, string inlinerNamespace, string inlinerName,
            string inlinerNameSignature, string inlineeNamespace, string inlineeName, string inlineeNameSignature,
            ushort clrInstanceId)
        {
            _methodBeingCompiledNamespace = methodBeingCompiledNamespace;
            _methodBeingCompiledName = methodBeingCompiledName;
            _methodBeingCompiledNameSignature = methodBeingCompiledNameSignature;
            _inlinerNamespace = inlinerNamespace;
            _inlinerName = inlinerName;
            _inlinerNameSignature = inlinerNameSignature;
            _inlineeNamespace = inlineeNamespace;
            _inlineeName = inlineeName;
            _inlineeNameSignature = inlineeNameSignature;
            _clrInstanceId = clrInstanceId;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "MethodBeingCompiledNamespace":
                    value = _methodBeingCompiledNamespace;
                    return true;
                case "MethodBeingCompiledName":
                    value = _methodBeingCompiledName;
                    return true;
                case "MethodBeingCompiledNameSignature":
                    value = _methodBeingCompiledNameSignature;
                    return true;
                case "InlinerNamespace":
                    value = _inlinerNamespace;
                    return true;
                case "InlinerName":
                    value = _inlinerName;
                    return true;
                case "InlinerNameSignature":
                    value = _inlinerNameSignature;
                    return true;
                case "InlineeNamespace":
                    value = _inlineeNamespace;
                    return true;
                case "InlineeName":
                    value = _inlineeName;
                    return true;
                case "InlineeNameSignature":
                    value = _inlineeNameSignature;
                    return true;
                case "ClrInstanceID":
                    value = _clrInstanceId;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("MethodBeingCompiledNamespace", _methodBeingCompiledNamespace);
            yield return new KeyValuePair<string, object>("MethodBeingCompiledName", _methodBeingCompiledName);
            yield return new KeyValuePair<string, object>("MethodBeingCompiledNameSignature", _methodBeingCompiledNameSignature);
            yield return new KeyValuePair<string, object>("InlinerNamespace", _inlinerNamespace);
            yield return new KeyValuePair<string, object>("InlinerName", _inlinerName);
            yield return new KeyValuePair<string, object>("InlinerNameSignature", _inlinerNameSignature);
            yield return new KeyValuePair<string, object>("InlineeNamespace", _inlineeNamespace);
            yield return new KeyValuePair<string, object>("InlineeName", _inlineeName);
            yield return new KeyValuePair<string, object>("InlineeNameSignature", _inlineeNameSignature);
            yield return new KeyValuePair<string, object>("ClrInstanceID", _clrInstanceId);
        }
    }

    private sealed class MethodJitTailCallSucceededPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("MethodBeingCompiledNamespace", NetTraceTypeCode.NullTerminatedUtf16String),
            new("MethodBeingCompiledName", NetTraceTypeCode.NullTerminatedUtf16String),
            new("MethodBeingCompiledNameSignature", NetTraceTypeCode.NullTerminatedUtf16String),
            new("CallerNamespace", NetTraceTypeCode.NullTerminatedUtf16String),
            new("CallerName", NetTraceTypeCode.NullTerminatedUtf16String),
            new("CallerNameSignature", NetTraceTypeCode.NullTerminatedUtf16String),
            new("CalleeNamespace", NetTraceTypeCode.NullTerminatedUtf16String),
            new("CalleeName", NetTraceTypeCode.NullTerminatedUtf16String),
            new("CalleeNameSignature", NetTraceTypeCode.NullTerminatedUtf16String),
            new("TailPrefix", NetTraceTypeCode.Boolean32),
            new("TailCallType", NetTraceTypeCode.UInt32),
            new("ClrInstanceID", NetTraceTypeCode.UInt16),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new MethodJitTailCallSucceededPayload(
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadInt32() != 0,
                reader.ReadUInt32(),
                reader.ReadUInt16());
        }

        private readonly string _methodBeingCompiledNamespace;
        private readonly string _methodBeingCompiledName;
        private readonly string _methodBeingCompiledNameSignature;
        private readonly string _callerNamespace;
        private readonly string _callerName;
        private readonly string _callerNameSignature;
        private readonly string _calleeNamespace;
        private readonly string _calleeName;
        private readonly string _calleeNameSignature;
        private readonly bool _tailPrefix;
        private readonly uint _tailCallType;
        private readonly ushort _clrInstanceId;

        private MethodJitTailCallSucceededPayload(string methodBeingCompiledNamespace, string methodBeingCompiledName,
            string methodBeingCompiledNameSignature, string callerNamespace, string callerName,
            string callerNameSignature, string calleeNamespace, string calleeName, string calleeNameSignature,
            bool tailPrefix, uint tailCallType, ushort clrInstanceId)
        {
            _methodBeingCompiledNamespace = methodBeingCompiledNamespace;
            _methodBeingCompiledName = methodBeingCompiledName;
            _methodBeingCompiledNameSignature = methodBeingCompiledNameSignature;
            _callerNamespace = callerNamespace;
            _callerName = callerName;
            _callerNameSignature = callerNameSignature;
            _calleeNamespace = calleeNamespace;
            _calleeName = calleeName;
            _calleeNameSignature = calleeNameSignature;
            _tailPrefix = tailPrefix;
            _tailCallType = tailCallType;
            _clrInstanceId = clrInstanceId;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "MethodBeingCompiledNamespace":
                    value = _methodBeingCompiledNamespace;
                    return true;
                case "MethodBeingCompiledName":
                    value = _methodBeingCompiledName;
                    return true;
                case "MethodBeingCompiledNameSignature":
                    value = _methodBeingCompiledNameSignature;
                    return true;
                case "CallerNamespace":
                    value = _callerNamespace;
                    return true;
                case "CallerName":
                    value = _callerName;
                    return true;
                case "CallerNameSignature":
                    value = _callerNameSignature;
                    return true;
                case "CalleeNamespace":
                    value = _calleeNamespace;
                    return true;
                case "CalleeName":
                    value = _calleeName;
                    return true;
                case "CalleeNameSignature":
                    value = _calleeNameSignature;
                    return true;
                case "TailPrefix":
                    value = _tailPrefix;
                    return true;
                case "TailCallType":
                    value = _tailCallType;
                    return true;
                case "ClrInstanceID":
                    value = _clrInstanceId;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("MethodBeingCompiledNamespace", _methodBeingCompiledNamespace);
            yield return new KeyValuePair<string, object>("MethodBeingCompiledName", _methodBeingCompiledName);
            yield return new KeyValuePair<string, object>("MethodBeingCompiledNameSignature", _methodBeingCompiledNameSignature);
            yield return new KeyValuePair<string, object>("CallerNamespace", _callerNamespace);
            yield return new KeyValuePair<string, object>("CallerName", _callerName);
            yield return new KeyValuePair<string, object>("CallerNameSignature", _callerNameSignature);
            yield return new KeyValuePair<string, object>("CalleeNamespace", _calleeNamespace);
            yield return new KeyValuePair<string, object>("CalleeName", _calleeName);
            yield return new KeyValuePair<string, object>("CalleeNameSignature", _calleeNameSignature);
            yield return new KeyValuePair<string, object>("TailPrefix", _tailPrefix);
            yield return new KeyValuePair<string, object>("TailCallType", _tailCallType);
            yield return new KeyValuePair<string, object>("ClrInstanceID", _clrInstanceId);
        }
    }

    private sealed class MethodJitInliningFailedPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("MethodBeingCompiledNamespace", NetTraceTypeCode.NullTerminatedUtf16String),
            new("MethodBeingCompiledName", NetTraceTypeCode.NullTerminatedUtf16String),
            new("MethodBeingCompiledNameSignature", NetTraceTypeCode.NullTerminatedUtf16String),
            new("InlinerNamespace", NetTraceTypeCode.NullTerminatedUtf16String),
            new("InlinerName", NetTraceTypeCode.NullTerminatedUtf16String),
            new("InlinerNameSignature", NetTraceTypeCode.NullTerminatedUtf16String),
            new("InlineeNamespace", NetTraceTypeCode.NullTerminatedUtf16String),
            new("InlineeName", NetTraceTypeCode.NullTerminatedUtf16String),
            new("InlineeNameSignature", NetTraceTypeCode.NullTerminatedUtf16String),
            new("FailAlways", NetTraceTypeCode.Boolean32),
            new("FailReason", NetTraceTypeCode.NullTerminatedUtf16String),
            new("ClrInstanceID", NetTraceTypeCode.UInt16),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new MethodJitInliningFailedPayload(
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadInt32() != 0,
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadUInt16());
        }

        private readonly string _methodBeingCompiledNamespace;
        private readonly string _methodBeingCompiledName;
        private readonly string _methodBeingCompiledNameSignature;
        private readonly string _inlinerNamespace;
        private readonly string _inlinerName;
        private readonly string _inlinerNameSignature;
        private readonly string _inlineeNamespace;
        private readonly string _inlineeName;
        private readonly string _inlineeNameSignature;
        private readonly bool _failAlways;
        private readonly string _failReason;
        private readonly ushort _clrInstanceId;

        private MethodJitInliningFailedPayload(string methodBeingCompiledNamespace, string methodBeingCompiledName,
            string methodBeingCompiledNameSignature, string inlinerNamespace, string inlinerName,
            string inlinerNameSignature, string inlineeNamespace, string inlineeName, string inlineeNameSignature,
            bool failAlways, string failReason, ushort clrInstanceId)
        {
            _methodBeingCompiledNamespace = methodBeingCompiledNamespace;
            _methodBeingCompiledName = methodBeingCompiledName;
            _methodBeingCompiledNameSignature = methodBeingCompiledNameSignature;
            _inlinerNamespace = inlinerNamespace;
            _inlinerName = inlinerName;
            _inlinerNameSignature = inlinerNameSignature;
            _inlineeNamespace = inlineeNamespace;
            _inlineeName = inlineeName;
            _inlineeNameSignature = inlineeNameSignature;
            _failAlways = failAlways;
            _failReason = failReason;
            _clrInstanceId = clrInstanceId;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "MethodBeingCompiledNamespace":
                    value = _methodBeingCompiledNamespace;
                    return true;
                case "MethodBeingCompiledName":
                    value = _methodBeingCompiledName;
                    return true;
                case "MethodBeingCompiledNameSignature":
                    value = _methodBeingCompiledNameSignature;
                    return true;
                case "InlinerNamespace":
                    value = _inlinerNamespace;
                    return true;
                case "InlinerName":
                    value = _inlinerName;
                    return true;
                case "InlinerNameSignature":
                    value = _inlinerNameSignature;
                    return true;
                case "InlineeNamespace":
                    value = _inlineeNamespace;
                    return true;
                case "InlineeName":
                    value = _inlineeName;
                    return true;
                case "InlineeNameSignature":
                    value = _inlineeNameSignature;
                    return true;
                case "FailAlways":
                    value = _failAlways;
                    return true;
                case "FailReason":
                    value = _failReason;
                    return true;
                case "ClrInstanceID":
                    value = _clrInstanceId;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("MethodBeingCompiledNamespace", _methodBeingCompiledNamespace);
            yield return new KeyValuePair<string, object>("MethodBeingCompiledName", _methodBeingCompiledName);
            yield return new KeyValuePair<string, object>("MethodBeingCompiledNameSignature", _methodBeingCompiledNameSignature);
            yield return new KeyValuePair<string, object>("InlinerNamespace", _inlinerNamespace);
            yield return new KeyValuePair<string, object>("InlinerName", _inlinerName);
            yield return new KeyValuePair<string, object>("InlinerNameSignature", _inlinerNameSignature);
            yield return new KeyValuePair<string, object>("InlineeNamespace", _inlineeNamespace);
            yield return new KeyValuePair<string, object>("InlineeName", _inlineeName);
            yield return new KeyValuePair<string, object>("InlineeNameSignature", _inlineeNameSignature);
            yield return new KeyValuePair<string, object>("FailAlways", _failAlways);
            yield return new KeyValuePair<string, object>("FailReason", _failReason);
            yield return new KeyValuePair<string, object>("ClrInstanceID", _clrInstanceId);
        }
    }

    private sealed class GcMarkWithTypePayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("HeapNum", NetTraceTypeCode.UInt32),
            new("ClrInstanceID", NetTraceTypeCode.UInt16),
            new("Type", NetTraceTypeCode.UInt32),
            new("Bytes", NetTraceTypeCode.UInt64),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new GcMarkWithTypePayload(
                reader.ReadUInt32(),
                reader.ReadUInt16(),
                reader.ReadUInt32(),
                reader.ReadUInt64());
        }

        private readonly uint _heapNum;
        private readonly ushort _clrInstanceId;
        private readonly uint _type;
        private readonly ulong _bytes;

        private GcMarkWithTypePayload(uint heapNum, ushort clrInstanceId, uint type, ulong bytes)
        {
            _heapNum = heapNum;
            _clrInstanceId = clrInstanceId;
            _type = type;
            _bytes = bytes;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "HeapNum":
                    value = _heapNum;
                    return true;
                case "ClrInstanceID":
                    value = _clrInstanceId;
                    return true;
                case "Type":
                    value = _type;
                    return true;
                case "Bytes":
                    value = _bytes;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("HeapNum", _heapNum);
            yield return new KeyValuePair<string, object>("ClrInstanceID", _clrInstanceId);
            yield return new KeyValuePair<string, object>("Type", _type);
            yield return new KeyValuePair<string, object>("Bytes", _bytes);
        }
    }

    private sealed class GcJoinV2Payload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("Heap", NetTraceTypeCode.UInt32),
            new("JoinTime", NetTraceTypeCode.UInt32),
            new("JoinType", NetTraceTypeCode.UInt32),
            new("ClrInstanceID", NetTraceTypeCode.UInt16),
            new("JoinID", NetTraceTypeCode.UInt32),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new GcJoinV2Payload(
                reader.ReadUInt32(),
                reader.ReadUInt32(),
                reader.ReadUInt32(),
                reader.ReadUInt16(),
                reader.ReadUInt32());
        }

        private readonly uint _heap;
        private readonly uint _joinTime;
        private readonly uint _joinType;
        private readonly ushort _clrInstanceId;
        private readonly uint _joinId;

        private GcJoinV2Payload(uint heap, uint joinTime, uint joinType, ushort clrInstanceId, uint joinId)
        {
            _heap = heap;
            _joinTime = joinTime;
            _joinType = joinType;
            _clrInstanceId = clrInstanceId;
            _joinId = joinId;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "Heap":
                    value = _heap;
                    return true;
                case "JoinTime":
                    value = _joinTime;
                    return true;
                case "JoinType":
                    value = _joinType;
                    return true;
                case "ClrInstanceID":
                    value = _clrInstanceId;
                    return true;
                case "JoinID":
                    value = _joinId;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("Heap", _heap);
            yield return new KeyValuePair<string, object>("JoinTime", _joinTime);
            yield return new KeyValuePair<string, object>("JoinType", _joinType);
            yield return new KeyValuePair<string, object>("ClrInstanceID", _clrInstanceId);
            yield return new KeyValuePair<string, object>("JoinID", _joinId);
        }
    }

    private sealed class ExceptionHandlingPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("EntryEIP", NetTraceTypeCode.UInt64),
            new("MethodID", NetTraceTypeCode.UInt64),
            new("MethodName", NetTraceTypeCode.NullTerminatedUtf16String),
            new("ClrInstanceID", NetTraceTypeCode.UInt16),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new ExceptionHandlingPayload(
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadUInt16());
        }

        private readonly ulong _entryEip;
        private readonly ulong _methodId;
        private readonly string _methodName;
        private readonly ushort _clrInstanceId;

        private ExceptionHandlingPayload(ulong entryEip, ulong methodId, string methodName, ushort clrInstanceId)
        {
            _entryEip = entryEip;
            _methodId = methodId;
            _methodName = methodName;
            _clrInstanceId = clrInstanceId;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "EntryEIP":
                    value = _entryEip;
                    return true;
                case "MethodID":
                    value = _methodId;
                    return true;
                case "MethodName":
                    value = _methodName;
                    return true;
                case "ClrInstanceID":
                    value = _clrInstanceId;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("EntryEIP", _entryEip);
            yield return new KeyValuePair<string, object>("MethodID", _methodId);
            yield return new KeyValuePair<string, object>("MethodName", _methodName);
            yield return new KeyValuePair<string, object>("ClrInstanceID", _clrInstanceId);
        }
    }

    private sealed class WaitHandleWaitStartPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("WaitSource", NetTraceTypeCode.Byte),
            new("AssociatedObjectID", NetTraceTypeCode.UInt64),
            new("ClrInstanceID", NetTraceTypeCode.UInt16),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new WaitHandleWaitStartPayload(
                reader.ReadByte(),
                reader.ReadUInt64(),
                reader.ReadUInt16());
        }

        private readonly byte _waitSource;
        private readonly ulong _associatedObjectId;
        private readonly ushort _clrInstanceId;

        private WaitHandleWaitStartPayload(byte waitSource, ulong associatedObjectId, ushort clrInstanceId)
        {
            _waitSource = waitSource;
            _associatedObjectId = associatedObjectId;
            _clrInstanceId = clrInstanceId;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "WaitSource":
                    value = _waitSource;
                    return true;
                case "AssociatedObjectID":
                    value = _associatedObjectId;
                    return true;
                case "ClrInstanceID":
                    value = _clrInstanceId;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("WaitSource", _waitSource);
            yield return new KeyValuePair<string, object>("AssociatedObjectID", _associatedObjectId);
            yield return new KeyValuePair<string, object>("ClrInstanceID", _clrInstanceId);
        }
    }

    private sealed class WaitHandleWaitStopPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("ClrInstanceID", NetTraceTypeCode.UInt16),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new WaitHandleWaitStopPayload(
                reader.ReadUInt16());
        }

        private readonly ushort _clrInstanceId;

        private WaitHandleWaitStopPayload(ushort clrInstanceId)
        {
            _clrInstanceId = clrInstanceId;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "ClrInstanceID":
                    value = _clrInstanceId;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("ClrInstanceID", _clrInstanceId);
        }
    }

    private sealed class MethodLoadUnloadRundownVerboseV1Payload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("MethodID", NetTraceTypeCode.UInt64),
            new("ModuleID", NetTraceTypeCode.UInt64),
            new("MethodStartAddress", NetTraceTypeCode.UInt64),
            new("MethodSize", NetTraceTypeCode.UInt32),
            new("MethodToken", NetTraceTypeCode.UInt32),
            new("MethodFlags", NetTraceTypeCode.UInt32),
            new("MethodNamespace", NetTraceTypeCode.NullTerminatedUtf16String),
            new("MethodName", NetTraceTypeCode.NullTerminatedUtf16String),
            new("MethodSignature", NetTraceTypeCode.NullTerminatedUtf16String),
            new("ClrInstanceID", NetTraceTypeCode.UInt16),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new MethodLoadUnloadRundownVerboseV1Payload(
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadUInt32(),
                reader.ReadUInt32(),
                reader.ReadUInt32(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadUInt16());
        }

        private readonly ulong _methodId;
        private readonly ulong _moduleId;
        private readonly ulong _methodStartAddress;
        private readonly uint _methodSize;
        private readonly uint _methodToken;
        private readonly uint _methodFlags;
        private readonly string _methodNamespace;
        private readonly string _methodName;
        private readonly string _methodSignature;
        private readonly ushort _clrInstanceId;

        private MethodLoadUnloadRundownVerboseV1Payload(ulong methodId, ulong moduleId, ulong methodStartAddress,
            uint methodSize, uint methodToken, uint methodFlags, string methodNamespace, string methodName,
            string methodSignature, ushort clrInstanceId)
        {
            _methodId = methodId;
            _moduleId = moduleId;
            _methodStartAddress = methodStartAddress;
            _methodSize = methodSize;
            _methodToken = methodToken;
            _methodFlags = methodFlags;
            _methodNamespace = methodNamespace;
            _methodName = methodName;
            _methodSignature = methodSignature;
            _clrInstanceId = clrInstanceId;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "MethodID":
                    value = _methodId;
                    return true;
                case "ModuleID":
                    value = _moduleId;
                    return true;
                case "MethodStartAddress":
                    value = _methodStartAddress;
                    return true;
                case "MethodSize":
                    value = _methodSize;
                    return true;
                case "MethodToken":
                    value = _methodToken;
                    return true;
                case "MethodFlags":
                    value = _methodFlags;
                    return true;
                case "MethodNamespace":
                    value = _methodNamespace;
                    return true;
                case "MethodName":
                    value = _methodName;
                    return true;
                case "MethodSignature":
                    value = _methodSignature;
                    return true;
                case "ClrInstanceID":
                    value = _clrInstanceId;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("MethodID", _methodId);
            yield return new KeyValuePair<string, object>("ModuleID", _moduleId);
            yield return new KeyValuePair<string, object>("MethodStartAddress", _methodStartAddress);
            yield return new KeyValuePair<string, object>("MethodSize", _methodSize);
            yield return new KeyValuePair<string, object>("MethodToken", _methodToken);
            yield return new KeyValuePair<string, object>("MethodFlags", _methodFlags);
            yield return new KeyValuePair<string, object>("MethodNamespace", _methodNamespace);
            yield return new KeyValuePair<string, object>("MethodName", _methodName);
            yield return new KeyValuePair<string, object>("MethodSignature", _methodSignature);
            yield return new KeyValuePair<string, object>("ClrInstanceID", _clrInstanceId);
        }
    }

    private sealed class MethodLoadUnloadRundownVerboseV2Payload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("MethodID", NetTraceTypeCode.UInt64),
            new("ModuleID", NetTraceTypeCode.UInt64),
            new("MethodStartAddress", NetTraceTypeCode.UInt64),
            new("MethodSize", NetTraceTypeCode.UInt32),
            new("MethodToken", NetTraceTypeCode.UInt32),
            new("MethodFlags", NetTraceTypeCode.UInt32),
            new("MethodNamespace", NetTraceTypeCode.NullTerminatedUtf16String),
            new("MethodName", NetTraceTypeCode.NullTerminatedUtf16String),
            new("MethodSignature", NetTraceTypeCode.NullTerminatedUtf16String),
            new("ClrInstanceID", NetTraceTypeCode.UInt16),
            new("ReJITID", NetTraceTypeCode.UInt64),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new MethodLoadUnloadRundownVerboseV2Payload(
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadUInt32(),
                reader.ReadUInt32(),
                reader.ReadUInt32(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadUInt16(),
                reader.ReadUInt64());
        }

        private readonly ulong _methodId;
        private readonly ulong _moduleId;
        private readonly ulong _methodStartAddress;
        private readonly uint _methodSize;
        private readonly uint _methodToken;
        private readonly uint _methodFlags;
        private readonly string _methodNamespace;
        private readonly string _methodName;
        private readonly string _methodSignature;
        private readonly ushort _clrInstanceId;
        private readonly ulong _reJitId;

        private MethodLoadUnloadRundownVerboseV2Payload(ulong methodId, ulong moduleId, ulong methodStartAddress,
            uint methodSize, uint methodToken, uint methodFlags, string methodNamespace, string methodName,
            string methodSignature, ushort clrInstanceId, ulong reJitId)
        {
            _methodId = methodId;
            _moduleId = moduleId;
            _methodStartAddress = methodStartAddress;
            _methodSize = methodSize;
            _methodToken = methodToken;
            _methodFlags = methodFlags;
            _methodNamespace = methodNamespace;
            _methodName = methodName;
            _methodSignature = methodSignature;
            _clrInstanceId = clrInstanceId;
            _reJitId = reJitId;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "MethodID":
                    value = _methodId;
                    return true;
                case "ModuleID":
                    value = _moduleId;
                    return true;
                case "MethodStartAddress":
                    value = _methodStartAddress;
                    return true;
                case "MethodSize":
                    value = _methodSize;
                    return true;
                case "MethodToken":
                    value = _methodToken;
                    return true;
                case "MethodFlags":
                    value = _methodFlags;
                    return true;
                case "MethodNamespace":
                    value = _methodNamespace;
                    return true;
                case "MethodName":
                    value = _methodName;
                    return true;
                case "MethodSignature":
                    value = _methodSignature;
                    return true;
                case "ClrInstanceID":
                    value = _clrInstanceId;
                    return true;
                case "ReJITID":
                    value = _reJitId;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("MethodID", _methodId);
            yield return new KeyValuePair<string, object>("ModuleID", _moduleId);
            yield return new KeyValuePair<string, object>("MethodStartAddress", _methodStartAddress);
            yield return new KeyValuePair<string, object>("MethodSize", _methodSize);
            yield return new KeyValuePair<string, object>("MethodToken", _methodToken);
            yield return new KeyValuePair<string, object>("MethodFlags", _methodFlags);
            yield return new KeyValuePair<string, object>("MethodNamespace", _methodNamespace);
            yield return new KeyValuePair<string, object>("MethodName", _methodName);
            yield return new KeyValuePair<string, object>("MethodSignature", _methodSignature);
            yield return new KeyValuePair<string, object>("ClrInstanceID", _clrInstanceId);
            yield return new KeyValuePair<string, object>("ReJITID", _reJitId);
        }
    }

    private sealed class DcStartEndPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("ClrInstanceID", NetTraceTypeCode.UInt16),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new DcStartEndPayload(
                reader.ReadUInt16());
        }

        private readonly ushort _clrInstanceId;

        private DcStartEndPayload(ushort clrInstanceId)
        {
            _clrInstanceId = clrInstanceId;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "ClrInstanceID":
                    value = _clrInstanceId;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("ClrInstanceID", _clrInstanceId);
        }
    }

    private sealed class DomainModuleLoadUnloadRundownV1Payload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("ModuleID", NetTraceTypeCode.UInt64),
            new("AssemblyID", NetTraceTypeCode.UInt64),
            new("AppDomainID", NetTraceTypeCode.UInt64),
            new("ModuleFlags", NetTraceTypeCode.UInt32),
            new("Reserved1", NetTraceTypeCode.UInt32),
            new("ModuleILPath", NetTraceTypeCode.NullTerminatedUtf16String),
            new("ModuleNativePath", NetTraceTypeCode.NullTerminatedUtf16String),
            new("ClrInstanceID", NetTraceTypeCode.UInt16),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new DomainModuleLoadUnloadRundownV1Payload(
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadUInt32(),
                reader.ReadUInt32(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadUInt16());
        }

        private readonly ulong _moduleId;
        private readonly ulong _assemblyId;
        private readonly ulong _appDomainId;
        private readonly uint _moduleFlags;
        private readonly uint _reserved1;
        private readonly string _moduleIlPath;
        private readonly string _moduleNativePath;
        private readonly ushort _clrInstanceId;

        private DomainModuleLoadUnloadRundownV1Payload(ulong moduleId, ulong assemblyId, ulong appDomainId,
            uint moduleFlags, uint reserved1, string moduleIlPath, string moduleNativePath, ushort clrInstanceId)
        {
            _moduleId = moduleId;
            _assemblyId = assemblyId;
            _appDomainId = appDomainId;
            _moduleFlags = moduleFlags;
            _reserved1 = reserved1;
            _moduleIlPath = moduleIlPath;
            _moduleNativePath = moduleNativePath;
            _clrInstanceId = clrInstanceId;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "ModuleID":
                    value = _moduleId;
                    return true;
                case "AssemblyID":
                    value = _assemblyId;
                    return true;
                case "AppDomainID":
                    value = _appDomainId;
                    return true;
                case "ModuleFlags":
                    value = _moduleFlags;
                    return true;
                case "Reserved1":
                    value = _reserved1;
                    return true;
                case "ModuleILPath":
                    value = _moduleIlPath;
                    return true;
                case "ModuleNativePath":
                    value = _moduleNativePath;
                    return true;
                case "ClrInstanceID":
                    value = _clrInstanceId;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("ModuleID", _moduleId);
            yield return new KeyValuePair<string, object>("AssemblyID", _assemblyId);
            yield return new KeyValuePair<string, object>("AppDomainID", _appDomainId);
            yield return new KeyValuePair<string, object>("ModuleFlags", _moduleFlags);
            yield return new KeyValuePair<string, object>("Reserved1", _reserved1);
            yield return new KeyValuePair<string, object>("ModuleILPath", _moduleIlPath);
            yield return new KeyValuePair<string, object>("ModuleNativePath", _moduleNativePath);
            yield return new KeyValuePair<string, object>("ClrInstanceID", _clrInstanceId);
        }
    }

    private sealed class ModuleLoadUnloadRundownV2Payload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("ModuleID", NetTraceTypeCode.UInt64),
            new("AssemblyID", NetTraceTypeCode.UInt64),
            new("ModuleFlags", NetTraceTypeCode.UInt32),
            new("Reserved1", NetTraceTypeCode.UInt32),
            new("ModuleILPath", NetTraceTypeCode.NullTerminatedUtf16String),
            new("ModuleNativePath", NetTraceTypeCode.NullTerminatedUtf16String),
            new("ClrInstanceID", NetTraceTypeCode.UInt16),
            new("ManagedPdbSignature", NetTraceTypeCode.Guid),
            new("ManagedPdbAge", NetTraceTypeCode.UInt32),
            new("ManagedPdbBuildPath", NetTraceTypeCode.NullTerminatedUtf16String),
            new("NativePdbSignature", NetTraceTypeCode.Guid),
            new("NativePdbAge", NetTraceTypeCode.UInt32),
            new("NativePdbBuildPath", NetTraceTypeCode.NullTerminatedUtf16String),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new ModuleLoadUnloadRundownV2Payload(
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadUInt32(),
                reader.ReadUInt32(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadUInt16(),
                reader.ReadGuid(),
                reader.ReadUInt32(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadGuid(),
                reader.ReadUInt32(),
                reader.ReadNullTerminatedUtf16String());
        }

        private readonly ulong _moduleId;
        private readonly ulong _assemblyId;
        private readonly uint _moduleFlags;
        private readonly uint _reserved1;
        private readonly string _moduleIlPath;
        private readonly string _moduleNativePath;
        private readonly ushort _clrInstanceId;
        private readonly Guid _managedPdbSignature;
        private readonly uint _managedPdbAge;
        private readonly string _managedPdbBuildPath;
        private readonly Guid _nativePdbSignature;
        private readonly uint _nativePdbAge;
        private readonly string _nativePdbBuildPath;

        private ModuleLoadUnloadRundownV2Payload(ulong moduleId, ulong assemblyId, uint moduleFlags, uint reserved1,
            string moduleIlPath, string moduleNativePath, ushort clrInstanceId, Guid managedPdbSignature,
            uint managedPdbAge, string managedPdbBuildPath, Guid nativePdbSignature, uint nativePdbAge,
            string nativePdbBuildPath)
        {
            _moduleId = moduleId;
            _assemblyId = assemblyId;
            _moduleFlags = moduleFlags;
            _reserved1 = reserved1;
            _moduleIlPath = moduleIlPath;
            _moduleNativePath = moduleNativePath;
            _clrInstanceId = clrInstanceId;
            _managedPdbSignature = managedPdbSignature;
            _managedPdbAge = managedPdbAge;
            _managedPdbBuildPath = managedPdbBuildPath;
            _nativePdbSignature = nativePdbSignature;
            _nativePdbAge = nativePdbAge;
            _nativePdbBuildPath = nativePdbBuildPath;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "ModuleID":
                    value = _moduleId;
                    return true;
                case "AssemblyID":
                    value = _assemblyId;
                    return true;
                case "ModuleFlags":
                    value = _moduleFlags;
                    return true;
                case "Reserved1":
                    value = _reserved1;
                    return true;
                case "ModuleILPath":
                    value = _moduleIlPath;
                    return true;
                case "ModuleNativePath":
                    value = _moduleNativePath;
                    return true;
                case "ClrInstanceID":
                    value = _clrInstanceId;
                    return true;
                case "ManagedPdbSignature":
                    value = _managedPdbSignature;
                    return true;
                case "ManagedPdbAge":
                    value = _managedPdbAge;
                    return true;
                case "ManagedPdbBuildPath":
                    value = _managedPdbBuildPath;
                    return true;
                case "NativePdbSignature":
                    value = _nativePdbSignature;
                    return true;
                case "NativePdbAge":
                    value = _nativePdbAge;
                    return true;
                case "NativePdbBuildPath":
                    value = _nativePdbBuildPath;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("ModuleID", _moduleId);
            yield return new KeyValuePair<string, object>("AssemblyID", _assemblyId);
            yield return new KeyValuePair<string, object>("ModuleFlags", _moduleFlags);
            yield return new KeyValuePair<string, object>("Reserved1", _reserved1);
            yield return new KeyValuePair<string, object>("ModuleILPath", _moduleIlPath);
            yield return new KeyValuePair<string, object>("ModuleNativePath", _moduleNativePath);
            yield return new KeyValuePair<string, object>("ClrInstanceID", _clrInstanceId);
            yield return new KeyValuePair<string, object>("ManagedPdbSignature", _managedPdbSignature);
            yield return new KeyValuePair<string, object>("ManagedPdbAge", _managedPdbAge);
            yield return new KeyValuePair<string, object>("ManagedPdbBuildPath", _managedPdbBuildPath);
            yield return new KeyValuePair<string, object>("NativePdbSignature", _nativePdbSignature);
            yield return new KeyValuePair<string, object>("NativePdbAge", _nativePdbAge);
            yield return new KeyValuePair<string, object>("NativePdbBuildPath", _nativePdbBuildPath);
        }
    }

    private sealed class AssemblyLoadUnloadRundownV1Payload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("AssemblyID", NetTraceTypeCode.UInt64),
            new("AppDomainID", NetTraceTypeCode.UInt64),
            new("BindingID", NetTraceTypeCode.UInt64),
            new("AssemblyFlags", NetTraceTypeCode.UInt32),
            new("FullyQualifiedAssemblyName", NetTraceTypeCode.NullTerminatedUtf16String),
            new("ClrInstanceID", NetTraceTypeCode.UInt16),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new AssemblyLoadUnloadRundownV1Payload(
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadUInt32(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadUInt16());
        }

        private readonly ulong _assemblyId;
        private readonly ulong _appDomainId;
        private readonly ulong _bindingId;
        private readonly uint _assemblyFlags;
        private readonly string _fullyQualifiedAssemblyName;
        private readonly ushort _clrInstanceId;

        private AssemblyLoadUnloadRundownV1Payload(ulong assemblyId, ulong appDomainId, ulong bindingId,
            uint assemblyFlags, string fullyQualifiedAssemblyName, ushort clrInstanceId)
        {
            _assemblyId = assemblyId;
            _appDomainId = appDomainId;
            _bindingId = bindingId;
            _assemblyFlags = assemblyFlags;
            _fullyQualifiedAssemblyName = fullyQualifiedAssemblyName;
            _clrInstanceId = clrInstanceId;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "AssemblyID":
                    value = _assemblyId;
                    return true;
                case "AppDomainID":
                    value = _appDomainId;
                    return true;
                case "BindingID":
                    value = _bindingId;
                    return true;
                case "AssemblyFlags":
                    value = _assemblyFlags;
                    return true;
                case "FullyQualifiedAssemblyName":
                    value = _fullyQualifiedAssemblyName;
                    return true;
                case "ClrInstanceID":
                    value = _clrInstanceId;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("AssemblyID", _assemblyId);
            yield return new KeyValuePair<string, object>("AppDomainID", _appDomainId);
            yield return new KeyValuePair<string, object>("BindingID", _bindingId);
            yield return new KeyValuePair<string, object>("AssemblyFlags", _assemblyFlags);
            yield return new KeyValuePair<string, object>("FullyQualifiedAssemblyName", _fullyQualifiedAssemblyName);
            yield return new KeyValuePair<string, object>("ClrInstanceID", _clrInstanceId);
        }
    }

    private sealed class AppDomainLoadUnloadRundownV1Payload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("AppDomainID", NetTraceTypeCode.UInt64),
            new("AppDomainFlags", NetTraceTypeCode.UInt32),
            new("AppDomainName", NetTraceTypeCode.NullTerminatedUtf16String),
            new("AppDomainIndex", NetTraceTypeCode.UInt32),
            new("ClrInstanceID", NetTraceTypeCode.UInt16),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new AppDomainLoadUnloadRundownV1Payload(
                reader.ReadUInt64(),
                reader.ReadUInt32(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadUInt32(),
                reader.ReadUInt16());
        }

        private readonly ulong _appDomainId;
        private readonly uint _appDomainFlags;
        private readonly string _appDomainName;
        private readonly uint _appDomainIndex;
        private readonly ushort _clrInstanceId;

        private AppDomainLoadUnloadRundownV1Payload(ulong appDomainId, uint appDomainFlags, string appDomainName,
            uint appDomainIndex, ushort clrInstanceId)
        {
            _appDomainId = appDomainId;
            _appDomainFlags = appDomainFlags;
            _appDomainName = appDomainName;
            _appDomainIndex = appDomainIndex;
            _clrInstanceId = clrInstanceId;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "AppDomainID":
                    value = _appDomainId;
                    return true;
                case "AppDomainFlags":
                    value = _appDomainFlags;
                    return true;
                case "AppDomainName":
                    value = _appDomainName;
                    return true;
                case "AppDomainIndex":
                    value = _appDomainIndex;
                    return true;
                case "ClrInstanceID":
                    value = _clrInstanceId;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("AppDomainID", _appDomainId);
            yield return new KeyValuePair<string, object>("AppDomainFlags", _appDomainFlags);
            yield return new KeyValuePair<string, object>("AppDomainName", _appDomainName);
            yield return new KeyValuePair<string, object>("AppDomainIndex", _appDomainIndex);
            yield return new KeyValuePair<string, object>("ClrInstanceID", _clrInstanceId);
        }
    }

    private sealed class RuntimeInformationRundownPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("ClrInstanceID", NetTraceTypeCode.UInt16),
            new("Sku", NetTraceTypeCode.UInt16),
            new("BclMajorVersion", NetTraceTypeCode.UInt16),
            new("BclMinorVersion", NetTraceTypeCode.UInt16),
            new("BclBuildNumber", NetTraceTypeCode.UInt16),
            new("BclQfeNumber", NetTraceTypeCode.UInt16),
            new("VMMajorVersion", NetTraceTypeCode.UInt16),
            new("VMMinorVersion", NetTraceTypeCode.UInt16),
            new("VMBuildNumber", NetTraceTypeCode.UInt16),
            new("VMQfeNumber", NetTraceTypeCode.UInt16),
            new("StartupFlags", NetTraceTypeCode.UInt32),
            new("StartupMode", NetTraceTypeCode.Byte),
            new("CommandLine", NetTraceTypeCode.NullTerminatedUtf16String),
            new("ComObjectGuid", NetTraceTypeCode.Guid),
            new("RuntimeDllPath", NetTraceTypeCode.NullTerminatedUtf16String),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new RuntimeInformationRundownPayload(
                reader.ReadUInt16(),
                reader.ReadUInt16(),
                reader.ReadUInt16(),
                reader.ReadUInt16(),
                reader.ReadUInt16(),
                reader.ReadUInt16(),
                reader.ReadUInt16(),
                reader.ReadUInt16(),
                reader.ReadUInt16(),
                reader.ReadUInt16(),
                reader.ReadUInt32(),
                reader.ReadByte(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadGuid(),
                reader.ReadNullTerminatedUtf16String());
        }

        private readonly ushort _clrInstanceId;
        private readonly ushort _sku;
        private readonly ushort _bclMajorVersion;
        private readonly ushort _bclMinorVersion;
        private readonly ushort _bclBuildNumber;
        private readonly ushort _bclQfeNumber;
        private readonly ushort _vMMajorVersion;
        private readonly ushort _vMMinorVersion;
        private readonly ushort _vMBuildNumber;
        private readonly ushort _vMQfeNumber;
        private readonly uint _startupFlags;
        private readonly byte _startupMode;
        private readonly string _commandLine;
        private readonly Guid _comObjectGuid;
        private readonly string _runtimeDllPath;

        private RuntimeInformationRundownPayload(ushort clrInstanceId, ushort sku, ushort bclMajorVersion,
            ushort bclMinorVersion, ushort bclBuildNumber, ushort bclQfeNumber, ushort vMMajorVersion,
            ushort vMMinorVersion, ushort vMBuildNumber, ushort vMQfeNumber, uint startupFlags, byte startupMode,
            string commandLine, Guid comObjectGuid, string runtimeDllPath)
        {
            _clrInstanceId = clrInstanceId;
            _sku = sku;
            _bclMajorVersion = bclMajorVersion;
            _bclMinorVersion = bclMinorVersion;
            _bclBuildNumber = bclBuildNumber;
            _bclQfeNumber = bclQfeNumber;
            _vMMajorVersion = vMMajorVersion;
            _vMMinorVersion = vMMinorVersion;
            _vMBuildNumber = vMBuildNumber;
            _vMQfeNumber = vMQfeNumber;
            _startupFlags = startupFlags;
            _startupMode = startupMode;
            _commandLine = commandLine;
            _comObjectGuid = comObjectGuid;
            _runtimeDllPath = runtimeDllPath;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "ClrInstanceID":
                    value = _clrInstanceId;
                    return true;
                case "Sku":
                    value = _sku;
                    return true;
                case "BclMajorVersion":
                    value = _bclMajorVersion;
                    return true;
                case "BclMinorVersion":
                    value = _bclMinorVersion;
                    return true;
                case "BclBuildNumber":
                    value = _bclBuildNumber;
                    return true;
                case "BclQfeNumber":
                    value = _bclQfeNumber;
                    return true;
                case "VMMajorVersion":
                    value = _vMMajorVersion;
                    return true;
                case "VMMinorVersion":
                    value = _vMMinorVersion;
                    return true;
                case "VMBuildNumber":
                    value = _vMBuildNumber;
                    return true;
                case "VMQfeNumber":
                    value = _vMQfeNumber;
                    return true;
                case "StartupFlags":
                    value = _startupFlags;
                    return true;
                case "StartupMode":
                    value = _startupMode;
                    return true;
                case "CommandLine":
                    value = _commandLine;
                    return true;
                case "ComObjectGuid":
                    value = _comObjectGuid;
                    return true;
                case "RuntimeDllPath":
                    value = _runtimeDllPath;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("ClrInstanceID", _clrInstanceId);
            yield return new KeyValuePair<string, object>("Sku", _sku);
            yield return new KeyValuePair<string, object>("BclMajorVersion", _bclMajorVersion);
            yield return new KeyValuePair<string, object>("BclMinorVersion", _bclMinorVersion);
            yield return new KeyValuePair<string, object>("BclBuildNumber", _bclBuildNumber);
            yield return new KeyValuePair<string, object>("BclQfeNumber", _bclQfeNumber);
            yield return new KeyValuePair<string, object>("VMMajorVersion", _vMMajorVersion);
            yield return new KeyValuePair<string, object>("VMMinorVersion", _vMMinorVersion);
            yield return new KeyValuePair<string, object>("VMBuildNumber", _vMBuildNumber);
            yield return new KeyValuePair<string, object>("VMQfeNumber", _vMQfeNumber);
            yield return new KeyValuePair<string, object>("StartupFlags", _startupFlags);
            yield return new KeyValuePair<string, object>("StartupMode", _startupMode);
            yield return new KeyValuePair<string, object>("CommandLine", _commandLine);
            yield return new KeyValuePair<string, object>("ComObjectGuid", _comObjectGuid);
            yield return new KeyValuePair<string, object>("RuntimeDllPath", _runtimeDllPath);
        }
    }

    private sealed class ThreadSamplePayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("Type", NetTraceTypeCode.Int32),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new ThreadSamplePayload(
                reader.ReadInt32());
        }

        private readonly int _type;

        private ThreadSamplePayload(int type)
        {
            _type = type;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "Type":
                    value = _type;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("Type", _type);
        }
    }

    private sealed class TaskScheduledPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("OriginatingTaskSchedulerID", NetTraceTypeCode.Int32),
            new("OriginatingTaskID", NetTraceTypeCode.Int32),
            new("TaskID", NetTraceTypeCode.Int32),
            new("CreatingTaskID", NetTraceTypeCode.Int32),
            new("TaskCreationOptions", NetTraceTypeCode.Int32),
            new("appDomain", NetTraceTypeCode.Int32),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new TaskScheduledPayload(
                reader.ReadInt32(),
                reader.ReadInt32(),
                reader.ReadInt32(),
                reader.ReadInt32(),
                reader.ReadInt32(),
                reader.ReadInt32());
        }

        private readonly int _originatingTaskSchedulerId;
        private readonly int _originatingTaskId;
        private readonly int _taskId;
        private readonly int _creatingTaskId;
        private readonly int _taskCreationOptions;
        private readonly int _appDomain;

        private TaskScheduledPayload(int originatingTaskSchedulerId, int originatingTaskId, int taskId,
            int creatingTaskId, int taskCreationOptions, int appDomain)
        {
            _originatingTaskSchedulerId = originatingTaskSchedulerId;
            _originatingTaskId = originatingTaskId;
            _taskId = taskId;
            _creatingTaskId = creatingTaskId;
            _taskCreationOptions = taskCreationOptions;
            _appDomain = appDomain;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "OriginatingTaskSchedulerID":
                    value = _originatingTaskSchedulerId;
                    return true;
                case "OriginatingTaskID":
                    value = _originatingTaskId;
                    return true;
                case "TaskID":
                    value = _taskId;
                    return true;
                case "CreatingTaskID":
                    value = _creatingTaskId;
                    return true;
                case "TaskCreationOptions":
                    value = _taskCreationOptions;
                    return true;
                case "appDomain":
                    value = _appDomain;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("OriginatingTaskSchedulerID", _originatingTaskSchedulerId);
            yield return new KeyValuePair<string, object>("OriginatingTaskID", _originatingTaskId);
            yield return new KeyValuePair<string, object>("TaskID", _taskId);
            yield return new KeyValuePair<string, object>("CreatingTaskID", _creatingTaskId);
            yield return new KeyValuePair<string, object>("TaskCreationOptions", _taskCreationOptions);
            yield return new KeyValuePair<string, object>("appDomain", _appDomain);
        }
    }

    private sealed class TaskStartedPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("OriginatingTaskSchedulerID", NetTraceTypeCode.Int32),
            new("OriginatingTaskID", NetTraceTypeCode.Int32),
            new("TaskID", NetTraceTypeCode.Int32),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new TaskStartedPayload(
                reader.ReadInt32(),
                reader.ReadInt32(),
                reader.ReadInt32());
        }

        private readonly int _originatingTaskSchedulerId;
        private readonly int _originatingTaskId;
        private readonly int _taskId;

        private TaskStartedPayload(int originatingTaskSchedulerId, int originatingTaskId, int taskId)
        {
            _originatingTaskSchedulerId = originatingTaskSchedulerId;
            _originatingTaskId = originatingTaskId;
            _taskId = taskId;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "OriginatingTaskSchedulerID":
                    value = _originatingTaskSchedulerId;
                    return true;
                case "OriginatingTaskID":
                    value = _originatingTaskId;
                    return true;
                case "TaskID":
                    value = _taskId;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("OriginatingTaskSchedulerID", _originatingTaskSchedulerId);
            yield return new KeyValuePair<string, object>("OriginatingTaskID", _originatingTaskId);
            yield return new KeyValuePair<string, object>("TaskID", _taskId);
        }
    }

    private sealed class TaskCompletedPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("OriginatingTaskSchedulerID", NetTraceTypeCode.Int32),
            new("OriginatingTaskID", NetTraceTypeCode.Int32),
            new("TaskID", NetTraceTypeCode.Int32),
            new("IsExceptional", NetTraceTypeCode.Boolean32),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new TaskCompletedPayload(
                reader.ReadInt32(),
                reader.ReadInt32(),
                reader.ReadInt32(),
                reader.ReadInt32() != 0);
        }

        private readonly int _originatingTaskSchedulerId;
        private readonly int _originatingTaskId;
        private readonly int _taskId;
        private readonly bool _isExceptional;

        private TaskCompletedPayload(int originatingTaskSchedulerId, int originatingTaskId, int taskId, bool isExceptional)
        {
            _originatingTaskSchedulerId = originatingTaskSchedulerId;
            _originatingTaskId = originatingTaskId;
            _taskId = taskId;
            _isExceptional = isExceptional;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "OriginatingTaskSchedulerID":
                    value = _originatingTaskSchedulerId;
                    return true;
                case "OriginatingTaskID":
                    value = _originatingTaskId;
                    return true;
                case "TaskID":
                    value = _taskId;
                    return true;
                case "IsExceptional":
                    value = _isExceptional;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("OriginatingTaskSchedulerID", _originatingTaskSchedulerId);
            yield return new KeyValuePair<string, object>("OriginatingTaskID", _originatingTaskId);
            yield return new KeyValuePair<string, object>("TaskID", _taskId);
            yield return new KeyValuePair<string, object>("IsExceptional", _isExceptional);
        }
    }

    private sealed class TaskWaitBeginPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("OriginatingTaskSchedulerID", NetTraceTypeCode.Int32),
            new("OriginatingTaskID", NetTraceTypeCode.Int32),
            new("TaskID", NetTraceTypeCode.Int32),
            new("Behavior", NetTraceTypeCode.Int32),
            new("ContinueWithTaskID", NetTraceTypeCode.Int32),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new TaskWaitBeginPayload(
                reader.ReadInt32(),
                reader.ReadInt32(),
                reader.ReadInt32(),
                reader.ReadInt32(),
                reader.ReadInt32());
        }

        private readonly int _originatingTaskSchedulerId;
        private readonly int _originatingTaskId;
        private readonly int _taskId;
        private readonly int _behavior;
        private readonly int _continueWithTaskId;

        private TaskWaitBeginPayload(int originatingTaskSchedulerId, int originatingTaskId, int taskId,
            int behavior, int continueWithTaskId)
        {
            _originatingTaskSchedulerId = originatingTaskSchedulerId;
            _originatingTaskId = originatingTaskId;
            _taskId = taskId;
            _behavior = behavior;
            _continueWithTaskId = continueWithTaskId;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "OriginatingTaskSchedulerID":
                    value = _originatingTaskSchedulerId;
                    return true;
                case "OriginatingTaskID":
                    value = _originatingTaskId;
                    return true;
                case "TaskID":
                    value = _taskId;
                    return true;
                case "Behavior":
                    value = _behavior;
                    return true;
                case "ContinueWithTaskID":
                    value = _continueWithTaskId;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("OriginatingTaskSchedulerID", _originatingTaskSchedulerId);
            yield return new KeyValuePair<string, object>("OriginatingTaskID", _originatingTaskId);
            yield return new KeyValuePair<string, object>("TaskID", _taskId);
            yield return new KeyValuePair<string, object>("Behavior", _behavior);
            yield return new KeyValuePair<string, object>("ContinueWithTaskID", _continueWithTaskId);
        }
    }

    private sealed class TaskWaitEndPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("OriginatingTaskSchedulerID", NetTraceTypeCode.Int32),
            new("OriginatingTaskID", NetTraceTypeCode.Int32),
            new("TaskID", NetTraceTypeCode.Int32),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new TaskWaitEndPayload(
                reader.ReadInt32(),
                reader.ReadInt32(),
                reader.ReadInt32());
        }

        private readonly int _originatingTaskSchedulerId;
        private readonly int _originatingTaskId;
        private readonly int _taskId;

        private TaskWaitEndPayload(int originatingTaskSchedulerId, int originatingTaskId, int taskId)
        {
            _originatingTaskSchedulerId = originatingTaskSchedulerId;
            _originatingTaskId = originatingTaskId;
            _taskId = taskId;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "OriginatingTaskSchedulerID":
                    value = _originatingTaskSchedulerId;
                    return true;
                case "OriginatingTaskID":
                    value = _originatingTaskId;
                    return true;
                case "TaskID":
                    value = _taskId;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("OriginatingTaskSchedulerID", _originatingTaskSchedulerId);
            yield return new KeyValuePair<string, object>("OriginatingTaskID", _originatingTaskId);
            yield return new KeyValuePair<string, object>("TaskID", _taskId);
        }
    }

    private sealed class AwaitTaskContinuationScheduledPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("OriginatingTaskSchedulerID", NetTraceTypeCode.Int32),
            new("OriginatingTaskID", NetTraceTypeCode.Int32),
            new("ContinueWithTaskID", NetTraceTypeCode.Int32),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new AwaitTaskContinuationScheduledPayload(
                reader.ReadInt32(),
                reader.ReadInt32(),
                reader.ReadInt32());
        }

        private readonly int _originatingTaskSchedulerId;
        private readonly int _originatingTaskId;
        private readonly int _continueWithTaskId;

        private AwaitTaskContinuationScheduledPayload(int originatingTaskSchedulerId, int originatingTaskId,
            int continueWithTaskId)
        {
            _originatingTaskSchedulerId = originatingTaskSchedulerId;
            _originatingTaskId = originatingTaskId;
            _continueWithTaskId = continueWithTaskId;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "OriginatingTaskSchedulerID":
                    value = _originatingTaskSchedulerId;
                    return true;
                case "OriginatingTaskID":
                    value = _originatingTaskId;
                    return true;
                case "ContinueWithTaskID":
                    value = _continueWithTaskId;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("OriginatingTaskSchedulerID", _originatingTaskSchedulerId);
            yield return new KeyValuePair<string, object>("OriginatingTaskID", _originatingTaskId);
            yield return new KeyValuePair<string, object>("ContinueWithTaskID", _continueWithTaskId);
        }
    }

    private sealed class TaskWaitContinuationPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("TaskID", NetTraceTypeCode.Int32),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new TaskWaitContinuationPayload(reader.ReadInt32());
        }

        private readonly int _taskId;

        private TaskWaitContinuationPayload(int taskId)
        {
            _taskId = taskId;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "TaskID":
                    value = _taskId;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("TaskID", _taskId);
        }
    }

    private sealed class TraceOperationStartPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("TaskID", NetTraceTypeCode.Int32),
            new("OperationName", NetTraceTypeCode.NullTerminatedUtf16String),
            new("RelatedContext", NetTraceTypeCode.Int64),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new TraceOperationStartPayload(
                reader.ReadInt32(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadInt64());
        }

        private readonly int _taskId;
        private readonly string _operationName;
        private readonly long _relatedContext;

        private TraceOperationStartPayload(int taskId, string operationName, long relatedContext)
        {
            _taskId = taskId;
            _operationName = operationName;
            _relatedContext = relatedContext;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "TaskID":
                    value = _taskId;
                    return true;
                case "OperationName":
                    value = _operationName;
                    return true;
                case "RelatedContext":
                    value = _relatedContext;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("TaskID", _taskId);
            yield return new KeyValuePair<string, object>("OperationName", _operationName);
            yield return new KeyValuePair<string, object>("RelatedContext", _relatedContext);
        }
    }

    private sealed class TraceOperationStopPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("TaskID", NetTraceTypeCode.Int32),
            new("Status", NetTraceTypeCode.Int32),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new TraceOperationStopPayload(
                reader.ReadInt32(),
                reader.ReadInt32());
        }

        private readonly int _taskId;
        private readonly int _status;

        private TraceOperationStopPayload(int taskId, int status)
        {
            _taskId = taskId;
            _status = status;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "TaskID":
                    value = _taskId;
                    return true;
                case "Status":
                    value = _status;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("TaskID", _taskId);
            yield return new KeyValuePair<string, object>("Status", _status);
        }
    }

    private sealed class TraceOperationRelationPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("TaskID", NetTraceTypeCode.Int32),
            new("Relation", NetTraceTypeCode.Int32),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new TraceOperationRelationPayload(
                reader.ReadInt32(),
                reader.ReadInt32());
        }

        private readonly int _taskId;
        private readonly int _relation;

        private TraceOperationRelationPayload(int taskId, int relation)
        {
            _taskId = taskId;
            _relation = relation;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "TaskID":
                    value = _taskId;
                    return true;
                case "Status":
                    value = _relation;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("TaskID", _taskId);
            yield return new KeyValuePair<string, object>("Relation", _relation);
        }
    }

    private sealed class BufferRentedPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("bufferId", NetTraceTypeCode.Int32),
            new("bufferSize", NetTraceTypeCode.Int32),
            new("poolId", NetTraceTypeCode.Int32),
            new("bucketId", NetTraceTypeCode.Int32),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new BufferRentedPayload(
                reader.ReadInt32(),
                reader.ReadInt32(),
                reader.ReadInt32(),
                reader.ReadInt32());
        }

        private readonly int _bufferId;
        private readonly int _bufferSize;
        private readonly int _poolId;
        private readonly int _bucketId;

        private BufferRentedPayload(int bufferId, int bufferSize, int poolId, int bucketId)
        {
            _bufferId = bufferId;
            _bufferSize = bufferSize;
            _poolId = poolId;
            _bucketId = bucketId;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "bufferId":
                    value = _bufferId;
                    return true;
                case "bufferSize":
                    value = _bufferSize;
                    return true;
                case "poolId":
                    value = _poolId;
                    return true;
                case "bucketId":
                    value = _bucketId;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("bufferId", _bufferId);
            yield return new KeyValuePair<string, object>("bufferSize", _bufferSize);
            yield return new KeyValuePair<string, object>("poolId", _poolId);
            yield return new KeyValuePair<string, object>("bucketId", _bucketId);
        }
    }

    private sealed class BufferAllocatedPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("bufferId", NetTraceTypeCode.Int32),
            new("bufferSize", NetTraceTypeCode.Int32),
            new("poolId", NetTraceTypeCode.Int32),
            new("bucketId", NetTraceTypeCode.Int32),
            new("reason", NetTraceTypeCode.Int32),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new BufferAllocatedPayload(
                reader.ReadInt32(),
                reader.ReadInt32(),
                reader.ReadInt32(),
                reader.ReadInt32(),
                reader.ReadInt32());
        }

        private readonly int _bufferId;
        private readonly int _bufferSize;
        private readonly int _poolId;
        private readonly int _bucketId;
        private readonly int _reason;

        private BufferAllocatedPayload(int bufferId, int bufferSize, int poolId, int bucketId, int reason)
        {
            _bufferId = bufferId;
            _bufferSize = bufferSize;
            _poolId = poolId;
            _bucketId = bucketId;
            _reason = reason;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "bufferId":
                    value = _bufferId;
                    return true;
                case "bufferSize":
                    value = _bufferSize;
                    return true;
                case "poolId":
                    value = _poolId;
                    return true;
                case "bucketId":
                    value = _bucketId;
                    return true;
                case "reason":
                    value = _reason;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("bufferId", _bufferId);
            yield return new KeyValuePair<string, object>("bufferSize", _bufferSize);
            yield return new KeyValuePair<string, object>("poolId", _poolId);
            yield return new KeyValuePair<string, object>("bucketId", _bucketId);
            yield return new KeyValuePair<string, object>("reason", _reason);
        }
    }

    private sealed class BufferReturnedPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("bufferId", NetTraceTypeCode.Int32),
            new("bufferSize", NetTraceTypeCode.Int32),
            new("poolId", NetTraceTypeCode.Int32),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new BufferReturnedPayload(
                reader.ReadInt32(),
                reader.ReadInt32(),
                reader.ReadInt32());
        }

        private readonly int _bufferId;
        private readonly int _bufferSize;
        private readonly int _poolId;

        private BufferReturnedPayload(int bufferId, int bufferSize, int poolId)
        {
            _bufferId = bufferId;
            _bufferSize = bufferSize;
            _poolId = poolId;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "bufferId":
                    value = _bufferId;
                    return true;
                case "bufferSize":
                    value = _bufferSize;
                    return true;
                case "poolId":
                    value = _poolId;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("bufferId", _bufferId);
            yield return new KeyValuePair<string, object>("bufferSize", _bufferSize);
            yield return new KeyValuePair<string, object>("poolId", _poolId);
        }
    }

    private sealed class BufferTrimmedPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("bufferId", NetTraceTypeCode.Int32),
            new("bufferSize", NetTraceTypeCode.Int32),
            new("poolId", NetTraceTypeCode.Int32),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new BufferTrimmedPayload(
                reader.ReadInt32(),
                reader.ReadInt32(),
                reader.ReadInt32());
        }

        private readonly int _bufferId;
        private readonly int _bufferSize;
        private readonly int _poolId;

        private BufferTrimmedPayload(int bufferId, int bufferSize, int poolId)
        {
            _bufferId = bufferId;
            _bufferSize = bufferSize;
            _poolId = poolId;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "bufferId":
                    value = _bufferId;
                    return true;
                case "bufferSize":
                    value = _bufferSize;
                    return true;
                case "poolId":
                    value = _poolId;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("bufferId", _bufferId);
            yield return new KeyValuePair<string, object>("bufferSize", _bufferSize);
            yield return new KeyValuePair<string, object>("poolId", _poolId);
        }
    }

    private sealed class BufferTrimPollPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("milliseconds", NetTraceTypeCode.Int32),
            new("pressure", NetTraceTypeCode.Int32),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new BufferTrimPollPayload(
                reader.ReadInt32(),
                reader.ReadInt32());
        }

        private readonly int _milliseconds;
        private readonly int _pressure;

        private BufferTrimPollPayload(int milliseconds, int pressure)
        {
            _milliseconds = milliseconds;
            _pressure = pressure;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "milliseconds":
                    value = _milliseconds;
                    return true;
                case "pressure":
                    value = _pressure;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("milliseconds", _milliseconds);
            yield return new KeyValuePair<string, object>("pressure", _pressure);
        }
    }

    private sealed class BufferDroppedPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("bufferId", NetTraceTypeCode.Int32),
            new("bufferSize", NetTraceTypeCode.Int32),
            new("poolId", NetTraceTypeCode.Int32),
            new("bucketId", NetTraceTypeCode.Int32),
            new("reason", NetTraceTypeCode.Int32),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new BufferDroppedPayload(
                reader.ReadInt32(),
                reader.ReadInt32(),
                reader.ReadInt32(),
                reader.ReadInt32(),
                reader.ReadInt32());
        }

        private readonly int _bufferId;
        private readonly int _bufferSize;
        private readonly int _poolId;
        private readonly int _bucketId;
        private readonly int _reason;

        private BufferDroppedPayload(int bufferId, int bufferSize, int poolId, int bucketId, int reason)
        {
            _bufferId = bufferId;
            _bufferSize = bufferSize;
            _poolId = poolId;
            _bucketId = bucketId;
            _reason = reason;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "bufferId":
                    value = _bufferId;
                    return true;
                case "bufferSize":
                    value = _bufferSize;
                    return true;
                case "poolId":
                    value = _poolId;
                    return true;
                case "bucketId":
                    value = _bucketId;
                    return true;
                case "reason":
                    value = _reason;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("bufferId", _bufferId);
            yield return new KeyValuePair<string, object>("bufferSize", _bufferSize);
            yield return new KeyValuePair<string, object>("poolId", _poolId);
            yield return new KeyValuePair<string, object>("bucketId", _bucketId);
            yield return new KeyValuePair<string, object>("reason", _reason);
        }
    }

    private sealed class RequestStartPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("scheme", NetTraceTypeCode.NullTerminatedUtf16String),
            new("host", NetTraceTypeCode.NullTerminatedUtf16String),
            new("port", NetTraceTypeCode.Int32),
            new("pathAndQuery", NetTraceTypeCode.NullTerminatedUtf16String),
            new("versionMajor", NetTraceTypeCode.Byte),
            new("versionMinor", NetTraceTypeCode.Byte),
            new("versionPolicy", NetTraceTypeCode.Int32),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new RequestStartPayload(
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadInt32(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadByte(),
                reader.ReadByte(),
                reader.ReadInt32());
        }

        private readonly string _scheme;
        private readonly string _host;
        private readonly int _port;
        private readonly string _pathAndQuery;
        private readonly byte _versionMajor;
        private readonly byte _versionMinor;
        private readonly int _versionPolicy;

        private RequestStartPayload(string scheme, string host, int port, string pathAndQuery, byte versionMajor, byte versionMinor, int versionPolicy)
        {
            _scheme = scheme;
            _host = host;
            _port = port;
            _pathAndQuery = pathAndQuery;
            _versionMajor = versionMajor;
            _versionMinor = versionMinor;
            _versionPolicy = versionPolicy;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "scheme":
                    value = _scheme;
                    return true;
                case "host":
                    value = _host;
                    return true;
                case "port":
                    value = _port;
                    return true;
                case "pathAndQuery":
                    value = _pathAndQuery;
                    return true;
                case "versionMajor":
                    value = _versionMajor;
                    return true;
                case "versionMinor":
                    value = _versionMinor;
                    return true;
                case "versionPolicy":
                    value = _versionPolicy;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("scheme", _scheme);
            yield return new KeyValuePair<string, object>("host", _host);
            yield return new KeyValuePair<string, object>("port", _port);
            yield return new KeyValuePair<string, object>("pathAndQuery", _pathAndQuery);
            yield return new KeyValuePair<string, object>("versionMajor", _versionMajor);
            yield return new KeyValuePair<string, object>("versionMinor", _versionMinor);
            yield return new KeyValuePair<string, object>("versionPolicy", _versionPolicy);
        }
    }

    private sealed class RequestStopPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("statusCode", NetTraceTypeCode.Int32),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new RequestStopPayload(
                reader.ReadInt32());
        }

        private readonly int _statusCode;

        private RequestStopPayload(int statusCode)
        {
            _statusCode = statusCode;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "statusCode":
                    value = _statusCode;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("statusCode", _statusCode);
        }
    }

    private sealed class RequestFailedPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("exceptionMessage", NetTraceTypeCode.NullTerminatedUtf16String),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new RequestFailedPayload(
                reader.ReadNullTerminatedUtf16String());
        }

        private readonly string _exceptionMessage;

        private RequestFailedPayload(string exceptionMessage)
        {
            _exceptionMessage = exceptionMessage;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "exceptionMessage":
                    value = _exceptionMessage;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("exceptionMessage", _exceptionMessage);
        }
    }

    private sealed class ConnectionEstablishedPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("versionMajor", NetTraceTypeCode.Byte),
            new("versionMinor", NetTraceTypeCode.Byte),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new ConnectionEstablishedPayload(
                reader.ReadByte(),
                reader.ReadByte());
        }

        private readonly byte _versionMajor;
        private readonly byte _versionMinor;

        private ConnectionEstablishedPayload(byte versionMajor, byte versionMinor)
        {
            _versionMajor = versionMajor;
            _versionMinor = versionMinor;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "versionMajor":
                    value = _versionMajor;
                    return true;
                case "versionMinor":
                    value = _versionMinor;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("versionMajor", _versionMajor);
            yield return new KeyValuePair<string, object>("versionMinor", _versionMinor);
        }
    }

    private sealed class ConnectionClosedPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("versionMajor", NetTraceTypeCode.Byte),
            new("versionMinor", NetTraceTypeCode.Byte),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new ConnectionClosedPayload(
                reader.ReadByte(),
                reader.ReadByte());
        }

        private readonly byte _versionMajor;
        private readonly byte _versionMinor;

        private ConnectionClosedPayload(byte versionMajor, byte versionMinor)
        {
            _versionMajor = versionMajor;
            _versionMinor = versionMinor;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "versionMajor":
                    value = _versionMajor;
                    return true;
                case "versionMinor":
                    value = _versionMinor;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("versionMajor", _versionMajor);
            yield return new KeyValuePair<string, object>("versionMinor", _versionMinor);
        }
    }

    private sealed class RequestLeftQueuePayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("timeOnQueueMilliseconds", NetTraceTypeCode.Double),
            new("versionMajor", NetTraceTypeCode.Byte),
            new("versionMinor", NetTraceTypeCode.Byte),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new RequestLeftQueuePayload(
                reader.ReadDouble(),
                reader.ReadByte(),
                reader.ReadByte());
        }

        private readonly double _timeOnQueueMilliseconds;
        private readonly byte _versionMajor;
        private readonly byte _versionMinor;

        private RequestLeftQueuePayload(double timeOnQueueMilliseconds, byte versionMajor, byte versionMinor)
        {
            _timeOnQueueMilliseconds = timeOnQueueMilliseconds;
            _versionMajor = versionMajor;
            _versionMinor = versionMinor;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "timeOnQueueMilliseconds":
                    value = _timeOnQueueMilliseconds;
                    return true;
                case "versionMajor":
                    value = _versionMajor;
                    return true;
                case "versionMinor":
                    value = _versionMinor;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("timeOnQueueMilliseconds", _timeOnQueueMilliseconds);
            yield return new KeyValuePair<string, object>("versionMajor", _versionMajor);
            yield return new KeyValuePair<string, object>("versionMinor", _versionMinor);
        }
    }

    private sealed class RequestHeaderStartPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("connectionId", NetTraceTypeCode.Int64),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new RequestHeaderStartPayload(
                reader.ReadInt64());
        }

        private readonly long _connectionId;

        private RequestHeaderStartPayload(long connectionId)
        {
            _connectionId = connectionId;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "connectionId":
                    value = _connectionId;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("connectionId", _connectionId);
        }
    }

    private sealed class RequestContentStopPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("contentLength", NetTraceTypeCode.Int64),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new RequestContentStopPayload(
                reader.ReadInt64());
        }

        private readonly long _contentLength;

        private RequestContentStopPayload(long contentLength)
        {
            _contentLength = contentLength;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "contentLength":
                    value = _contentLength;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("contentLength", _contentLength);
        }
    }

    private sealed class ResponseHeadersStopPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("statusCode", NetTraceTypeCode.Int32),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new ResponseHeadersStopPayload(
                reader.ReadInt32());
        }

        private readonly int _statusCode;

        private ResponseHeadersStopPayload(int statusCode)
        {
            _statusCode = statusCode;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "statusCode":
                    value = _statusCode;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("statusCode", _statusCode);
        }
    }

    private sealed class RequestFailedDetailedPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("exception", NetTraceTypeCode.NullTerminatedUtf16String),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new RequestFailedDetailedPayload(
                reader.ReadNullTerminatedUtf16String());
        }

        private readonly string _exception;

        private RequestFailedDetailedPayload(string exception)
        {
            _exception = exception;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "exception":
                    value = _exception;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("exception", _exception);
        }
    }

    private sealed class RedirectPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("redirectUri", NetTraceTypeCode.NullTerminatedUtf16String),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new RedirectPayload(
                reader.ReadNullTerminatedUtf16String());
        }

        private readonly string _redirectUri;

        private RedirectPayload(string redirectUri)
        {
            _redirectUri = redirectUri;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "redirectUri":
                    value = _redirectUri;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("redirectUri", _redirectUri);
        }
    }

    private sealed class ConnectStartPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("address", NetTraceTypeCode.NullTerminatedUtf16String),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new ConnectStartPayload(
                reader.ReadNullTerminatedUtf16String());
        }

        private readonly string _address;

        private ConnectStartPayload(string address)
        {
            _address = address;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "address":
                    value = _address;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("address", _address);
        }
    }

    private sealed class SocketErrorPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("error", NetTraceTypeCode.Int32),
            new("exceptionMessage", NetTraceTypeCode.NullTerminatedUtf16String),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new SocketErrorPayload(
                reader.ReadInt32(),
                reader.ReadNullTerminatedUtf16String());
        }

        private readonly int _error;
        private readonly string _exceptionMessage;

        private SocketErrorPayload(int error, string exceptionMessage)
        {
            _error = error;
            _exceptionMessage = exceptionMessage;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "error":
                    value = _error;
                    return true;
                case "exceptionMessage":
                    value = _exceptionMessage;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("error", _error);
            yield return new KeyValuePair<string, object>("exceptionMessage", _exceptionMessage);
        }
    }

    private sealed class AcceptStartPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("address", NetTraceTypeCode.NullTerminatedUtf16String),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new AcceptStartPayload(
                reader.ReadNullTerminatedUtf16String());
        }

        private readonly string _address;

        private AcceptStartPayload(string address)
        {
            _address = address;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "address":
                    value = _address;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("address", _address);
        }
    }

    private sealed class ResolutionStartPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("hostNameOrAddress", NetTraceTypeCode.NullTerminatedUtf16String),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new ResolutionStartPayload(
                reader.ReadNullTerminatedUtf16String());
        }

        private readonly string _hostNameOrAddress;

        private ResolutionStartPayload(string hostNameOrAddress)
        {
            _hostNameOrAddress = hostNameOrAddress;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "hostNameOrAddress":
                    value = _hostNameOrAddress;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("hostNameOrAddress", _hostNameOrAddress);
        }
    }

    private sealed class ServiceProviderBuiltPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("serviceProviderHashCode", NetTraceTypeCode.Int32),
            new("singletonServices", NetTraceTypeCode.Int32),
            new("scopedServices", NetTraceTypeCode.Int32),
            new("transientServices", NetTraceTypeCode.Int32),
            new("closedGenericsServices", NetTraceTypeCode.Int32),
            new("openGenericsServices", NetTraceTypeCode.Int32),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new ServiceProviderBuiltPayload(
                reader.ReadInt32(),
                reader.ReadInt32(),
                reader.ReadInt32(),
                reader.ReadInt32(),
                reader.ReadInt32(),
                reader.ReadInt32());
        }

        private readonly int _serviceProviderHashCode;
        private readonly int _singletonServices;
        private readonly int _scopedServices;
        private readonly int _transientServices;
        private readonly int _closedGenericsServices;
        private readonly int _openGenericsServices;

        private ServiceProviderBuiltPayload(int serviceProviderHashCode, int singletonServices, int scopedServices, int transientServices, int closedGenericsServices, int openGenericsServices)
        {
            _serviceProviderHashCode = serviceProviderHashCode;
            _singletonServices = singletonServices;
            _scopedServices = scopedServices;
            _transientServices = transientServices;
            _closedGenericsServices = closedGenericsServices;
            _openGenericsServices = openGenericsServices;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "serviceProviderHashCode":
                    value = _serviceProviderHashCode;
                    return true;
                case "singletonServices":
                    value = _singletonServices;
                    return true;
                case "scopedServices":
                    value = _scopedServices;
                    return true;
                case "transientServices":
                    value = _transientServices;
                    return true;
                case "closedGenericsServices":
                    value = _closedGenericsServices;
                    return true;
                case "openGenericsServices":
                    value = _openGenericsServices;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("serviceProviderHashCode", _serviceProviderHashCode);
            yield return new KeyValuePair<string, object>("singletonServices", _singletonServices);
            yield return new KeyValuePair<string, object>("scopedServices", _scopedServices);
            yield return new KeyValuePair<string, object>("transientServices", _transientServices);
            yield return new KeyValuePair<string, object>("closedGenericsServices", _closedGenericsServices);
            yield return new KeyValuePair<string, object>("openGenericsServices", _openGenericsServices);
        }
    }

    private sealed class ServiceProviderDescriptorsPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("serviceProviderHashCode", NetTraceTypeCode.Int32),
            new("descriptors", NetTraceTypeCode.NullTerminatedUtf16String),
            new("chunkIndex", NetTraceTypeCode.Int32),
            new("chunkCount", NetTraceTypeCode.Int32),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new ServiceProviderDescriptorsPayload(
                reader.ReadInt32(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadInt32(),
                reader.ReadInt32());
        }

        private readonly int _serviceProviderHashCode;
        private readonly string _descriptors;
        private readonly int _chunkIndex;
        private readonly int _chunkCount;

        private ServiceProviderDescriptorsPayload(int serviceProviderHashCode, string descriptors, int chunkIndex, int chunkCount)
        {
            _serviceProviderHashCode = serviceProviderHashCode;
            _descriptors = descriptors;
            _chunkIndex = chunkIndex;
            _chunkCount = chunkCount;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "serviceProviderHashCode":
                    value = _serviceProviderHashCode;
                    return true;
                case "descriptors":
                    value = _descriptors;
                    return true;
                case "chunkIndex":
                    value = _chunkIndex;
                    return true;
                case "chunkCount":
                    value = _chunkCount;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("serviceProviderHashCode", _serviceProviderHashCode);
            yield return new KeyValuePair<string, object>("descriptors", _descriptors);
            yield return new KeyValuePair<string, object>("chunkIndex", _chunkIndex);
            yield return new KeyValuePair<string, object>("chunkCount", _chunkCount);
        }
    }

    private sealed class ServiceResolvedPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("serviceType", NetTraceTypeCode.NullTerminatedUtf16String),
            new("serviceProviderHashCode", NetTraceTypeCode.Int32),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new ServiceResolvedPayload(
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadInt32());
        }

        private readonly string _serviceType;
        private readonly int _serviceProviderHashCode;

        private ServiceResolvedPayload(string serviceType, int serviceProviderHashCode)
        {
            _serviceType = serviceType;
            _serviceProviderHashCode = serviceProviderHashCode;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "serviceType":
                    value = _serviceType;
                    return true;
                case "serviceProviderHashCode":
                    value = _serviceProviderHashCode;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("serviceType", _serviceType);
            yield return new KeyValuePair<string, object>("serviceProviderHashCode", _serviceProviderHashCode);
        }
    }

    private sealed class ScopeDisposedPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("serviceProviderHashCode", NetTraceTypeCode.Int32),
            new("scopedServicesResolved", NetTraceTypeCode.Int32),
            new("disposableServices", NetTraceTypeCode.Int32),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new ScopeDisposedPayload(
                reader.ReadInt32(),
                reader.ReadInt32(),
                reader.ReadInt32());
        }

        private readonly int _serviceProviderHashCode;
        private readonly int _scopedServicesResolved;
        private readonly int _disposableServices;

        private ScopeDisposedPayload(int serviceProviderHashCode, int scopedServicesResolved, int disposableServices)
        {
            _serviceProviderHashCode = serviceProviderHashCode;
            _scopedServicesResolved = scopedServicesResolved;
            _disposableServices = disposableServices;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "serviceProviderHashCode":
                    value = _serviceProviderHashCode;
                    return true;
                case "scopedServicesResolved":
                    value = _scopedServicesResolved;
                    return true;
                case "disposableServices":
                    value = _disposableServices;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("serviceProviderHashCode", _serviceProviderHashCode);
            yield return new KeyValuePair<string, object>("scopedServicesResolved", _scopedServicesResolved);
            yield return new KeyValuePair<string, object>("disposableServices", _disposableServices);
        }
    }

    private sealed class MemoryStreamCreatedPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("guid", NetTraceTypeCode.Guid),
            new("tag", NetTraceTypeCode.NullTerminatedUtf16String),
            new("requestedSize", NetTraceTypeCode.Int64),
            new("actualSize", NetTraceTypeCode.Int64),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new MemoryStreamCreatedPayload(
                reader.ReadGuid(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadInt64(),
                reader.ReadInt64());
        }

        private readonly Guid _guid;
        private readonly string _tag;
        private readonly long _requestedSize;
        private readonly long _actualSize;

        private MemoryStreamCreatedPayload(Guid guid, string tag, long requestedSize, long actualSize)
        {
            _guid = guid;
            _tag = tag;
            _requestedSize = requestedSize;
            _actualSize = actualSize;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "guid":
                    value = _guid;
                    return true;
                case "tag":
                    value = _tag;
                    return true;
                case "requestedSize":
                    value = _requestedSize;
                    return true;
                case "actualSize":
                    value = _actualSize;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("guid", _guid);
            yield return new KeyValuePair<string, object>("tag", _tag);
            yield return new KeyValuePair<string, object>("requestedSize", _requestedSize);
            yield return new KeyValuePair<string, object>("actualSize", _actualSize);
        }
    }

    private sealed class MemoryStreamDisposedPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("guid", NetTraceTypeCode.Guid),
            new("tag", NetTraceTypeCode.NullTerminatedUtf16String),
            new("lifetimeMs", NetTraceTypeCode.Int64),
            new("allocationStack", NetTraceTypeCode.NullTerminatedUtf16String),
            new("disposeStack", NetTraceTypeCode.NullTerminatedUtf16String),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new MemoryStreamDisposedPayload(
                reader.ReadGuid(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadInt64(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String());
        }

        private readonly Guid _guid;
        private readonly string _tag;
        private readonly long _lifetimeMs;
        private readonly string _allocationStack;
        private readonly string _disposeStack;

        private MemoryStreamDisposedPayload(Guid guid, string tag, long lifetimeMs, string allocationStack, string disposeStack)
        {
            _guid = guid;
            _tag = tag;
            _lifetimeMs = lifetimeMs;
            _allocationStack = allocationStack;
            _disposeStack = disposeStack;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "guid":
                    value = _guid;
                    return true;
                case "tag":
                    value = _tag;
                    return true;
                case "lifetimeMs":
                    value = _lifetimeMs;
                    return true;
                case "allocationStack":
                    value = _allocationStack;
                    return true;
                case "disposeStack":
                    value = _disposeStack;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("guid", _guid);
            yield return new KeyValuePair<string, object>("tag", _tag);
            yield return new KeyValuePair<string, object>("lifetimeMs", _lifetimeMs);
            yield return new KeyValuePair<string, object>("allocationStack", _allocationStack);
            yield return new KeyValuePair<string, object>("disposeStack", _disposeStack);
        }
    }

    private sealed class MemoryStreamDoubleDisposePayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("guid", NetTraceTypeCode.Guid),
            new("tag", NetTraceTypeCode.NullTerminatedUtf16String),
            new("allocationStack", NetTraceTypeCode.NullTerminatedUtf16String),
            new("disposeStack1", NetTraceTypeCode.NullTerminatedUtf16String),
            new("disposeStack2", NetTraceTypeCode.NullTerminatedUtf16String),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new MemoryStreamDoubleDisposePayload(
                reader.ReadGuid(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String());
        }

        private readonly Guid _guid;
        private readonly string _tag;
        private readonly string _allocationStack;
        private readonly string _disposeStack1;
        private readonly string _disposeStack2;

        private MemoryStreamDoubleDisposePayload(Guid guid, string tag, string allocationStack, string disposeStack1, string disposeStack2)
        {
            _guid = guid;
            _tag = tag;
            _allocationStack = allocationStack;
            _disposeStack1 = disposeStack1;
            _disposeStack2 = disposeStack2;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "guid":
                    value = _guid;
                    return true;
                case "tag":
                    value = _tag;
                    return true;
                case "allocationStack":
                    value = _allocationStack;
                    return true;
                case "disposeStack1":
                    value = _disposeStack1;
                    return true;
                case "disposeStack2":
                    value = _disposeStack2;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("guid", _guid);
            yield return new KeyValuePair<string, object>("tag", _tag);
            yield return new KeyValuePair<string, object>("allocationStack", _allocationStack);
            yield return new KeyValuePair<string, object>("disposeStack1", _disposeStack1);
            yield return new KeyValuePair<string, object>("disposeStack2", _disposeStack2);
        }
    }

    private sealed class MemoryStreamFinalizedPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("guid", NetTraceTypeCode.Guid),
            new("tag", NetTraceTypeCode.NullTerminatedUtf16String),
            new("allocationStack", NetTraceTypeCode.NullTerminatedUtf16String),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new MemoryStreamFinalizedPayload(
                reader.ReadGuid(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String());
        }

        private readonly Guid _guid;
        private readonly string _tag;
        private readonly string _allocationStack;

        private MemoryStreamFinalizedPayload(Guid guid, string tag, string allocationStack)
        {
            _guid = guid;
            _tag = tag;
            _allocationStack = allocationStack;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "guid":
                    value = _guid;
                    return true;
                case "tag":
                    value = _tag;
                    return true;
                case "allocationStack":
                    value = _allocationStack;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("guid", _guid);
            yield return new KeyValuePair<string, object>("tag", _tag);
            yield return new KeyValuePair<string, object>("allocationStack", _allocationStack);
        }
    }

    private sealed class MemoryStreamToArrayPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("guid", NetTraceTypeCode.Guid),
            new("tag", NetTraceTypeCode.NullTerminatedUtf16String),
            new("stack", NetTraceTypeCode.NullTerminatedUtf16String),
            new("size", NetTraceTypeCode.Int64),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new MemoryStreamToArrayPayload(
                reader.ReadGuid(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadInt64());
        }

        private readonly Guid _guid;
        private readonly string _tag;
        private readonly string _stack;
        private readonly long _size;

        private MemoryStreamToArrayPayload(Guid guid, string tag, string stack, long size)
        {
            _guid = guid;
            _tag = tag;
            _stack = stack;
            _size = size;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "guid":
                    value = _guid;
                    return true;
                case "tag":
                    value = _tag;
                    return true;
                case "stack":
                    value = _stack;
                    return true;
                case "size":
                    value = _size;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("guid", _guid);
            yield return new KeyValuePair<string, object>("tag", _tag);
            yield return new KeyValuePair<string, object>("stack", _stack);
            yield return new KeyValuePair<string, object>("size", _size);
        }
    }

    private sealed class MemoryStreamManagerInitializedPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("blockSize", NetTraceTypeCode.Int32),
            new("largeBufferMultiple", NetTraceTypeCode.Int32),
            new("maximumBufferSize", NetTraceTypeCode.Int32),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new MemoryStreamManagerInitializedPayload(
                reader.ReadInt32(),
                reader.ReadInt32(),
                reader.ReadInt32());
        }

        private readonly int _blockSize;
        private readonly int _largeBufferMultiple;
        private readonly int _maximumBufferSize;

        private MemoryStreamManagerInitializedPayload(int blockSize, int largeBufferMultiple, int maximumBufferSize)
        {
            _blockSize = blockSize;
            _largeBufferMultiple = largeBufferMultiple;
            _maximumBufferSize = maximumBufferSize;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "blockSize":
                    value = _blockSize;
                    return true;
                case "largeBufferMultiple":
                    value = _largeBufferMultiple;
                    return true;
                case "maximumBufferSize":
                    value = _maximumBufferSize;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("blockSize", _blockSize);
            yield return new KeyValuePair<string, object>("largeBufferMultiple", _largeBufferMultiple);
            yield return new KeyValuePair<string, object>("maximumBufferSize", _maximumBufferSize);
        }
    }

    private sealed class MemoryStreamNewBlockCreatedPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("smallPoolInUseBytes", NetTraceTypeCode.Int64),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new MemoryStreamNewBlockCreatedPayload(
                reader.ReadInt64());
        }

        private readonly long _smallPoolInUseBytes;

        private MemoryStreamNewBlockCreatedPayload(long smallPoolInUseBytes)
        {
            _smallPoolInUseBytes = smallPoolInUseBytes;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "smallPoolInUseBytes":
                    value = _smallPoolInUseBytes;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("smallPoolInUseBytes", _smallPoolInUseBytes);
        }
    }

    private sealed class MemoryStreamNewLargeBufferCreatedPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("requiredSize", NetTraceTypeCode.Int64),
            new("largePoolInUseBytes", NetTraceTypeCode.Int64),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new MemoryStreamNewLargeBufferCreatedPayload(
                reader.ReadInt64(),
                reader.ReadInt64());
        }

        private readonly long _requiredSize;
        private readonly long _largePoolInUseBytes;

        private MemoryStreamNewLargeBufferCreatedPayload(long requiredSize, long largePoolInUseBytes)
        {
            _requiredSize = requiredSize;
            _largePoolInUseBytes = largePoolInUseBytes;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "requiredSize":
                    value = _requiredSize;
                    return true;
                case "largePoolInUseBytes":
                    value = _largePoolInUseBytes;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("requiredSize", _requiredSize);
            yield return new KeyValuePair<string, object>("largePoolInUseBytes", _largePoolInUseBytes);
        }
    }

    private sealed class MemoryStreamNonPooledLargeBufferCreatedPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("guid", NetTraceTypeCode.Guid),
            new("tag", NetTraceTypeCode.NullTerminatedUtf16String),
            new("requiredSize", NetTraceTypeCode.Int64),
            new("allocationStack", NetTraceTypeCode.NullTerminatedUtf16String),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new MemoryStreamNonPooledLargeBufferCreatedPayload(
                reader.ReadGuid(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadInt64(),
                reader.ReadNullTerminatedUtf16String());
        }

        private readonly Guid _guid;
        private readonly string _tag;
        private readonly long _requiredSize;
        private readonly string _allocationStack;

        private MemoryStreamNonPooledLargeBufferCreatedPayload(Guid guid, string tag, long requiredSize, string allocationStack)
        {
            _guid = guid;
            _tag = tag;
            _requiredSize = requiredSize;
            _allocationStack = allocationStack;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "guid":
                    value = _guid;
                    return true;
                case "tag":
                    value = _tag;
                    return true;
                case "requiredSize":
                    value = _requiredSize;
                    return true;
                case "allocationStack":
                    value = _allocationStack;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("guid", _guid);
            yield return new KeyValuePair<string, object>("tag", _tag);
            yield return new KeyValuePair<string, object>("requiredSize", _requiredSize);
            yield return new KeyValuePair<string, object>("allocationStack", _allocationStack);
        }
    }

    private sealed class MemoryStreamDiscardBufferPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("guid", NetTraceTypeCode.Guid),
            new("tag", NetTraceTypeCode.NullTerminatedUtf16String),
            new("bufferType", NetTraceTypeCode.Int32),
            new("reason", NetTraceTypeCode.Int32),
            new("smallBlocksFree", NetTraceTypeCode.Int64),
            new("smallPoolBytesFree", NetTraceTypeCode.Int64),
            new("smallPoolBytesInUse", NetTraceTypeCode.Int64),
            new("largeBlocksFree", NetTraceTypeCode.Int64),
            new("largePoolBytesFree", NetTraceTypeCode.Int64),
            new("largePoolBytesInUse", NetTraceTypeCode.Int64),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new MemoryStreamDiscardBufferPayload(
                reader.ReadGuid(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadInt32(),
                reader.ReadInt32(),
                reader.ReadInt64(),
                reader.ReadInt64(),
                reader.ReadInt64(),
                reader.ReadInt64(),
                reader.ReadInt64(),
                reader.ReadInt64());
        }

        private readonly Guid _guid;
        private readonly string _tag;
        private readonly int _bufferType;
        private readonly int _reason;
        private readonly long _smallBlocksFree;
        private readonly long _smallPoolBytesFree;
        private readonly long _smallPoolBytesInUse;
        private readonly long _largeBlocksFree;
        private readonly long _largePoolBytesFree;
        private readonly long _largePoolBytesInUse;

        private MemoryStreamDiscardBufferPayload(Guid guid, string tag, int bufferType, int reason, long smallBlocksFree, long smallPoolBytesFree, long smallPoolBytesInUse, long largeBlocksFree, long largePoolBytesFree, long largePoolBytesInUse)
        {
            _guid = guid;
            _tag = tag;
            _bufferType = bufferType;
            _reason = reason;
            _smallBlocksFree = smallBlocksFree;
            _smallPoolBytesFree = smallPoolBytesFree;
            _smallPoolBytesInUse = smallPoolBytesInUse;
            _largeBlocksFree = largeBlocksFree;
            _largePoolBytesFree = largePoolBytesFree;
            _largePoolBytesInUse = largePoolBytesInUse;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "guid":
                    value = _guid;
                    return true;
                case "tag":
                    value = _tag;
                    return true;
                case "bufferType":
                    value = _bufferType;
                    return true;
                case "reason":
                    value = _reason;
                    return true;
                case "smallBlocksFree":
                    value = _smallBlocksFree;
                    return true;
                case "smallPoolBytesFree":
                    value = _smallPoolBytesFree;
                    return true;
                case "smallPoolBytesInUse":
                    value = _smallPoolBytesInUse;
                    return true;
                case "largeBlocksFree":
                    value = _largeBlocksFree;
                    return true;
                case "largePoolBytesFree":
                    value = _largePoolBytesFree;
                    return true;
                case "largePoolBytesInUse":
                    value = _largePoolBytesInUse;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("guid", _guid);
            yield return new KeyValuePair<string, object>("tag", _tag);
            yield return new KeyValuePair<string, object>("bufferType", _bufferType);
            yield return new KeyValuePair<string, object>("reason", _reason);
            yield return new KeyValuePair<string, object>("smallBlocksFree", _smallBlocksFree);
            yield return new KeyValuePair<string, object>("smallPoolBytesFree", _smallPoolBytesFree);
            yield return new KeyValuePair<string, object>("smallPoolBytesInUse", _smallPoolBytesInUse);
            yield return new KeyValuePair<string, object>("largeBlocksFree", _largeBlocksFree);
            yield return new KeyValuePair<string, object>("largePoolBytesFree", _largePoolBytesFree);
            yield return new KeyValuePair<string, object>("largePoolBytesInUse", _largePoolBytesInUse);
        }
    }

    private sealed class MemoryStreamOverCapacityPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("guid", NetTraceTypeCode.Guid),
            new("tag", NetTraceTypeCode.NullTerminatedUtf16String),
            new("requestedCapacity", NetTraceTypeCode.Int64),
            new("maxCapacity", NetTraceTypeCode.Int64),
            new("allocationStack", NetTraceTypeCode.NullTerminatedUtf16String),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new MemoryStreamOverCapacityPayload(
                reader.ReadGuid(),
                reader.ReadNullTerminatedUtf16String(),
                reader.ReadInt64(),
                reader.ReadInt64(),
                reader.ReadNullTerminatedUtf16String());
        }

        private readonly Guid _guid;
        private readonly string _tag;
        private readonly long _requestedCapacity;
        private readonly long _maxCapacity;
        private readonly string _allocationStack;

        private MemoryStreamOverCapacityPayload(Guid guid, string tag, long requestedCapacity, long maxCapacity, string allocationStack)
        {
            _guid = guid;
            _tag = tag;
            _requestedCapacity = requestedCapacity;
            _maxCapacity = maxCapacity;
            _allocationStack = allocationStack;
        }

        public int Count => FieldDefinitions.Length;

        public object this[string key] => TryGetValue(key, out object? val)
            ? val
            : throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");

        public IEnumerable<string> Keys => FieldDefinitions.Select(d => d.Name);

        public bool ContainsKey(string key)
        {
            return TryGetValue(key, out _);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            switch (key)
            {
                case "guid":
                    value = _guid;
                    return true;
                case "tag":
                    value = _tag;
                    return true;
                case "requestedCapacity":
                    value = _requestedCapacity;
                    return true;
                case "maxCapacity":
                    value = _maxCapacity;
                    return true;
                case "allocationStack":
                    value = _allocationStack;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }

        public IEnumerable<object> Values => GetKeyValues().Select(kvp => kvp.Value);

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetKeyValues().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<KeyValuePair<string, object>> GetKeyValues()
        {
            yield return new KeyValuePair<string, object>("guid", _guid);
            yield return new KeyValuePair<string, object>("tag", _tag);
            yield return new KeyValuePair<string, object>("requestedCapacity", _requestedCapacity);
            yield return new KeyValuePair<string, object>("maxCapacity", _maxCapacity);
            yield return new KeyValuePair<string, object>("allocationStack", _allocationStack);
        }
    }
}