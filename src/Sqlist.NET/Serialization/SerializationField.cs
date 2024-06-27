namespace Sqlist.NET.Serialization;

internal class SerializationField
{
    public string? Name { get; set; }

    public virtual object? Parse(object obj) => obj;
}