using System.Text;

namespace NetTrace;

public sealed class StackTrace
{
    public static StackTrace Empty { get; } = new(-1, Array.Empty<MethodDescription>(), Array.Empty<ulong>());

    /// <summary>Uniquely identifies the <see cref="StackTrace"/> in a <see cref="Trace"/>.</summary>
    public int Index { get; }
    public MethodDescription[] Frames { get; }
    /// <summary>The original instruction pointers, in the same order as <see cref="Frames"/>.</summary>
    public ulong[] Addresses { get; }

    internal StackTrace(int index, MethodDescription[] frames, ulong[] addresses)
    {
        Index = index;
        Frames = frames;
        Addresses = addresses;
    }

    public override string ToString()
    {
        StringBuilder builder = new();
        foreach (var frame in Frames)
        {
            builder.Append(frame.Namespace);
            builder.Append('.');
            builder.AppendLine(frame.Name);
        }

        return builder.ToString();
    }
}