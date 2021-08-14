using System;
using System.Data.Common;
using System.Runtime.Serialization;

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

        public DbConnectionException(SerializationInfo info, StreamingContext context) : base(info, context)
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
