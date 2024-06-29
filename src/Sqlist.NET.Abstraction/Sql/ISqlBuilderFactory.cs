namespace Sqlist.NET.Sql;
public interface ISqlBuilderFactory
{
    /// <summary>
    ///     Invokes and returns the a new case-sensitive <see cref="ISqlBuilder"/> instance.
    /// </summary>
    /// <returns>The <see cref="ISqlBuilder"/>.</returns>
    public ISqlBuilder CasedSql();

    /// <summary>
    ///     Invokes and returns the a new case-sensitive <see cref="ISqlBuilder"/> instance.
    /// </summary>
    /// <param name="table">The table that's referenced in the statement.</param>
    /// <returns>The <see cref="ISqlBuilder"/>.</returns>
    public ISqlBuilder CasedSql(string? table);

    /// <summary>
    ///     Invokes and returns the a new case-sensitive <see cref="ISqlBuilder"/> instance.
    /// </summary>
    /// <param name="schema">The schema where the sql statement is to be executed.</param>
    /// <param name="table">The table that's referenced in the statement.</param>
    /// <returns>The <see cref="ISqlBuilder"/>.</returns>
    public ISqlBuilder CasedSql(string? schema, string? table);

    /// <summary>
    ///     Invokes and returns the a new <see cref="ISqlBuilder"/> instance.
    /// </summary>
    /// <returns>The <see cref="ISqlBuilder"/>.</returns>
    public ISqlBuilder Sql();

    /// <summary>
    ///     Invokes and returns the a new <see cref="ISqlBuilder"/> instance.
    /// </summary>
    /// <param name="table">The table that's referenced in the statement.</param>
    /// <returns>The <see cref="ISqlBuilder"/>.</returns>
    public ISqlBuilder Sql(string? table);

    /// <summary>
    ///     Invokes and returns the a new <see cref="ISqlBuilder"/> instance.
    /// </summary>
    /// <param name="schema">The schema where the sql statement is to be executed.</param>
    /// <param name="table">The table that's referenced in the statement.</param>
    /// <returns>The <see cref="ISqlBuilder"/>.</returns>
    public ISqlBuilder Sql(string? schema, string? table);
}
