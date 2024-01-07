using System.Text;

namespace EventPipe;

public class StackTrace
{
    public static StackTrace Empty { get; } = new(-1, Array.Empty<MethodSymbolInfo>());

    public int Id { get; }
    public MethodSymbolInfo[] Frames { get; }

    internal StackTrace(int id, MethodSymbolInfo[] frames)
    {
        Id = id;
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