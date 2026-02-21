using FirebirdSql.Data.FirebirdClient;
using System.Data.Common;

namespace Sqlist.NET.Infrastructure
{
    internal sealed class FirebirdDataSource : DbDataSource
    {
        private readonly string _connectionString;

        public FirebirdDataSource(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public override string ConnectionString => _connectionString;

        protected override DbConnection CreateDbConnection()
        {
            return new FbConnection(_connectionString);
        }
    }
}
