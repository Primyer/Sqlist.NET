namespace Sqlist.NET.Sql;

public class FirebirdBuilder : SqlBuilder
{
    public FirebirdBuilder() : base(new FirebirdEnclosure())
    {
    }

    public FirebirdBuilder(Enclosure? encloser) : base(encloser)
    {
    }

    public FirebirdBuilder(string table) : base(new FirebirdEnclosure(), table)
    {
    }

    public FirebirdBuilder(string? schema, string table) : base(new FirebirdEnclosure(), schema, table)
    {
    }

    public FirebirdBuilder(Enclosure? encloser, string? schema, string table) : base(encloser ?? new FirebirdEnclosure(), schema, table)
    {
    }
}
