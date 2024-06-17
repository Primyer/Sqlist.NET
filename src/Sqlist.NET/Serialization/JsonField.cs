using System;
using System.Text.Json;

namespace Sqlist.NET.Serialization
{
    internal class JsonField : SerializationField
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="JsonField"/> class.
        /// </summary>
        public JsonField(Type type) => Type = type;

        public Type Type { get; set; }

        public override object? Parse(object obj)
        {
            if (obj is string str)
                return JsonSerializer.Deserialize(str, Type);

            throw new InvalidOperationException($"Invalid JSON string.");
        }
    }
}
