using System.Collections.Frozen;
using System.Diagnostics.Tracing;

namespace EventPipe;

internal static class KnownEvents
{
    private const string RuntimeProvider = "Microsoft-Windows-DotNETRuntime";
    private const string RundownProvider = "Microsoft-Windows-DotNETRuntimeRundown";

    // Some Microsoft-Windows-DotNETRuntime events have incomplete metadata in the trace (https://github.com/dotnet/runtime/issues/96365)
    // so they have to be hardcoded here.
    public static readonly FrozenDictionary<Key, EventMetadata> All =
        new Dictionary<Key, EventMetadata>
        {
            [new Key(RuntimeProvider, 1, 2)] = new(default, "", default, "GCStart", default, default, default,
                null, new EventFieldDefinition[]
                {
                    new("Count", TypeCode.UInt32),
                    new("Depth", TypeCode.UInt32),
                    new("Reason", TypeCode.UInt32),
                    new("Type", TypeCode.UInt32),
                    new("ClrInstanceID", TypeCode.UInt16),
                    new("ClientSequenceNumber", TypeCode.UInt64),
                }),
            [new Key(RuntimeProvider, 2, 1)] = new(default, "", default, "GCStop", default, default, default,
                null, new EventFieldDefinition[]
                {
                    new("Count", TypeCode.UInt32),
                    new("Depth", TypeCode.UInt32),
                    new("ClrInstanceID", TypeCode.UInt16),
                }),
            [new Key(RuntimeProvider, 3, 1)] = new(default, "", default, "GCRestartEEEnd", default, default, default,
                null, new EventFieldDefinition[]
                {
                    new("ClrInstanceID", TypeCode.UInt16),
                }),
            [new Key(RuntimeProvider, 4, 2)] = new(default, "", default, "GCHeapStats", default, default, default,
                null, new EventFieldDefinition[]
                {
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
                }),
            [new Key(RuntimeProvider, 7, 1)] = new(default, "", default, "GCRestartEEBegin", default, default, default,
                null, new EventFieldDefinition[]
                {
                    new("ClrInstanceID", TypeCode.UInt16),
                }),
            [new Key(RuntimeProvider, 8, 1)] = new(default, "", default, "GCSuspendEEEnd", default, default, default,
                null, new EventFieldDefinition[]
                {
                    new("ClrInstanceID", TypeCode.UInt16),
                }),
            [new Key(RuntimeProvider, 9, 1)] = new(default, "", default, "GCSuspendEEBegin", default, default, default,
                null, new EventFieldDefinition[]
                {
                    new("Reason", TypeCode.UInt32),
                    new("Count", TypeCode.UInt32),
                    new("ClrInstanceID", TypeCode.UInt16),
                }),
            [new Key(RuntimeProvider, 10, 4)] = new(default, string.Empty, default, "GCAllocationTick", default,
                default, default,
                null, new EventFieldDefinition[]
                {
                    new("AllocationAmount", TypeCode.UInt32),
                    new("AllocationKind", TypeCode.UInt32),
                    new("ClrInstanceID", TypeCode.UInt16),
                    new("AllocationAmount64", TypeCode.UInt64),
                    new("TypeID", TypeCode.UInt64),
                    new("TypeName", TypeCode.String),
                    new("HeapIndex", TypeCode.UInt32),
                    new("Address", TypeCode.UInt64),
                    new("ObjectSize", TypeCode.UInt64),
                }),
            [new Key(RuntimeProvider, 13, 1)] = new(default, "", default, "GCFinalizersEnd", default, default, default,
                null, new EventFieldDefinition[]
                {
                    new("Count", TypeCode.UInt32),
                    new("ClrInstanceID", TypeCode.UInt16),
                }),
            [new Key(RuntimeProvider, 14, 1)] = new(default, "", default, "GCFinalizersBegin", default, default,
                default,
                null, new EventFieldDefinition[]
                {
                    new("ClrInstanceID", TypeCode.UInt16),
                }),
            // [new Key(RuntimeProvider, 22, 0)] = new(default, "", default, "GCBulkMovedObjectRanges", default, default, default, null, new EventFieldDefinition[]
            [new Key(RuntimeProvider, 23, 0)] = new(default, "", default, "GCGenerationRange", default, default,
                default, null, new EventFieldDefinition[]
                {
                    new("Generation", TypeCode.Byte),
                    new("RangeStart", TypeCode.UInt64),
                    new("RangeUsedLength", TypeCode.UInt64),
                    new("RangeReservedLength", TypeCode.UInt64),
                    new("ClrInstanceID", TypeCode.UInt16),
                }),
            [new Key(RuntimeProvider, 29, 0)] = new(default, "", default, "FinalizeObject", default, default, default,
                null, new EventFieldDefinition[]
                {
                    new("TypeID", TypeCode.UInt64),
                    new("ObjectID", TypeCode.UInt64),
                    new("ClrInstanceID", TypeCode.UInt16),
                }),
            [new Key(RuntimeProvider, 30, 0)] = new(default, "", default, "SetGCHandle", default, default, default,
                null, new EventFieldDefinition[]
                {
                    new("HandleID", TypeCode.UInt64),
                    new("ObjectID", TypeCode.UInt64),
                    new("Kind", TypeCode.UInt32),
                    new("Generation", TypeCode.UInt32),
                    new("AppDomainID", TypeCode.UInt64),
                    new("ClrInstanceID", TypeCode.UInt16),
                }),
            [new Key(RuntimeProvider, 31, 0)] = new(default, "", default, "DestroyGCHandle", default, default, default,
                null, new EventFieldDefinition[]
                {
                    new("HandleID", TypeCode.UInt64),
                    new("ClrInstanceID", TypeCode.UInt16),
                }),
            [new Key(RuntimeProvider, 33, 0)] = new(default, "", default, "PinObjectAtGCTime", default, default,
                default, null, new EventFieldDefinition[]
                {
                    new("HandleID", TypeCode.UInt64),
                    new("ObjectID", TypeCode.UInt64),
                    new("ObjectSize", TypeCode.UInt64),
                    new("TypeName", TypeCode.String),
                    new("ClrInstanceID", TypeCode.UInt16),
                }),
            [new Key(RuntimeProvider, 35, 0)] = new(default, "", default, "GCTriggered", default, default, default,
                null, new EventFieldDefinition[]
                {
                    new("Reason", TypeCode.UInt32),
                    new("ClrInstanceID", TypeCode.UInt16),
                }),
            [new Key(RuntimeProvider, 54, 0)] = new(default, "", default, "ThreadPoolWorkerThreadAdjustmentSample",
                default, default, default, null, new EventFieldDefinition[]
                {
                    new("Throughput", TypeCode.Double),
                    new("ClrInstanceID", TypeCode.UInt16),
                }),
            [new Key(RuntimeProvider, 55, 0)] = new(default, "", default, "ThreadPoolWorkerThreadAdjustmentAdjustment",
                default, default, default, null, new EventFieldDefinition[]
                {
                    new("AverageThroughput", TypeCode.Double),
                    new("NewWorkerThreadCount", TypeCode.UInt32),
                    new("Reason", TypeCode.UInt32),
                    new("ClrInstanceID", TypeCode.UInt16),
                }),
            [new Key(RuntimeProvider, 56, 0)] = new(default, "", default, "ThreadPoolWorkerThreadAdjustmentStats",
                default, default, default, null, new EventFieldDefinition[]
                {
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
                }),
            [new Key(RuntimeProvider, 57, 0)] = new(default, "", default, "ThreadPoolWorkerThreadWait", default,
                default, default, null, new EventFieldDefinition[]
                {
                    new("ActiveWorkerThreadCount", TypeCode.UInt32),
                    new("RetiredWorkerThreadCount", TypeCode.UInt32),
                    new("ClrInstanceID", TypeCode.UInt16),
                }),
            [new Key(RuntimeProvider, 58, 0)] = new(default, "", default, "YieldProcessorMeasurement", default, default,
                default, null, new EventFieldDefinition[]
                {
                    new("ClrInstanceID", TypeCode.UInt16),
                    new("NsPerYield", TypeCode.Double),
                    new("EstablishedNsPerYield", TypeCode.Double),
                }),
            [new Key(RuntimeProvider, 80, 1)] = new(default, "", default, "ExceptionStart", default, default, default,
                null, new EventFieldDefinition[]
                {
                    new("ExceptionType", TypeCode.String),
                    new("ExceptionMessage", TypeCode.String),
                    new("ExceptionEIP", TypeCode.UInt64),
                    new("ExceptionHRESULT", TypeCode.UInt32),
                    new("ExceptionFlags", TypeCode.UInt16),
                    new("ClrInstanceID", TypeCode.UInt16),
                }),
            [new Key(RuntimeProvider, 81, 1)] =
                new(default, string.Empty, default, "ContentionStart", default, default, default,
                    EventOpcode.Start, new EventFieldDefinition[]
                    {
                        new("ContentionFlags", TypeCode.Byte),
                        new("ClrInstanceID", TypeCode.UInt16),
                    }),
            [new Key(RuntimeProvider, 81, 2)] =
                new(default, string.Empty, default, "ContentionStart", default, default, default,
                    EventOpcode.Start, new EventFieldDefinition[]
                    {
                        new("ContentionFlags", TypeCode.Byte),
                        new("ClrInstanceID", TypeCode.UInt16),
                        new("LockID", TypeCode.UInt64),
                        new("AssociatedObjectID", TypeCode.UInt64),
                        new("LockOwnerThreadID", TypeCode.UInt64),
                    }),
            [new Key(RuntimeProvider, 85, 0)] = new(default, "", default, "ThreadCreated", default, default, default,
                null, new EventFieldDefinition[]
                {
                    new("ManagedThreadID", TypeCode.UInt64),
                    new("AppDomainID", TypeCode.UInt64),
                    new("Flags", TypeCode.UInt32),
                    new("ManagedThreadIndex", TypeCode.UInt32),
                    new("OSThreadID", TypeCode.UInt32),
                    new("ClrInstanceID", TypeCode.UInt16),
                }),
            [new Key(RuntimeProvider, 88, 0)] = new(default, "", default, "ILStubGenerated", default, default, default,
                null, new EventFieldDefinition[]
                {
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
                }),
            [new Key(RuntimeProvider, 91, 1)] =
                new(default, string.Empty, default, "ContentionStop", default, default, default,
                    EventOpcode.Stop, new EventFieldDefinition[]
                    {
                        new("ContentionFlags", TypeCode.Byte),
                        new("ClrInstanceID", TypeCode.UInt16),
                        new("DurationNs", TypeCode.Double),
                    }),
            [new Key(RuntimeProvider, 143, 1)] = new(default, "", default, "MethodLoadVerbose", default, default,
                default, null, new EventFieldDefinition[]
                {
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
                }),
            [new Key(RuntimeProvider, 143, 2)] = new(default, "", default, "MethodLoadVerbose", default, default,
                default, null, new EventFieldDefinition[]
                {
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
                }),
            [new Key(RuntimeProvider, 145, 1)] = new(default, "", default, "MethodJittingStarted", default, default,
                default, null, new EventFieldDefinition[]
                {
                    new("MethodID", TypeCode.UInt64),
                    new("ModuleID", TypeCode.UInt64),
                    new("MethodToken", TypeCode.UInt32),
                    new("MethodILSize", TypeCode.UInt32),
                    new("MethodNamespace", TypeCode.String),
                    new("MethodName", TypeCode.String),
                    new("MethodSignature", TypeCode.String),
                    new("ClrInstanceID", TypeCode.UInt16),
                }),
            [new Key(RuntimeProvider, 146, 0)] = new(default, "", default, "MemoryAllocatedForJitCode", default,
                default, default, null, new EventFieldDefinition[]
                {
                    new("MethodID", TypeCode.UInt64),
                    new("ModuleID", TypeCode.UInt64),
                    new("JitHotCodeRequestSize", TypeCode.UInt64),
                    new("JitRODataRequestSize", TypeCode.UInt64),
                    new("AllocatedSizeForJitCode", TypeCode.UInt64),
                    new("JitAllocFlag", TypeCode.UInt32),
                    new("ClrInstanceID", TypeCode.UInt16),
                }),
            [new Key(RuntimeProvider, 185, 0)] = new(default, "", default, "JitInliningSucceeded", default, default,
                default, null, new EventFieldDefinition[]
                {
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
                }),
            [new Key(RuntimeProvider, 188, 0)] = new(default, "", default, "JitTailCallSucceeded", default, default,
                default, null, new EventFieldDefinition[]
                {
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
                }),
            [new Key(RuntimeProvider, 192, 0)] = new(default, "", default, "JitInliningFailed", default, default,
                default, null, new EventFieldDefinition[]
                {
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
                }),
            [new Key(RuntimeProvider, 202, 0)] = new(default, "", default, "GCMarkWithType", default, default, default,
                null, new EventFieldDefinition[]
                {
                    new("HeapNum", TypeCode.UInt32),
                    new("ClrInstanceID", TypeCode.UInt16),
                    new("Type", TypeCode.UInt32),
                    new("Bytes", TypeCode.UInt64),
                }),
            [new Key(RuntimeProvider, 203, 2)] = new(default, "", default, "GCJoin", default, default, default,
                null, new EventFieldDefinition[]
                {
                    new("Heap", TypeCode.UInt32),
                    new("JoinTime", TypeCode.UInt32),
                    new("JoinType", TypeCode.UInt32),
                    new("ClrInstanceID", TypeCode.UInt16),
                    new("JoinID", TypeCode.UInt32),
                }),
            // [new Key(RuntimeProvider, 204, 3)] = new(default, "", default, "GCPerHeapHistory", default, default, default, null, new EventFieldDefinition[]
            // [new Key(RuntimeProvider, 205, 4)] = new(default, "", default, "GCGlobalHeapHistory", default, default, default, null, new EventFieldDefinition[]
            // [new Key(RuntimeProvider, 209, 0)] = new(default, "", default, "GCFitBucketInfo", default, default, default, null, new EventFieldDefinition[]
            [new Key(RuntimeProvider, 250, 0)] = new(default, "", default, "ExceptionCatchStart", default, default,
                default, null, new EventFieldDefinition[]
                {
                    new("EntryEIP", TypeCode.UInt64),
                    new("MethodID", TypeCode.UInt64),
                    new("MethodName", TypeCode.String),
                    new("ClrInstanceID", TypeCode.UInt16),
                }),
            [new Key(RuntimeProvider, 252, 0)] = new(default, "", default, "ExceptionFinallyStart", default, default,
                default, null, new EventFieldDefinition[]
                {
                    new("EntryEIP", TypeCode.UInt64),
                    new("MethodID", TypeCode.UInt64),
                    new("MethodName", TypeCode.String),
                    new("ClrInstanceID", TypeCode.UInt16),
                }),
            [new Key(RuntimeProvider, 254, 0)] = new(default, "", default, "ExceptionFilterStart", default, default,
                default, null, new EventFieldDefinition[]
                {
                    new("EntryEIP", TypeCode.UInt64),
                    new("MethodID", TypeCode.UInt64),
                    new("MethodName", TypeCode.String),
                    new("ClrInstanceID", TypeCode.UInt16),
                }),
            [new Key(RuntimeProvider, 301, 0)] =
                new(default, string.Empty, default, "WaitHandleWaitStart", default, default, default,
                    EventOpcode.Start, new EventFieldDefinition[]
                    {
                        new("WaitSource", TypeCode.Byte),
                        new("AssociatedObjectID", TypeCode.UInt64),
                        new("ClrInstanceID", TypeCode.UInt16),
                    }),
            [new Key(RuntimeProvider, 302, 0)] =
                new(default, string.Empty, default, "WaitHandleWaitStop", default, default, default,
                    EventOpcode.Stop, new EventFieldDefinition[]
                    {
                        new("ClrInstanceID", TypeCode.UInt16),
                    }),
            [new Key(RundownProvider, 144, 1)] = new(default, "", default, "MethodUnloadVerbose", default, default,
                default, null, new EventFieldDefinition[]
                {
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
                }),
            [new Key(RundownProvider, 144, 2)] = new(default, "", default, "MethodUnloadVerbose", default, default,
                default, null, new EventFieldDefinition[]
                {
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
                }),
            [new Key(RundownProvider, 146, 1)] = new(default, "", default, "DCEndComplete", default, default, default,
                null, new EventFieldDefinition[]
                {
                    new("ClrInstanceID", TypeCode.UInt16),
                }),
            [new Key(RundownProvider, 148, 1)] = new(default, "", default, "DCEndInit", default, default, default,
                null, new EventFieldDefinition[]
                {
                    new("ClrInstanceID", TypeCode.UInt16),
                }),
            [new Key(RundownProvider, 154, 2)] = new(default, "", default, "ModuleDCEnd", default, default, default,
                null, new EventFieldDefinition[]
                {
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
                }),
            // [new Key(RundownProvider, 156, 1)] = new(default, "", default, "AppDomainLoad", default, default, default, null, new EventFieldDefinition[]
            [new Key(RundownProvider, 158, 1)] = new(default, "", default, "AppDomainDCEnd", default, default, default,
                null, new EventFieldDefinition[]
                {
                    new("AppDomainID", TypeCode.UInt64),
                    new("AppDomainFlags", TypeCode.UInt32),
                    new("AppDomainName", TypeCode.String),
                    new("AppDomainIndex", TypeCode.UInt32),
                    new("ClrInstanceID", TypeCode.UInt16),
                }),
            [new Key(RundownProvider, 187, 0)] = new(default, "", default, "RuntimeInformation", default, default,
                default, null, new EventFieldDefinition[]
                {
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
                }),
        }.ToFrozenDictionary();

    public record struct Key(string ProviderName, int EventId, int Version);
}