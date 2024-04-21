using System.Text;

namespace EventPipe;

public class StackTrace
{
    public static StackTrace Empty { get; } = new(-1, Array.Empty<MethodDescription>());

    /// <summary>Uniquely identifies the <see cref="StackTrace"/> in a <see cref="Trace"/>.</summary>
    public int Index { get; }
    public MethodDescription[] Frames { get; }

    internal StackTrace(int index, MethodDescription[] frames)
    {
        Index = index;
        Frames = frames;
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