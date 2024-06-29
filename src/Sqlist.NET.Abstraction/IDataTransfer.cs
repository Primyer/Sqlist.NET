using Sqlist.NET.Data;
using System.Data.Common;

namespace Sqlist.NET;
public interface IDataTransfer
{
    DbConnection? Connection { get; }

    virtual Task CopyFromAsync(DbConnection source, string table, TransactionRuleDictionary rules, CancellationToken cancellationToken = default)
    {
        if (Connection is null)
            throw new DbConnectionException("Destination connection is not initialized.");

        return CopyAsync(source, Connection, table, rules, cancellationToken);
    }

    virtual Task CopyToAsync(DbConnection destination, string table, TransactionRuleDictionary rules, CancellationToken cancellationToken = default)
    {
        if (Connection is null)
            throw new DbConnectionException("Source connection is not initialized.");

        return CopyAsync(Connection, destination, table, rules, cancellationToken);
    }

    Task CopyAsync(DbConnection exporter, DbConnection importer, string table, TransactionRuleDictionary rules, CancellationToken cancellationToken = default);

    Task CopyFromAsync(DbDataReader reader, string table, ICollection<KeyValuePair<string, string>> columns, CancellationToken cancellationToken = default);
}
