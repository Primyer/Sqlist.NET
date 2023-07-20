using System;

namespace Sqlist.NET.Migration.Deserialization;
public class DataTransferDefinition
{
    public string[] Columns { get; set; } = Array.Empty<string>();
    public required string Script { get; set; }
}
