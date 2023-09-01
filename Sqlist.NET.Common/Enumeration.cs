using System.Reflection;

namespace Sqlist.NET;
public abstract class Enumeration : IComparable
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Enumerable"/> class.
    /// </summary>
    protected Enumeration(int value, string displayName)
    {
        Value = value;
        DisplayName = displayName;
    }

    public int Value { get; }
    public string DisplayName { get; }

    public static bool operator >(Enumeration first, Enumeration other)
    {
        return first.Value > other.Value;
    }

    public static bool operator <(Enumeration first, Enumeration other)
    {
        return first.Value < other.Value;
    }

    public static bool operator >=(Enumeration first, Enumeration other)
    {
        return first.Value >= other.Value;
    }

    public static bool operator <=(Enumeration first, Enumeration other)
    {
        return first.Value <= other.Value;
    }

    public static bool operator ==(Enumeration first, Enumeration other)
    {
        return first.Value == other.Value;
    }

    public static bool operator !=(Enumeration first, Enumeration other)
    {
        return first.Value != other.Value;
    }

    public static IEnumerable<T> GetAll<T>() where T : Enumeration
    {
        var fields = typeof(T).GetFields(
            BindingFlags.Public
            | BindingFlags.Static
            | BindingFlags.DeclaredOnly);

        return fields.Select(f => f.GetValue(null)).Cast<T>();
    }

    public static IEnumerable<Enumeration> GetAll(Type type)
    {
        if (!type.IsSubclassOf(typeof(Enumeration)))
            throw new ArgumentException($"Type '{type.FullName}' is not an Enumeration type.");

        var fields = type.GetFields(
            BindingFlags.Public
            | BindingFlags.Static
            | BindingFlags.DeclaredOnly);

        return fields.Select(f => f.GetValue(null)).Cast<Enumeration>();
    }

    public static int AbsoluteDifference(Enumeration firstValue, Enumeration secondValue)
    {
        var absoluteDifference = Math.Abs(firstValue.Value - secondValue.Value);
        return absoluteDifference;
    }

    public static T FromValue<T>(int value) where T : Enumeration
    {
        return GetAll<T>().Single(u => u.Value == value);
    }

    public static Enumeration FromValue(Type type, int value)
    {
        return GetAll(type).Single(u => u.Value == value);
    }

    public static T FromDisplayName<T>(string displayName) where T : Enumeration
    {
        return GetAll<T>().Single(u => u.DisplayName == displayName);
    }

    public static Enumeration FromDisplayName(Type type, string displayName)
    {
        return GetAll(type).Single(u => u.DisplayName == displayName);
    }

    public int CompareTo(object? other)
    {
        return Value.CompareTo(((Enumeration?)other)?.Value);
    }

    public override string ToString() => DisplayName;

    public override bool Equals(object? obj)
    {
        if (obj is not Enumeration otherValue)
        {
            return false;
        }

        var typeMatches = GetType().Equals(obj.GetType());
        var valueMatches = Value.Equals(otherValue.Value);

        return typeMatches && valueMatches;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}
