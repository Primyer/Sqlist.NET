namespace Sqlist.NET.Sql;
public class NpgsqlBuilderFactory : ISqlBuilderFactory
{
    public ISqlBuilder CasedSql()
    {
        return Sql(null);
    }

    public ISqlBuilder CasedSql(string? table)
    {
        return Sql(null, table);
    }

    public ISqlBuilder CasedSql(string? schema, string? table)
    {
        return Sql(null, schema, table);
    }

    public ISqlBuilder Sql()
    {
        return Sql(null);
    }

    public ISqlBuilder Sql(string? table)
    {
        return Sql(null, table);
    }

    public ISqlBuilder Sql(string? schema, string? table)
    {
        return Sql(new DummyEnclosure(), schema, table);
    }

    static NpgsqlBuilder Sql(Enclosure? encloser, string? schema, string? table)
    {
        return table is null
            ? new NpgsqlBuilder(encloser)
            : new NpgsqlBuilder(encloser, schema, table);
    }
}
