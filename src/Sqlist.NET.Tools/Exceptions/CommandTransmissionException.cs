namespace Sqlist.NET.Tools.Exceptions;
public class CommandTransmissionException : Exception
{
    public CommandTransmissionException()
    {
    }

    public CommandTransmissionException(string? message) : base(message)
    {
    }

    public CommandTransmissionException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
