using System.Diagnostics;

namespace EventPipe;

public class MethodSymbolInfo
{
    public string Name { get; }
    public string Namespace { get; }
    public string Signature { get; }
    public ulong Address { get; }
    public ulong Size { get; }

    internal MethodSymbolInfo(string name, string @namespace, string signature, ulong address, uint size)
    {
        Name = name;
        Namespace = @namespace;
        Signature = signature;
        Address = address;
        Size = size;
    }

    public override string ToString()
    {
        if (Namespace.Length != 0 && Name.Length != 0)
        {
            return Namespace + '.' + Name;
        }

        return Name;
    }
}