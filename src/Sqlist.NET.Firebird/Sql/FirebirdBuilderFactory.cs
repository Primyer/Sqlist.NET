namespace Sqlist.NET.Sql;

public class FirebirdBuilderFactory : ISqlBuilderFactory
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
        return Sql(new FirebirdEnclosure(), schema, table);
    }

    static FirebirdBuilder Sql(Enclosure? encloser, string? schema, string? table)
    {
        return table is null
            ? new FirebirdBuilder(encloser)
            : new FirebirdBuilder(encloser, schema, table!);
    }
}
