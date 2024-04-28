namespace EventPipe;

public class InvalidNetTraceFileException : Exception
{
    internal InvalidNetTraceFileException(string message) : base(message)
    {
    }
}