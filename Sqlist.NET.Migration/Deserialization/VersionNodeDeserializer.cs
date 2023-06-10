using System;

using YamlDotNet.Core;
using YamlDotNet.Serialization;

using Version = System.Version;

namespace Sqlist.NET.Migration.Deserialization
{
    internal class VersionNodeDeserializer : INodeDeserializer
    {
        public bool Deserialize(IParser parser, Type expectedType, Func<IParser, Type, object?> nestedObjectDeserializer, out object? value)
        {
            if (expectedType == typeof(Version))
            {
                try
                {
                    var version = (string?)nestedObjectDeserializer(parser, typeof(string));
                    value = version is null ? null : new Version(version);

                    return true;
                }
                catch (Exception ex)
                {
                    throw new YamlException("Invalid version.", ex);
                }
            }

            value = null;
            return false;
        }
    }
}
