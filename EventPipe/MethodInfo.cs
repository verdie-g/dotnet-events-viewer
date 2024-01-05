namespace EventPipe;

public class MethodInfo
{
    public string Name { get; }
    public string Namespace { get; }
    public string Signature { get; }
    internal ulong Address { get; }

    internal MethodInfo(string name, string @namespace, string signature, ulong address)
    {
        Name = name;
        Namespace = @namespace;
        Signature = signature;
        Address = address;
    }
}