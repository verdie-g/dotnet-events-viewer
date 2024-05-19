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

        ulong[] allMethodAddresses = new ulong[_methodDescriptions.Count];
        Dictionary<ulong, MethodDescription> allMethodDescriptions = new(_methodDescriptions.Count);
        for (int i = 0; i < _methodDescriptions.Count; i += 1)
        {
            var methodDescription = _methodDescriptions[i];
            allMethodAddresses[i] = methodDescription.Address;
            allMethodDescriptions[methodDescription.Address] = methodDescription;
        }

        Array.Sort(allMethodAddresses);

        foreach (var evt in events)
        {
            if (!stackIndexToStackTraceGroup.TryGetValue(evt.StackIndex, out var group))
            {
                evt.StackTrace = StackTrace.Empty;
                continue;
            }

            group.StackTrace ??= ResolveStackTrace(evt.StackIndex, group.Addresses,
                allMethodAddresses, allMethodDescriptions);

            evt.StackIndex = group.StackTrace.Index;
            evt.StackTrace = group.StackTrace;
        }
    }

    private static StackTrace ResolveStackTrace(
        int stackIndex,
        ulong[] addresses,
        ulong[] allMethodAddresses,
        Dictionary<ulong, MethodDescription> allMethodDescriptions)
    {
        if (addresses.Length == 0)
        {
            return StackTrace.Empty;
        }

        var stackTrace = new MethodDescription[addresses.Length];
        for (int i = 0; i < stackTrace.Length; i += 1)
        {
            stackTrace[i] = ResolveSymbol(addresses[i], allMethodAddresses, allMethodDescriptions);

        }

        return new StackTrace(stackIndex, stackTrace);
    }

    private static MethodDescription ResolveSymbol(
        ulong address,
        ulong[] allMethodAddresses,
        Dictionary<ulong, MethodDescription> allMethodDescriptions)
    {
        int methodAddressIdx = Array.BinarySearch(allMethodAddresses, address);
        if (methodAddressIdx < 0)
        {
            methodAddressIdx = ~methodAddressIdx - 1;
            if (methodAddressIdx == -1) // Input address is lower than the lowest method address.
            {
                return UnresolvedMethodDescription;
            }
        }

        var methodAddress = allMethodAddresses[methodAddressIdx];
        var methodDescription = allMethodDescriptions[methodAddress];

        if (methodDescription.Address + methodDescription.Size < address)
        {
            return UnresolvedMethodDescription;
        }

        return methodDescription;
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