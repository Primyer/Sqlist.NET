using System.Data.Common;
using System.Data;

namespace Sqlist.NET;
public interface ICommandProvider
{
    ICommand CreateCommand();

    ICommand CreateCommand(DbConnection connection);

    ICommand CreateCommand(string sql, object? prms = null, int? timeout = null, CommandType? type = null);
}
