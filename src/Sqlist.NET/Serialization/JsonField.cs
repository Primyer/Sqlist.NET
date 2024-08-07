﻿using System.Text.Json;

namespace Sqlist.NET.Serialization;

/// <summary>
///     Initializes a new instance of the <see cref="JsonField"/> class.
/// </summary>
internal class JsonField(Type type) : SerializationField
{
    public Type Type { get; set; } = type;

    public override object? Parse(object obj)
    {
        if (obj is string str)
            return JsonSerializer.Deserialize(str, Type);

        throw new InvalidOperationException($"Invalid JSON string.");
    }
}