namespace Sqlist.NET.Sql.Partial
{
    public readonly struct Reference
    {
        public Reference(string table, params string[] columns)
        {
            Table = table;
            Columns = columns;
        }

        public string? Table { get; }

        public string[] Columns { get; }
    }
}
