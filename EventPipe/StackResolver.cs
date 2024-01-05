namespace EventPipe;

internal class StackResolver
{
    private readonly Dictionary<int, ulong[]> _stacksAddresses = [];
    private readonly List<MethodInfo> _methodInfos;

    public void AddStackAddresses(int stackId, ulong[] addresses)
    {
        _stacksAddresses[stackId] = addresses;
    }

    public void AddMethod(MethodInfo methodInfo)
    {
        // _methodInfos.Add(methodInfo);
    }
}