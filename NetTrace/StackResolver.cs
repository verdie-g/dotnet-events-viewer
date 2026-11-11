using System.Runtime.InteropServices;

namespace NetTrace;

/// <summary>
/// Resolves stack trace addresses to method symbols within the event's process.
///
/// Managed symbols come from Microsoft-Windows-DotNETRuntimeRundown events. Native symbols
/// come from Universal.System ProcessMapping and ProcessSymbol events. Mapping IDs can be
/// associated with multiple processes, and symbols can arrive before their mappings, so native
/// symbols are bound to all process associations only when resolving the completed trace.
/// </summary>
internal class StackResolver
{
    private static readonly MethodDescription UnresolvedMethodDescription = new("??", "", "", 0, 0);

    private readonly Dictionary<ulong[], StackTraceGroup> _groupedStackTraces;
    private readonly Dictionary<ulong[], StackTraceGroup>.AlternateLookup<ReadOnlySpan<ulong>> _groupedStackTracesAlternate;
    private readonly Dictionary<long, List<MethodDescription>> _methodDescriptions = [];
    private readonly Dictionary<ulong, Dictionary<long, ProcessMapping>> _processMappings = [];
    private readonly List<ProcessSymbol> _processSymbols = [];

    public StackResolver()
    {
        _groupedStackTraces = new Dictionary<ulong[], StackTraceGroup>(new UInt64ArrayComparer());
        _groupedStackTracesAlternate = _groupedStackTraces.GetAlternateLookup<ReadOnlySpan<ulong>>();
    }

    public void AddProcessMapping(long processId, ulong id, ulong startAddress, ulong endAddress, ulong fileOffset, string fileName, ulong metadataId)
    {
        if (!_processMappings.TryGetValue(id, out var mappings))
        {
            mappings = [];
            _processMappings.Add(id, mappings);
        }

        mappings[processId] = new ProcessMapping(startAddress, endAddress, fileOffset, fileName, metadataId);
    }

    public void AddProcessSymbol(ulong mappingId, ulong startAddress, ulong endAddress, string name)
    {
        if (endAddress < startAddress)
        {
            return;
        }

        _processSymbols.Add(new ProcessSymbol(mappingId, startAddress, endAddress, name));
    }

    public void AddStackAddresses(int stackIndex, ReadOnlySpan<ulong> addresses)
    {
        if (!_groupedStackTracesAlternate.TryGetValue(addresses, out var group))
        {
            group = new StackTraceGroup
            {
                StackIndexes = [],
                Addresses = addresses.ToArray(),
            };
            _groupedStackTracesAlternate[addresses] = group;
        }

        group.StackIndexes.Add(stackIndex);
    }

    public void AddMethodSymbolInfo(long processId, MethodDescription methodDescription)
    {
        if (!_methodDescriptions.TryGetValue(processId, out var methods))
        {
            methods = [];
            _methodDescriptions.Add(processId, methods);
        }

        methods.Add(methodDescription);
    }

    public void ResolveEventStackTraces(List<Event> events)
    {
        Dictionary<int, StackTraceGroup> stackIndexToStackTraceGroup = new(_groupedStackTraces.Count);
        foreach (var group in _groupedStackTraces.Values)
        {
            foreach (var stackIndex in group.StackIndexes)
            {
                stackIndexToStackTraceGroup[stackIndex] = group;
            }
        }

        var symbolsByProcess = BuildSymbolLookups();
        Dictionary<(StackTraceGroup Group, long ProcessId), StackTrace> resolvedStackTraces = [];
        int nextStackTraceIndex = 0;
        foreach (var evt in events)
        {
            if (!stackIndexToStackTraceGroup.TryGetValue(evt.StackIndex, out var group))
            {
                evt.StackTrace = StackTrace.Empty;
                continue;
            }

            var key = (group, evt.ProcessId);
            if (!resolvedStackTraces.TryGetValue(key, out var stackTrace))
            {
                ReadOnlySpan<Symbol> symbols = symbolsByProcess.TryGetValue(evt.ProcessId, out var processSymbols)
                    ? CollectionsMarshal.AsSpan(processSymbols)
                    : [];
                stackTrace = ResolveStackTrace(nextStackTraceIndex, group.Addresses, symbols);
                if (stackTrace != StackTrace.Empty)
                {
                    nextStackTraceIndex += 1;
                }
                resolvedStackTraces.Add(key, stackTrace);
            }

            // Keep StackIndex as the original input ID; the resolved stack has its own unique Index.
            evt.StackTrace = stackTrace;
        }
    }

