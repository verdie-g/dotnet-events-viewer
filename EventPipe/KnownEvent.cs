using System.Collections;
using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Tracing;
using EventPipe.FastSerializer;

namespace EventPipe;

/// <summary>
/// Some Microsoft-Windows-DotNETRuntime events have incomplete metadata in the trace (https://github.com/dotnet/runtime/issues/96365)
/// so they have to be hardcoded here. Also, it allows to have custom parsers for them which greatly improve the
/// performance and memory footprint.
/// </summary>
internal class KnownEvent
{
    public const string RuntimeProvider = "Microsoft-Windows-DotNETRuntime";
    public const string RundownProvider = "Microsoft-Windows-DotNETRuntimeRundown";
    public const string TplProvider = "System.Threading.Tasks.TplEventSource";

    public static readonly FrozenDictionary<Key, KnownEvent> All = new Dictionary<Key, KnownEvent>
    {
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
        [new Key(RuntimeProvider, 81, 1)] = new("ContentionStart", null, ContentionPayload.FieldDefinitions, ContentionPayload.Parse),
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
        [new Key(TplProvider, 7, 1)] = new("TaskScheduled", EventOpcode.Send, TaskScheduledPayload.FieldDefinitions, TaskScheduledPayload.Parse),
        [new Key(TplProvider, 8, 0)] = new("TaskStarted", null, TaskStartedPayload.FieldDefinitions, TaskStartedPayload.Parse),
        [new Key(TplProvider, 9, 1)] = new("TaskCompleted", null, TaskCompletedPayload.FieldDefinitions, TaskCompletedPayload.Parse),
        [new Key(TplProvider, 10, 3)] = new("TaskWaitBegin", EventOpcode.Send, TaskWaitBeginPayload.FieldDefinitions, TaskWaitBeginPayload.Parse),
        [new Key(TplProvider, 11, 0)] = new("TaskWaitEnd", null, TaskWaitEndPayload.FieldDefinitions, TaskWaitEndPayload.Parse),
        [new Key(TplProvider, 12, 0)] = new("AwaitTaskContinuationScheduled", EventOpcode.Send, AwaitTaskContinuationScheduledPayload.FieldDefinitions, AwaitTaskContinuationScheduledPayload.Parse),
        [new Key(TplProvider, 13, 0)] = new("TaskWaitContinuationComplete", null, TaskWaitContinuationPayload.FieldDefinitions, TaskWaitContinuationPayload.Parse),
        [new Key(TplProvider, 19, 0)] = new("TaskWaitContinuationStarted", null, TaskWaitContinuationPayload.FieldDefinitions, TaskWaitContinuationPayload.Parse),
    }.ToFrozenDictionary();

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

    private class GcStartV2Payload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("Count", TypeCode.UInt32),
            new("Depth", TypeCode.UInt32),
            new("Reason", TypeCode.UInt32),
            new("Type", TypeCode.UInt32),
            new("ClrInstanceID", TypeCode.UInt16),
            new("ClientSequenceNumber", TypeCode.UInt64),
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

    private class GcEndV1Payload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("Count", TypeCode.UInt32),
            new("Depth", TypeCode.UInt32),
            new("ClrInstanceID", TypeCode.UInt16),
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

    private class GcNoUserDataPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("ClrInstanceID", TypeCode.UInt16),
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

    private class GcHeapStatsV2Payload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("GenerationSize0", TypeCode.UInt64),
            new("TotalPromotedSize0", TypeCode.UInt64),
            new("GenerationSize1", TypeCode.UInt64),
            new("TotalPromotedSize1", TypeCode.UInt64),
            new("GenerationSize2", TypeCode.UInt64),
            new("TotalPromotedSize2", TypeCode.UInt64),
            new("GenerationSize3", TypeCode.UInt64),
            new("TotalPromotedSize3", TypeCode.UInt64),
            new("FinalizationPromotedSize", TypeCode.UInt64),
            new("FinalizationPromotedCount", TypeCode.UInt64),
            new("PinnedObjectCount", TypeCode.UInt32),
            new("SinkBlockCount", TypeCode.UInt32),
            new("GCHandleCount", TypeCode.UInt32),
            new("ClrInstanceID", TypeCode.UInt16),
            new("GenerationSize4", TypeCode.UInt64),
            new("TotalPromotedSize4", TypeCode.UInt64),
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

    private class GcSuspendEeV1Payload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("Reason", TypeCode.UInt32),
            new("Count", TypeCode.UInt32),
            new("ClrInstanceID", TypeCode.UInt16),
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

