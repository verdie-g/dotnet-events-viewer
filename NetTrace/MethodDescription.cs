namespace NetTrace;

public sealed class MethodDescription
{
    public string Name { get; }
    public string Namespace { get; }
    public string Signature { get; }
    public ulong Address { get; }
    public ulong Size { get; }

    public MethodDescription(string name, string @namespace)
        : this(name, @namespace, "", 0, 0)
    {
    }

    internal MethodDescription(string name, string @namespace, string signature, ulong address,
        uint size)
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
            try
            {
                return SymbolCleaner.Clean(Namespace, Name, Signature);
            }
            catch // It's hard to handle all cases so better have a fallback here.
            {
                return Namespace + "." + Name;
            }
        }

        return Name;
    }
}