using System.Collections.Generic;

namespace Sqlist.NET.Migration.Deserialization;
public class DataTransferDefinition
{
    public Dictionary<string, string> Columns { get; set; } = new();
    public required string Script { get; set; }
}
