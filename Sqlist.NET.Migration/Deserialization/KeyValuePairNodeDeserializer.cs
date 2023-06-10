using System;
using System.Collections.Generic;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Sqlist.NET.Migration.Deserialization
{
    internal class KeyValuePairNodeDeserializer : INodeDeserializer
    {
        public bool Deserialize(IParser parser, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value)
        {
            if (expectedType.IsGenericType && expectedType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
            {
                parser.Consume<MappingStart>();

                var pairArgs = expectedType.GetGenericArguments();

                object? key = null;
                object? val = null;

                if (parser.Accept<Scalar>(out _))
                    key = nestedObjectDeserializer(parser, pairArgs[0]);

                if (parser.Accept<Scalar>(out _))
                    val = nestedObjectDeserializer(parser, pairArgs[1]);

                value = Activator.CreateInstance(expectedType, key, val);

                parser.Consume<MappingEnd>();
                return true;
            }

            value = null;
            return false;
        }
    }
}
