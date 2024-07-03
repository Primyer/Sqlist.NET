using Npgsql;

using System;

namespace Sqlist.NET.Infrastructure
{
    public class NpgsqlOptions : DbOptions
    {
        public Action<NpgsqlDataSourceBuilder>? ConfigureDataSource { get; set; }
    }
}
