namespace Sqlist.NET.Sql.Partial;
public readonly struct Reference(string table, params string[] columns)
{
    public string? Table { get; } = table;

    public string[] Columns { get; } = columns;
}