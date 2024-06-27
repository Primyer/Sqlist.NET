namespace Sqlist.NET;
public class DbTransactionException : DbException
{
    public DbTransactionException()
    {
    }

    public DbTransactionException(string message) : base(message)
    {
    }

    public DbTransactionException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public DbTransactionException(string message, int errorCode) : base(message, errorCode)
    {
    }
}