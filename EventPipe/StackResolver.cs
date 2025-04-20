using System.Runtime.InteropServices;

namespace EventPipe;

internal class StackResolver
{
    private static readonly MethodDescription UnresolvedMethodDescription = new("??", "", "", 0, 0);

    private readonly Dictionary<ulong[], StackTraceGroup> _groupedStackTraces;
    private readonly Dictionary<ulong[], StackTraceGroup>.AlternateLookup<ReadOnlySpan<ulong>> _groupedStackTracesAlternate;
    private readonly List<MethodDescription> _methodDescriptions;

    public StackResolver()
    {
        _groupedStackTraces = new Dictionary<ulong[], StackTraceGroup>(new UInt64ArrayComparer());
        _groupedStackTracesAlternate = _groupedStackTraces.GetAlternateLookup<ReadOnlySpan<ulong>>();
        _methodDescriptions = [];
    }

    public void AddStackAddresses(int stackIndex, ReadOnlySpan<ulong> addresses)
    {
        if (!_groupedStackTracesAlternate.TryGetValue(addresses, out var group))
        {
            group = new StackTraceGroup
            {
                StackIndexes = [],
                Addresses = addresses.ToArray(),
                StackTrace = null,
            };
            _groupedStackTracesAlternate[addresses] = group;
        }

        group.StackIndexes.Add(stackIndex);
    }

    public void AddMethodSymbolInfo(MethodDescription methodDescription)
    {
        _methodDescriptions.Add(methodDescription);
    }

    public void ResolveEventStackTraces(List<Event> events)
    {
        Dictionary<int, StackTraceGroup> stackIndexToStackTraceGroup = new(_groupedStackTraces.Count);
        foreach (var kvp in _groupedStackTraces)
        {
            foreach (var stackIndex in kvp.Value.StackIndexes)
            {
                stackIndexToStackTraceGroup[stackIndex] = kvp.Value;
            }
        }

        var methodDescriptions = CollectionsMarshal.AsSpan(_methodDescriptions);
        methodDescriptions.Sort((x, y) => (int)(x.Address - y.Address));

        foreach (var evt in events)
        {
            if (!stackIndexToStackTraceGroup.TryGetValue(evt.StackIndex, out var group))
            {
                evt.StackTrace = StackTrace.Empty;
                continue;
            }

            group.StackTrace ??= ResolveStackTrace(evt.StackIndex, group.Addresses, methodDescriptions);

            evt.StackIndex = group.StackTrace.Index;
            evt.StackTrace = group.StackTrace;
        }
    }

    private static StackTrace ResolveStackTrace(
        int stackIndex,
        ulong[] addresses,
        ReadOnlySpan<MethodDescription> methodDescriptions)
    {
        if (addresses.Length == 0)
        {
            return StackTrace.Empty;
        }

        var stackTrace = new MethodDescription[addresses.Length];
        for (int i = 0; i < stackTrace.Length; i += 1)
        {
            stackTrace[i] = ResolveSymbol(methodDescriptions, addresses[i]);

        }

        return new StackTrace(stackIndex, stackTrace);
    }

    private static MethodDescription ResolveSymbol(
        ReadOnlySpan<MethodDescription> methodDescriptions,
        ulong address)
    {
        int methodAddressIdx = BinarySearchMethodAddress(methodDescriptions, address);
        if (methodAddressIdx < 0)
        {
            methodAddressIdx = ~methodAddressIdx - 1;
            if (methodAddressIdx == -1) // Input address is lower than the lowest method address.
            {
                return UnresolvedMethodDescription;
            }
        }

        var methodDescription = methodDescriptions[methodAddressIdx];
        if (methodDescription.Address + methodDescription.Size < address)
        {
            return UnresolvedMethodDescription;
        }

        return methodDescription;
    }

    private static int BinarySearchMethodAddress(ReadOnlySpan<MethodDescription> methodDescriptions, ulong address)
    {
        int lo = 0;
        int hi = methodDescriptions.Length - 1;
        while (lo <= hi)
        {
            int i = lo + (hi - lo) / 2;

            if (methodDescriptions[i].Address == address)
            {
                return i;
            }

            if (methodDescriptions[i].Address < address)
            {
                lo = i + 1;
            }
            else
            {
                hi = i - 1;
            }
        }

        return ~lo;
    }

    private sealed class StackTraceGroup
    {
        public required List<int> StackIndexes { get; init; }
        public required ulong[] Addresses { get; init; }
        public StackTrace? StackTrace { get; set; }
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
}