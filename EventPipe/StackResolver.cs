namespace EventPipe;

internal class StackResolver
{
    private static readonly MethodDescription UnresolvedMethodDescription = new("??", "", "", 0, 0);

    private readonly Dictionary<int, ulong[]> _stacksAddresses = [];
    private readonly List<MethodDescription> _methodDescriptions = [];

    public void AddStackAddresses(int stackId, ulong[] addresses)
    {
        _stacksAddresses[stackId] = addresses;
    }

    public void AddMethodSymbolInfo(MethodDescription methodDescription)
    {
        _methodDescriptions.Add(methodDescription);
    }

    public Dictionary<int, StackTrace> ResolveAllStackTraces()
    {
        ulong[] methodAddresses = new ulong[_methodDescriptions.Count];
        Dictionary<ulong, MethodDescription> methodDescriptions = new(_methodDescriptions.Count);
        for (int i = 0; i < _methodDescriptions.Count; i += 1)
        {
            var methodDescription = _methodDescriptions[i];
            methodAddresses[i] = methodDescription.Address;
            methodDescriptions[methodDescription.Address] = methodDescription;
        }

        Array.Sort(methodAddresses);

        Dictionary<int, StackTrace> resolvedStackTraces = new(_stacksAddresses.Count);
        foreach (var kvp in _stacksAddresses)
        {
            resolvedStackTraces[kvp.Key] = ResolveStackTrace(kvp.Key, kvp.Value, methodAddresses, methodDescriptions);
        }

        return resolvedStackTraces;
    }

    private static StackTrace ResolveStackTrace(
        int stackId,
        ulong[] addresses,
        ulong[] methodAddresses,
        Dictionary<ulong, MethodDescription> methodDescriptions)
    {
        if (addresses.Length == 0)
        {
            return StackTrace.Empty;
        }

        var stackTrace = new MethodDescription[addresses.Length];
        for (int i = 0; i < stackTrace.Length; i += 1)
        {
            stackTrace[i] = ResolveSymbol(addresses[i], methodAddresses, methodDescriptions);

        }

        return new StackTrace(stackId, stackTrace);
    }

    private static MethodDescription ResolveSymbol(
        ulong address,
        ulong[] methodAddresses,
        Dictionary<ulong, MethodDescription> methodDescriptions)
    {
        int methodAddressIdx = Array.BinarySearch(methodAddresses, address);
        if (methodAddressIdx < 0)
        {
            methodAddressIdx = ~methodAddressIdx - 1;
            if (methodAddressIdx == -1) // Input address is lower than the lowest method address.
            {
                return UnresolvedMethodDescription;
            }
        }

        var methodAddress = methodAddresses[methodAddressIdx];
        var methodDescription = methodDescriptions[methodAddress];

        if (methodDescription.Address + methodDescription.Size < address)
        {
            return UnresolvedMethodDescription;
        }

        return methodDescription;
    }
}