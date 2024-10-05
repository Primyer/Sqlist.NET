using System.Data;

namespace Sqlist.NET;
public interface ITypeMapper
{
    /// <summary>
    ///     Returns the name of the corresponding type of the DB provider.
    /// </summary>
    /// <typeparam name="T">The CLR type to match up.</typeparam>
    /// <param name="precision">
    /// The total count of significant digits in the whole number, that is, the number of digits to both sides
    /// of the decimal point.
    /// </param>
    /// <param name="scale">The number of decimal digits in the fractional part, to the right of the decimal point.</param>
    /// <returns>The name of the corresponding type of the DB provider.</returns>
    /// <exception cref="NotSupportedException">
    /// Thrown when <typeparamref name="T"/> has no corresponding type in the target DB provider.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the <paramref name="scale"/> is specified while the <paramref name="precision"/> is not.
    /// </exception>
    string TypeName<T>(uint? precision = null, int? scale = null);

    /// <summary>
    ///     Returns the name of the corresponding type of the DB provider.
    /// </summary>
    /// <param name="name">The name of the DB provider type.</param>
    /// <returns>The name of the corresponding type of the DB provider.</returns>
    /// <exception cref="NotSupportedException" />
    DbType GetDbType(string name);

    /// <summary>
    ///     Returns the corresponding type of the DB provider.
    /// </summary>
    /// <param name="type">The CLR type to match up.</param>
    /// <returns>The corresponding type of the DB provider</returns>
    DbType ToDbType(Type type);

    /// <summary>
    ///     Returns the corresponding CLR type.
    /// </summary>
    /// <param name="type">The <see cref="DbType"/> to match up.</param>
    /// <returns>The corresponding CLR type.</returns>
    Type FromDbType(DbType type);

    /// <summary>
    ///     Returns the corresponding CLR type.
    /// </summary>
    /// <param name="name">The name of the DB provider type.</param>
    /// <returns>The corresponding CLR type.</returns>
    /// <exception cref="NotSupportedException" />
    Type GetType(string name);

    /// <summary>
    ///     Returns the name of the corresponding type of the DB provider.
    /// </summary>
    /// <param name="dbType">The ADO.NET <see cref="DbType"/> to match up.</param>
    /// <returns>The name of the corresponding type of the DB provider.</returns>
    /// <exception cref="NotSupportedException" />
    string TypeName(DbType dbType);
}
