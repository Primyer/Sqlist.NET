using System;

namespace Sqlist.NET
{
    public class DbConnectionException : DbException
    {
        public DbConnectionException()
        {
        }

        public DbConnectionException(string message) : base(message)
        {
        }

        public DbConnectionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public DbConnectionException(string message, int errorCode) : base(message, errorCode)
        {
        }
    }
}