    private Dictionary<long, List<Symbol>> BuildSymbolLookups()
    {
        Dictionary<long, List<Symbol>> symbolsByProcess = [];
        foreach (var (processId, methods) in _methodDescriptions)
        {
            symbolsByProcess.Add(processId, methods.Select(method => new Symbol(method, 0, false)).ToList());
        }

        foreach (var symbol in _processSymbols)
        {
            if (!_processMappings.TryGetValue(symbol.MappingId, out var mappings))
            {
                continue;
            }

            foreach (var (processId, mapping) in mappings)
            {
                if (!symbolsByProcess.TryGetValue(processId, out var symbols))
                {
                    symbols = [];
                    symbolsByProcess.Add(processId, symbols);
                }

                // MethodDescription's constructor takes a uint managed size. Native ranges are
                // inclusive and may exceed 4 GB, so keep their full bounds in the local records.
                var method = new MethodDescription(symbol.Name, mapping.FileName, "", symbol.StartAddress, 0);
                symbols.Add(new Symbol(method, symbol.EndAddress, true));
            }
        }

        foreach (var symbols in symbolsByProcess.Values)
        {
            CollectionsMarshal.AsSpan(symbols).Sort((x, y) => x.Address.CompareTo(y.Address));
        }

        return symbolsByProcess;
    }

    private static StackTrace ResolveStackTrace(
        int stackIndex,
        ulong[] addresses,
        ReadOnlySpan<Symbol> symbols)
    {
        if (addresses.Length == 0)
        {
            return StackTrace.Empty;
        }

        var frames = new MethodDescription[addresses.Length];
        for (int i = 0; i < frames.Length; i += 1)
        {
            frames[i] = ResolveSymbol(symbols, addresses[i]);
        }

        return new StackTrace(stackIndex, frames, addresses);
    }

    private static MethodDescription ResolveSymbol(ReadOnlySpan<Symbol> symbols, ulong address)
    {
        // Find the last symbol whose start is at or below the instruction pointer.
        int lo = 0;
        int hi = symbols.Length;
        while (lo < hi)
        {
            int i = lo + (hi - lo) / 2;
            if (symbols[i].Address <= address)
            {
                lo = i + 1;
            }
            else
            {
                hi = i;
            }
        }

        if (lo == 0)
        {
            return UnresolvedMethodDescription;
        }

        var symbol = symbols[lo - 1];
        bool containsAddress = symbol.IsNative
            ? address <= symbol.EndAddress
            : address - symbol.Address < symbol.Method.Size;
        return containsAddress ? symbol.Method : UnresolvedMethodDescription;
    }

    private sealed class StackTraceGroup
    {
        public required List<int> StackIndexes { get; init; }
        public required ulong[] Addresses { get; init; }
    }

    private sealed class UInt64ArrayComparer : IEqualityComparer<ulong[]>, IAlternateEqualityComparer<ReadOnlySpan<ulong>, ulong[]>
    {
        public bool Equals(ulong[]? x, ulong[]? y)
        {
            return x!.Length == y!.Length && x.SequenceEqual(y);
        }

        public bool Equals(ReadOnlySpan<ulong> alternate, ulong[] other)
        {
            return alternate.Length == other.Length && alternate.SequenceEqual(other);
        }

        public int GetHashCode(ulong[] obj)
        {
            return GetHashCode(obj.AsSpan());
        }

        public int GetHashCode(ReadOnlySpan<ulong> alternate)
        {
            HashCode hash = new();
            foreach (ulong x in alternate)
            {
                hash.Add(x);
            }

            return hash.ToHashCode();
        }

        public ulong[] Create(ReadOnlySpan<ulong> alternate)
        {
            return alternate.ToArray();
        }
    }

    private sealed record ProcessMapping(ulong StartAddress, ulong EndAddress, ulong FileOffset, string FileName, ulong MetadataId);
    private sealed record ProcessSymbol(ulong MappingId, ulong StartAddress, ulong EndAddress, string Name);
    private sealed record Symbol(MethodDescription Method, ulong EndAddress, bool IsNative)
    {
        public ulong Address => Method.Address;
    }
}
