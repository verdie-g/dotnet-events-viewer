using System.Runtime.InteropServices;

namespace EventPipe;

internal class StackResolver
{
    private static readonly MethodDescription UnresolvedMethodDescription = new("??", "", "", 0, 0);

    private readonly Dictionary<int, List<StackTraceGroup>> _groupedStackTraces = new();
    private readonly List<MethodDescription> _methodDescriptions = [];

    public void AddStackAddresses(int stackIndex, ReadOnlySpan<ulong> addresses)
    {
        int addressesHash = ComputeAddressesHash(addresses);
        if (!_groupedStackTraces.TryGetValue(addressesHash, out var collisions))
        {
            StackTraceGroup group = new()
            {
                StackIndexes = [stackIndex],
                Addresses = addresses.ToArray(),
                StackTrace = null,
            };
            _groupedStackTraces[addressesHash] = [group];
            return;
        }

        foreach (var collision in CollectionsMarshal.AsSpan(collisions))
        {
            if (addresses.SequenceEqual(collision.Addresses))
            {
                collision.StackIndexes.Add(stackIndex);
                return;
            }
        }

        collisions.Add(new StackTraceGroup
        {
            StackIndexes = [stackIndex],
            Addresses = addresses.ToArray(),
            StackTrace = null,
        });
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
            foreach (var collision in kvp.Value)
            {
                foreach (var stackIndex in collision.StackIndexes)
                {
                    stackIndexToStackTraceGroup[stackIndex] = collision;
                }
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

    private static int ComputeAddressesHash(ReadOnlySpan<ulong> addresses)
    {
        HashCode h = new();
        foreach (ulong addr in addresses)
        {
            h.Add(addr);
        }

        return h.ToHashCode();
    }
}