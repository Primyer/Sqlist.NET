using System;
using System.Collections.Generic;
using Sqlist.NET.Migration.Deserialization.Collections;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Sqlist.NET.Migration.Deserialization;
internal class ColumnsDefinitionNodeDeserializer : INodeDeserializer
{
    public bool Deserialize(IParser reader, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value)
    {
        if (expectedType != typeof(ColumnsDefinition))
        {
            value = null;
            return false;
        }

        var accepts = reader.Accept<MappingStart>(out _);
        value = accepts
            ? (DefinitionCollection)nestedObjectDeserializer(reader, typeof(DefinitionCollection))!
            : new()
            {
                Columns = (List<KeyValuePair<string, ColumnDefinition>>?)nestedObjectDeserializer(reader, typeof(IEnumerable<KeyValuePair<string, ColumnDefinition>>)) ?? []
            };

        return true;
    }
}
