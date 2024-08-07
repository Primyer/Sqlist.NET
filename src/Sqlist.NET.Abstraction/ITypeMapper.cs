﻿using System.Data;

namespace Sqlist.NET;
public interface ITypeMapper
{
    /// <summary>
    ///     Returns the name of the corresponding type of the DB provider.
    /// </summary>
    /// <typeparam name="T">The CLR type to match up.</typeparam>
    /// <returns>The name of the corresponding type of the DB provider.</returns>
    /// <exception cref="NotSupportedException" />
    string TypeName<T>();

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
