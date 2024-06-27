using System;

namespace Sqlist.NET
{
    public class DbException : System.Data.Common.DbException
    {
        public DbException()
        {
        }

        public DbException(string message) : base(message)
        {
        }

        public DbException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public DbException(string message, int errorCode) : base(message, errorCode)
        {
        }
    }
}
