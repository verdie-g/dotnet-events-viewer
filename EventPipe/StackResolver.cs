namespace EventPipe;

internal class StackResolver
{
    private static readonly MethodDescription UnresolvedMethodDescription = new("??", "", "", 0, 0, 0);

    private readonly Dictionary<int, ulong[]> _stacksAddresses = [];
    private readonly Dictionary<ulong, MethodDescription> _methodSymbolInfosByMethodAddress = [];
    private readonly List<ulong> _methodAddresses = [];

    public void AddStackAddresses(int stackId, ulong[] addresses)
    {
        _stacksAddresses[stackId] = addresses;
    }

    public void AddMethodSymbolInfo(MethodDescription methodDescription)
    {
        _methodSymbolInfosByMethodAddress[methodDescription.Address] = methodDescription;
        _methodAddresses.Add(methodDescription.Address);
    }

    public Dictionary<int, StackTrace> ResolveAllStackTraces()
    {
        _methodAddresses.Sort();

        Dictionary<int, StackTrace> resolvedStackTraces = new(_stacksAddresses.Count);
        foreach (var kvp in _stacksAddresses)
        {
            resolvedStackTraces[kvp.Key] = ResolveStackTrace(kvp.Key, kvp.Value);
        }

        return resolvedStackTraces;
    }

    private StackTrace ResolveStackTrace(int stackId, ulong[] addresses)
    {
        if (addresses.Length == 0)
        {
            return StackTrace.Empty;
        }

        var stackTrace = new MethodDescription[addresses.Length];
        for (int i = 0; i < stackTrace.Length; i += 1)
        {
            stackTrace[i] = ResolveSymbol(addresses[i]);

        }

        return new StackTrace(stackId, stackTrace);
    }

    private MethodDescription ResolveSymbol(ulong address)
    {
        if (_methodAddresses.Count == 0)
        {
            return UnresolvedMethodDescription;
        }

        int methodAddressIdx = _methodAddresses.BinarySearch(address);
        if (methodAddressIdx < 0)
        {
            methodAddressIdx = ~methodAddressIdx - 1;
            if (methodAddressIdx == -1) // Input address is lower than the lowest method address.
            {
                return UnresolvedMethodDescription;
            }
        }

        var methodAddress = _methodAddresses[methodAddressIdx];
        var methodSymbolInfo = _methodSymbolInfosByMethodAddress[methodAddress];

        if (methodSymbolInfo.Address + methodSymbolInfo.Size < address)
        {
            return UnresolvedMethodDescription;
        }

        return methodSymbolInfo;
    }
}