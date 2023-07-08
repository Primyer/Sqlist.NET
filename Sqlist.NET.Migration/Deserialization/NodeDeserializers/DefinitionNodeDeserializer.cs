using System;
using System.Collections.Generic;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Sqlist.NET.Migration.Deserialization
{
    internal class DefinitionNodeDeserializer : INodeDeserializer
    {
        public bool Deserialize(IParser parser, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value)
        {
            if (expectedType.IsGenericType && expectedType == typeof(KeyValuePair<string, ColumnDefinition>))
            {
                parser.Consume<MappingStart>();

                var key = (string)nestedObjectDeserializer(parser, typeof(string))!;
                ColumnDefinition val;

                if (!parser.Accept<Scalar>(out _))
                    val = (ColumnDefinition)nestedObjectDeserializer(parser, typeof(ColumnDefinition))!;
                else
                {
                    var type = (string)nestedObjectDeserializer(parser, typeof(string))!;
                    val = new ColumnDefinition(type!);
                }

                value = KeyValuePair.Create(key, val);

                parser.Consume<MappingEnd>();
                return true;
            }

            value = null;
            return false;
        }
    }
}
