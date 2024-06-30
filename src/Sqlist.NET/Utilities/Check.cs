using System.Runtime.CompilerServices;

namespace Sqlist.NET.Utilities;
internal static class Check
{
    public static void Instantiable(Type type)
    {
        if (!type.IsClass || type.IsAbstract)
            throw new InvalidOperationException($"The type {type.Name} must be an instantiable class.");
    }

    public static void NotNull<T>(T? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (argument == null)
            throw new ArgumentNullException(paramName);
    }

    public static void NotNullOrEmpty(string? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (string.IsNullOrEmpty(argument))
            throw new ArgumentException($"The argument {paramName} can neither be null or empty.");
    }

    public static void NotEmpty<T>(IEnumerable<T> argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (!argument.Any())
            throw new ArgumentException($"The argument {paramName} cannot be empty.");
    }
}