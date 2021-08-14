using System;
using System.Data.Common;
using System.Runtime.Serialization;

namespace Sqlist.NET
{
    public class DbTransactionException : DbException
    {
        public DbTransactionException()
        {
        }

        public DbTransactionException(string message) : base(message)
        {
        }

        public DbTransactionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public DbTransactionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public DbTransactionException(string message, int errorCode) : base(message, errorCode)
        {
        }
    }
}
