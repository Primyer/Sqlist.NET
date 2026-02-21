using FirebirdSql.Data.FirebirdClient;
using Microsoft.Extensions.Options;
using System.Data.Common;

namespace Sqlist.NET.Infrastructure
{
    public class DbContext(IOptions<FirebirdOptions> options) : DbContextBase(options.Value)
    {
        /// <inheritdoc />
        public override FbConnection Connection => (FbConnection)base.Connection;

        /// <inheritdoc />
        public override FbTransaction? Transaction => base.Transaction as FbTransaction;

        public override FirebirdOptions Options => options.Value;

        public override TypeMapper TypeMapper => FirebirdTypeMapper.Default;

        public override string? DefaultDatabase => null;

        public override DbDataSource BuildDataSource(string? connectionString = null)
        {
            return new FirebirdDataSource(connectionString ?? Options.ConnectionString!);
        }

        public override string ChangeDatabase(string database)
        {
            throw new NotSupportedException("Changing database is not supported for Firebird connections.");
        }

        public override Task TerminateDatabaseConnectionsAsync(string database, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException("TerminateDatabaseConnections is not supported for Firebird provider.");
        }
    }
}
