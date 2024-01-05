using System.Diagnostics.CodeAnalysis;

namespace EventPipe;

public class CorruptedBlockException : Exception
{
    internal static void ThrowIfFalse([DoesNotReturnIf(false)] bool b, long position)
    {
        if (!b)
        {
            throw new CorruptedBlockException("Unexpected end of block at position", position);
        }
    }

    private CorruptedBlockException(string message, long position)
        : base(message + " at position " + position)
    {
        Position = position;
    }

    public long Position { get; }
}