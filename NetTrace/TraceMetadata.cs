namespace NetTrace;

public record TraceMetadata(
    DateTime Date,
    long QueryPerformanceCounterSyncTime,
    long QueryPerformanceCounterFrequency,
    int PointerSize,
    int ProcessId,
    int NumberOfProcessors,
    int CpuSamplingRate);