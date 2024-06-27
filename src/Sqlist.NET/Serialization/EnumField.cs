namespace Sqlist.NET.Serialization;

/// <summary>
///     Initializes a new instance of the <see cref="EnumField"/> class.
/// </summary>
internal class EnumField(Type type) : SerializationField
{
    public Type Type { get; } = type;

    public override object? Parse(object obj)
    {
        if (obj is string name)
            return Enumeration.FromDisplayName(Type, name);

        else if (obj is int value)
            return Enumeration.FromValue(Type, value);

        throw new InvalidOperationException($"Invalid Enumeration value: {obj}.");
    }
}
