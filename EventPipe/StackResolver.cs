namespace EventPipe;

internal class StackResolver
{
	private static readonly MethodSymbolInfo UnresolvedMethodSymbolInfo = new("??", "??", "??", 0, 0);

    private readonly Dictionary<int, ulong[]> _stacksAddresses = [];
    private readonly Dictionary<ulong, MethodSymbolInfo> _methodSymbolInfosByMethodAddress = [];
    private readonly List<ulong> _methodAddresses = [];
    private bool _methodAddressesSorted;

    public void AddStackAddresses(int stackId, ulong[] addresses)
    {
        _stacksAddresses[stackId] = addresses;
    }

    public void AddMethodSymbolInfo(MethodSymbolInfo methodSymbolInfo)
    {
        _methodSymbolInfosByMethodAddress[methodSymbolInfo.Address] = methodSymbolInfo;
        _methodAddresses.Add(methodSymbolInfo.Address);
        _methodAddressesSorted = false;
    }

    public StackTrace ResolveStackTrace(int stackId)
    {
	    if (!_stacksAddresses.TryGetValue(stackId, out ulong[]? addresses))
	    {
		    return StackTrace.Empty;
	    }

	    if (addresses.Length == 0)
	    {
		    return StackTrace.Empty;
	    }

	    if (!_methodAddressesSorted)
	    {
		    _methodAddressesSorted = true;
		    _methodAddresses.Sort();
	    }

	    var stackTrace = new MethodSymbolInfo[addresses.Length];
	    for (int i = 0; i < stackTrace.Length; i += 1)
	    {
		    stackTrace[i] = ResolveSymbol(addresses[i]);
	    }

	    return new StackTrace(stackId, stackTrace);
    }

    private MethodSymbolInfo ResolveSymbol(ulong address)
    {
	    if (_methodAddresses.Count == 0)
	    {
		    return UnresolvedMethodSymbolInfo;
	    }

	    int methodAddressIdx = _methodAddresses.BinarySearch(address);
	    methodAddressIdx = methodAddressIdx >= 0 ? methodAddressIdx : Math.Max(~methodAddressIdx - 1, 0);

	    var methodAddress = _methodAddresses[methodAddressIdx];
	    var methodSymbolInfo = _methodSymbolInfosByMethodAddress[methodAddress];

	    if (methodSymbolInfo.Address + methodSymbolInfo.Size < address)
	    {
		    return UnresolvedMethodSymbolInfo;
	    }

	    return methodSymbolInfo;
    }
}