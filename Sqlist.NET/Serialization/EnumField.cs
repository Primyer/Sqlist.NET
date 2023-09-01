using Sqlist.NET.Metadata;

using System;

namespace Sqlist.NET.Serialization;
internal class EnumField : SerializationField
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="EnumField"/> class.
    /// </summary>
    public EnumField(Type type) => Type = type;

    public Type Type { get; }

    public override object? Parse(object obj)
    {
        if (obj is string name)
            return Enumeration.FromDisplayName(Type, name);

        else if (obj is int value)
            return Enumeration.FromValue(Type, value);

        throw new InvalidOperationException($"Invalid Enumeration value: {obj}.");
    }
}
