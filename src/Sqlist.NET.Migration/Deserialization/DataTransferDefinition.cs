using System.Collections.Generic;

namespace Sqlist.NET.Migration.Deserialization;
public class DataTransferDefinition
{
    public Dictionary<string, string> Columns { get; init; } = [];
    public required string Script { get; init; }
}
