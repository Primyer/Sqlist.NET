using System.Collections.Generic;

namespace Sqlist.NET.Migration.Deserialization.Collections
{
    public class DefinitionCollection : ColumnsDefinition
    {
    }

    public class ColumnsDefinition
    {
        public List<KeyValuePair<string, ColumnDefinition>> Columns { get; set; } = new();
        public string? Condition { get; set; }
        public string? Before { get; set; }
    }
}