    private class GcAllocationTickV4Payload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("AllocationAmount", TypeCode.UInt32),
            new("AllocationKind", TypeCode.UInt32),
            new("ClrInstanceID", TypeCode.UInt16),
            new("AllocationAmount64", TypeCode.UInt64),
            new("TypeID", TypeCode.UInt64),
            new("TypeName", TypeCode.String),
            new("HeapIndex", TypeCode.UInt32),
            new("Address", TypeCode.UInt64),
            new("ObjectSize", TypeCode.UInt64),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new GcAllocationTickV4Payload(
                reader.ReadUInt32(),
                reader.ReadUInt32(),
                reader.ReadUInt16(),
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadNullTerminatedString(),
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

    private class GcFinalizersEndV1Payload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("Count", TypeCode.UInt32),
            new("ClrInstanceID", TypeCode.UInt16),
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

    private class GcGenerationRangePayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("Generation", TypeCode.Byte),
            new("RangeStart", TypeCode.UInt64),
            new("RangeUsedLength", TypeCode.UInt64),
            new("RangeReservedLength", TypeCode.UInt64),
            new("ClrInstanceID", TypeCode.UInt16),
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

    private class FinalizeObjectPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("TypeID", TypeCode.UInt64),
            new("ObjectID", TypeCode.UInt64),
            new("ClrInstanceID", TypeCode.UInt16),
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

    private class SetGcHandlePayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("HandleID", TypeCode.UInt64),
            new("ObjectID", TypeCode.UInt64),
            new("Kind", TypeCode.UInt32),
            new("Generation", TypeCode.UInt32),
            new("AppDomainID", TypeCode.UInt64),
            new("ClrInstanceID", TypeCode.UInt16),
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

    private class DestroyGcHandlePayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("HandleID", TypeCode.UInt64),
            new("ClrInstanceID", TypeCode.UInt16),
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

    private class PinObjectAtGcTimePayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("HandleID", TypeCode.UInt64),
            new("ObjectID", TypeCode.UInt64),
            new("ObjectSize", TypeCode.UInt64),
            new("TypeName", TypeCode.String),
            new("ClrInstanceID", TypeCode.UInt16),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new PinObjectAtGcTimePayload(
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadNullTerminatedString(),
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

    private class GcTriggeredPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("Reason", TypeCode.UInt32),
            new("ClrInstanceID", TypeCode.UInt16),
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

    private class ThreadPoolWorkerThreadAdjustmentSamplePayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("Throughput", TypeCode.Double),
            new("ClrInstanceID", TypeCode.UInt16),
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

    private class ThreadPoolWorkerThreadAdjustmentAdjustmentPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("AverageThroughput", TypeCode.Double),
            new("NewWorkerThreadCount", TypeCode.UInt32),
            new("Reason", TypeCode.UInt32),
            new("ClrInstanceID", TypeCode.UInt16),
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

    private class ThreadPoolWorkerThreadAdjustmentStatsPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("Duration", TypeCode.Double),
            new("Throughput", TypeCode.Double),
            new("ThreadWave", TypeCode.Double),
            new("ThroughputWave", TypeCode.Double),
            new("ThroughputErrorEstimate", TypeCode.Double),
            new("AverageThroughputErrorEstimate", TypeCode.Double),
            new("ThroughputRatio", TypeCode.Double),
            new("Confidence", TypeCode.Double),
            new("NewControlSetting", TypeCode.Double),
            new("NewThreadWaveMagnitude", TypeCode.UInt16),
            new("ClrInstanceID", TypeCode.UInt16),
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

    private class ThreadPoolWorkerThreadPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("ActiveWorkerThreadCount", TypeCode.UInt32),
            new("RetiredWorkerThreadCount", TypeCode.UInt32),
            new("ClrInstanceID", TypeCode.UInt16),
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

    private class YieldProcessorMeasurementPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("ClrInstanceID", TypeCode.UInt16),
            new("NsPerYield", TypeCode.Double),
            new("EstablishedNsPerYield", TypeCode.Double),
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

    private class ExceptionPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("ExceptionType", TypeCode.String),
            new("ExceptionMessage", TypeCode.String),
            new("ExceptionEIP", TypeCode.UInt64),
            new("ExceptionHRESULT", TypeCode.UInt32),
            new("ExceptionFlags", TypeCode.UInt16),
            new("ClrInstanceID", TypeCode.UInt16),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new ExceptionPayload(
                reader.ReadNullTerminatedString(),
                reader.ReadNullTerminatedString(),
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

    private class ContentionPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("ContentionFlags", TypeCode.Byte),
            new("ClrInstanceID", TypeCode.UInt16),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new ContentionPayload(
                reader.ReadByte(),
                reader.ReadUInt16());
        }

        private readonly byte _contentionFlags;
        private readonly ushort _clrInstanceId;

        private ContentionPayload(byte contentionFlags, ushort clrInstanceId)
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

    private class ThreadCreatedPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("ManagedThreadID", TypeCode.UInt64),
            new("AppDomainID", TypeCode.UInt64),
            new("Flags", TypeCode.UInt32),
            new("ManagedThreadIndex", TypeCode.UInt32),
            new("OSThreadID", TypeCode.UInt32),
            new("ClrInstanceID", TypeCode.UInt16),
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

    private class IlStubGeneratedPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("ClrInstanceID", TypeCode.UInt16),
            new("ModuleID", TypeCode.UInt64),
            new("StubMethodID", TypeCode.UInt64),
            new("StubFlags", TypeCode.UInt32),
            new("ManagedInteropMethodToken", TypeCode.UInt32),
            new("ManagedInteropMethodNamespace", TypeCode.String),
            new("ManagedInteropMethodName", TypeCode.String),
            new("ManagedInteropMethodSignature", TypeCode.String),
            new("NativeMethodSignature", TypeCode.String),
            new("StubMethodSignature", TypeCode.String),
            new("StubMethodILCode", TypeCode.String),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new IlStubGeneratedPayload(
                reader.ReadUInt16(),
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadUInt32(),
                reader.ReadUInt32(),
                reader.ReadNullTerminatedString(),
                reader.ReadNullTerminatedString(),
                reader.ReadNullTerminatedString(),
                reader.ReadNullTerminatedString(),
                reader.ReadNullTerminatedString(),
                reader.ReadNullTerminatedString());
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

    private class ContentionStopV1Payload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("ContentionFlags", TypeCode.Byte),
            new("ClrInstanceID", TypeCode.UInt16),
            new("DurationNs", TypeCode.Double),
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

    private class MethodLoadUnloadVerboseV1Payload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("MethodID", TypeCode.UInt64),
            new("ModuleID", TypeCode.UInt64),
            new("MethodStartAddress", TypeCode.UInt64),
            new("MethodSize", TypeCode.UInt32),
            new("MethodToken", TypeCode.UInt32),
            new("MethodFlags", TypeCode.UInt32),
            new("MethodNamespace", TypeCode.String),
            new("MethodName", TypeCode.String),
            new("MethodSignature", TypeCode.String),
            new("ClrInstanceID", TypeCode.UInt16),
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
                reader.ReadNullTerminatedString(),
                reader.ReadNullTerminatedString(),
                reader.ReadNullTerminatedString(),
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

    private class MethodLoadUnloadVerboseV2Payload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("MethodID", TypeCode.UInt64),
            new("ModuleID", TypeCode.UInt64),
            new("MethodStartAddress", TypeCode.UInt64),
            new("MethodSize", TypeCode.UInt32),
            new("MethodToken", TypeCode.UInt32),
            new("MethodFlags", TypeCode.UInt32),
            new("MethodNamespace", TypeCode.String),
            new("MethodName", TypeCode.String),
            new("MethodSignature", TypeCode.String),
            new("ClrInstanceID", TypeCode.UInt16),
            new("ReJITID", TypeCode.UInt64),
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
                reader.ReadNullTerminatedString(),
                reader.ReadNullTerminatedString(),
                reader.ReadNullTerminatedString(),
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

    private class MethodJittingStartedV1Payload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("MethodID", TypeCode.UInt64),
            new("ModuleID", TypeCode.UInt64),
            new("MethodToken", TypeCode.UInt32),
            new("MethodILSize", TypeCode.UInt32),
            new("MethodNamespace", TypeCode.String),
            new("MethodName", TypeCode.String),
            new("MethodSignature", TypeCode.String),
            new("ClrInstanceID", TypeCode.UInt16),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new MethodJittingStartedV1Payload(
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadUInt32(),
                reader.ReadUInt32(),
                reader.ReadNullTerminatedString(),
                reader.ReadNullTerminatedString(),
                reader.ReadNullTerminatedString(),
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

    private class MethodJitMemoryAllocatedForCodePayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("MethodID", TypeCode.UInt64),
            new("ModuleID", TypeCode.UInt64),
            new("JitHotCodeRequestSize", TypeCode.UInt64),
            new("JitRODataRequestSize", TypeCode.UInt64),
            new("AllocatedSizeForJitCode", TypeCode.UInt64),
            new("JitAllocFlag", TypeCode.UInt32),
            new("ClrInstanceID", TypeCode.UInt16),
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

    private class MethodJitInliningSucceededPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("MethodBeingCompiledNamespace", TypeCode.String),
            new("MethodBeingCompiledName", TypeCode.String),
            new("MethodBeingCompiledNameSignature", TypeCode.String),
            new("InlinerNamespace", TypeCode.String),
            new("InlinerName", TypeCode.String),
            new("InlinerNameSignature", TypeCode.String),
            new("InlineeNamespace", TypeCode.String),
            new("InlineeName", TypeCode.String),
            new("InlineeNameSignature", TypeCode.String),
            new("ClrInstanceID", TypeCode.UInt16),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new MethodJitInliningSucceededPayload(
                reader.ReadNullTerminatedString(),
                reader.ReadNullTerminatedString(),
                reader.ReadNullTerminatedString(),
                reader.ReadNullTerminatedString(),
                reader.ReadNullTerminatedString(),
                reader.ReadNullTerminatedString(),
                reader.ReadNullTerminatedString(),
                reader.ReadNullTerminatedString(),
                reader.ReadNullTerminatedString(),
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

    private class MethodJitTailCallSucceededPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("MethodBeingCompiledNamespace", TypeCode.String),
            new("MethodBeingCompiledName", TypeCode.String),
            new("MethodBeingCompiledNameSignature", TypeCode.String),
            new("CallerNamespace", TypeCode.String),
            new("CallerName", TypeCode.String),
            new("CallerNameSignature", TypeCode.String),
            new("CalleeNamespace", TypeCode.String),
            new("CalleeName", TypeCode.String),
            new("CalleeNameSignature", TypeCode.String),
            new("TailPrefix", TypeCode.Boolean),
            new("TailCallType", TypeCode.UInt32),
            new("ClrInstanceID", TypeCode.UInt16),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new MethodJitTailCallSucceededPayload(
                reader.ReadNullTerminatedString(),
                reader.ReadNullTerminatedString(),
                reader.ReadNullTerminatedString(),
                reader.ReadNullTerminatedString(),
                reader.ReadNullTerminatedString(),
                reader.ReadNullTerminatedString(),
                reader.ReadNullTerminatedString(),
                reader.ReadNullTerminatedString(),
                reader.ReadNullTerminatedString(),
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

    private class MethodJitInliningFailedPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("MethodBeingCompiledNamespace", TypeCode.String),
            new("MethodBeingCompiledName", TypeCode.String),
            new("MethodBeingCompiledNameSignature", TypeCode.String),
            new("InlinerNamespace", TypeCode.String),
            new("InlinerName", TypeCode.String),
            new("InlinerNameSignature", TypeCode.String),
            new("InlineeNamespace", TypeCode.String),
            new("InlineeName", TypeCode.String),
            new("InlineeNameSignature", TypeCode.String),
            new("FailAlways", TypeCode.Boolean),
            new("FailReason", TypeCode.String),
            new("ClrInstanceID", TypeCode.UInt16),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new MethodJitInliningFailedPayload(
                reader.ReadNullTerminatedString(),
                reader.ReadNullTerminatedString(),
                reader.ReadNullTerminatedString(),
                reader.ReadNullTerminatedString(),
                reader.ReadNullTerminatedString(),
                reader.ReadNullTerminatedString(),
                reader.ReadNullTerminatedString(),
                reader.ReadNullTerminatedString(),
                reader.ReadNullTerminatedString(),
                reader.ReadInt32() != 0,
                reader.ReadNullTerminatedString(),
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

    private class GcMarkWithTypePayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("HeapNum", TypeCode.UInt32),
            new("ClrInstanceID", TypeCode.UInt16),
            new("Type", TypeCode.UInt32),
            new("Bytes", TypeCode.UInt64),
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

    private class GcJoinV2Payload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("Heap", TypeCode.UInt32),
            new("JoinTime", TypeCode.UInt32),
            new("JoinType", TypeCode.UInt32),
            new("ClrInstanceID", TypeCode.UInt16),
            new("JoinID", TypeCode.UInt32),
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

    private class ExceptionHandlingPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("EntryEIP", TypeCode.UInt64),
            new("MethodID", TypeCode.UInt64),
            new("MethodName", TypeCode.String),
            new("ClrInstanceID", TypeCode.UInt16),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new ExceptionHandlingPayload(
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadNullTerminatedString(),
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

    private class WaitHandleWaitStartPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("WaitSource", TypeCode.Byte),
            new("AssociatedObjectID", TypeCode.UInt64),
            new("ClrInstanceID", TypeCode.UInt16),
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

    private class WaitHandleWaitStopPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("ClrInstanceID", TypeCode.UInt16),
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

    private class MethodLoadUnloadRundownVerboseV1Payload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("MethodID", TypeCode.UInt64),
            new("ModuleID", TypeCode.UInt64),
            new("MethodStartAddress", TypeCode.UInt64),
            new("MethodSize", TypeCode.UInt32),
            new("MethodToken", TypeCode.UInt32),
            new("MethodFlags", TypeCode.UInt32),
            new("MethodNamespace", TypeCode.String),
            new("MethodName", TypeCode.String),
            new("MethodSignature", TypeCode.String),
            new("ClrInstanceID", TypeCode.UInt16),
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
                reader.ReadNullTerminatedString(),
                reader.ReadNullTerminatedString(),
                reader.ReadNullTerminatedString(),
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

    private class MethodLoadUnloadRundownVerboseV2Payload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("MethodID", TypeCode.UInt64),
            new("ModuleID", TypeCode.UInt64),
            new("MethodStartAddress", TypeCode.UInt64),
            new("MethodSize", TypeCode.UInt32),
            new("MethodToken", TypeCode.UInt32),
            new("MethodFlags", TypeCode.UInt32),
            new("MethodNamespace", TypeCode.String),
            new("MethodName", TypeCode.String),
            new("MethodSignature", TypeCode.String),
            new("ClrInstanceID", TypeCode.UInt16),
            new("ReJITID", TypeCode.UInt64),
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
                reader.ReadNullTerminatedString(),
                reader.ReadNullTerminatedString(),
                reader.ReadNullTerminatedString(),
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

    private class DcStartEndPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("ClrInstanceID", TypeCode.UInt16),
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

    private class DomainModuleLoadUnloadRundownV1Payload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("ModuleID", TypeCode.UInt64),
            new("AssemblyID", TypeCode.UInt64),
            new("AppDomainID", TypeCode.UInt64),
            new("ModuleFlags", TypeCode.UInt32),
            new("Reserved1", TypeCode.UInt32),
            new("ModuleILPath", TypeCode.String),
            new("ModuleNativePath", TypeCode.String),
            new("ClrInstanceID", TypeCode.UInt16),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new DomainModuleLoadUnloadRundownV1Payload(
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadUInt32(),
                reader.ReadUInt32(),
                reader.ReadNullTerminatedString(),
                reader.ReadNullTerminatedString(),
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

    private class ModuleLoadUnloadRundownV2Payload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("ModuleID", TypeCode.UInt64),
            new("AssemblyID", TypeCode.UInt64),
            new("ModuleFlags", TypeCode.UInt32),
            new("Reserved1", TypeCode.UInt32),
            new("ModuleILPath", TypeCode.String),
            new("ModuleNativePath", TypeCode.String),
            new("ClrInstanceID", TypeCode.UInt16),
            new("ManagedPdbSignature", TypeCodeExtensions.Guid),
            new("ManagedPdbAge", TypeCode.UInt32),
            new("ManagedPdbBuildPath", TypeCode.String),
            new("NativePdbSignature", TypeCodeExtensions.Guid),
            new("NativePdbAge", TypeCode.UInt32),
            new("NativePdbBuildPath", TypeCode.String),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new ModuleLoadUnloadRundownV2Payload(
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadUInt32(),
                reader.ReadUInt32(),
                reader.ReadNullTerminatedString(),
                reader.ReadNullTerminatedString(),
                reader.ReadUInt16(),
                reader.ReadGuid(),
                reader.ReadUInt32(),
                reader.ReadNullTerminatedString(),
                reader.ReadGuid(),
                reader.ReadUInt32(),
                reader.ReadNullTerminatedString());
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

    private class AssemblyLoadUnloadRundownV1Payload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("AssemblyID", TypeCode.UInt64),
            new("AppDomainID", TypeCode.UInt64),
            new("BindingID", TypeCode.UInt64),
            new("AssemblyFlags", TypeCode.UInt32),
            new("FullyQualifiedAssemblyName", TypeCode.String),
            new("ClrInstanceID", TypeCode.UInt16),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new AssemblyLoadUnloadRundownV1Payload(
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadUInt64(),
                reader.ReadUInt32(),
                reader.ReadNullTerminatedString(),
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

    private class AppDomainLoadUnloadRundownV1Payload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("AppDomainID", TypeCode.UInt64),
            new("AppDomainFlags", TypeCode.UInt32),
            new("AppDomainName", TypeCode.String),
            new("AppDomainIndex", TypeCode.UInt32),
            new("ClrInstanceID", TypeCode.UInt16),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new AppDomainLoadUnloadRundownV1Payload(
                reader.ReadUInt64(),
                reader.ReadUInt32(),
                reader.ReadNullTerminatedString(),
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

    private class RuntimeInformationRundownPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("ClrInstanceID", TypeCode.UInt16),
            new("Sku", TypeCode.UInt16),
            new("BclMajorVersion", TypeCode.UInt16),
            new("BclMinorVersion", TypeCode.UInt16),
            new("BclBuildNumber", TypeCode.UInt16),
            new("BclQfeNumber", TypeCode.UInt16),
            new("VMMajorVersion", TypeCode.UInt16),
            new("VMMinorVersion", TypeCode.UInt16),
            new("VMBuildNumber", TypeCode.UInt16),
            new("VMQfeNumber", TypeCode.UInt16),
            new("StartupFlags", TypeCode.UInt32),
            new("StartupMode", TypeCode.Byte),
            new("CommandLine", TypeCode.String),
            new("ComObjectGuid", TypeCodeExtensions.Guid),
            new("RuntimeDllPath", TypeCode.String),
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
                reader.ReadNullTerminatedString(),
                reader.ReadGuid(),
                reader.ReadNullTerminatedString());
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

    private class TaskScheduledPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("OriginatingTaskSchedulerID", TypeCode.Int32),
            new("OriginatingTaskID", TypeCode.Int32),
            new("TaskID", TypeCode.Int32),
            new("CreatingTaskID", TypeCode.Int32),
            new("TaskCreationOptions", TypeCode.Int32),
            new("appDomain", TypeCode.Int32),
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

        public TaskScheduledPayload(int originatingTaskSchedulerId, int originatingTaskId, int taskId,
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

    private class TaskStartedPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("OriginatingTaskSchedulerID", TypeCode.Int32),
            new("OriginatingTaskID", TypeCode.Int32),
            new("TaskID", TypeCode.Int32),
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

        public TaskStartedPayload(int originatingTaskSchedulerId, int originatingTaskId, int taskId)
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

    private class TaskCompletedPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("OriginatingTaskSchedulerID", TypeCode.Int32),
            new("OriginatingTaskID", TypeCode.Int32),
            new("TaskID", TypeCode.Int32),
            new("IsExceptional", TypeCode.Boolean),
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

        public TaskCompletedPayload(int originatingTaskSchedulerId, int originatingTaskId, int taskId, bool isExceptional)
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

    private class TaskWaitBeginPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("OriginatingTaskSchedulerID", TypeCode.Int32),
            new("OriginatingTaskID", TypeCode.Int32),
            new("TaskID", TypeCode.Int32),
            new("Behavior", TypeCode.Int32),
            new("ContinueWithTaskID", TypeCode.Int32),
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

        public TaskWaitBeginPayload(int originatingTaskSchedulerId, int originatingTaskId, int taskId,
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

    private class TaskWaitEndPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("OriginatingTaskSchedulerID", TypeCode.Int32),
            new("OriginatingTaskID", TypeCode.Int32),
            new("TaskID", TypeCode.Int32),
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

        public TaskWaitEndPayload(int originatingTaskSchedulerId, int originatingTaskId, int taskId)
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

    private class AwaitTaskContinuationScheduledPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("OriginatingTaskSchedulerID", TypeCode.Int32),
            new("OriginatingTaskID", TypeCode.Int32),
            new("ContinueWithTaskID", TypeCode.Int32),
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

        public AwaitTaskContinuationScheduledPayload(int originatingTaskSchedulerId, int originatingTaskId,
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

    private class TaskWaitContinuationPayload : IReadOnlyDictionary<string, object>
    {
        public static EventFieldDefinition[] FieldDefinitions { get; } =
        [
            new("TaskID", TypeCode.Int32),
        ];

        public static IReadOnlyDictionary<string, object> Parse(ref FastSerializerSequenceReader reader)
        {
            return new TaskWaitContinuationPayload(reader.ReadInt32());
        }

        private readonly int _taskId;

        public TaskWaitContinuationPayload(int taskId)
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
}