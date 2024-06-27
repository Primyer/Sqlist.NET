using Sqlist.NET.Sql.Constraints;
using Sqlist.NET.Sql.Metadata;
using Sqlist.NET.Utilities;

using System.Text;

namespace Sqlist.NET.Sql;

/// <summary>
///     Provides the API to automatically generate SQL statements.
/// </summary>
/// <remarks>
///     Initalizes a new instance of the <see cref="SqlBuilder"/> class.
/// </remarks>
/// <param name="encloser">The appopertiate <see cref="Sql.Encloser"/> implementation according to the target DBMS.</param>
public class SqlBuilder(Encloser? encloser)
{
    private const string Tab = "    ";
    private string? _tableName;

    /// <summary>
    ///     Initalizes a new instance of the <see cref="SqlBuilder"/> class.
    /// </summary>
    public SqlBuilder() : this(Encloser.Default)
    { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="SqlBuilder"/> class.
    /// </summary>
    /// <param name="encloser">The appopertiate <see cref="Sql.Encloser"/> implementation according to the target DBMS.</param>
    /// <param name="schema">The name of the schema where the table exists.</param>
    /// <param name="table">The name of table to base the statement on.</param>
    public SqlBuilder(Encloser? encloser, string? schema, string table) : this(encloser, table)
    {
        if (!string.IsNullOrEmpty(schema))
            TableName = Encloser.Replace(schema) + "." + TableName;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="SqlBuilder"/> class.
    /// </summary>
    /// <param name="encloser">The appopertiate <see cref="Sql.Encloser"/> implementation according to the target DBMS.</param>
    /// <param name="table">The name of table to base the statement on.</param>
    public SqlBuilder(Encloser? encloser, string table) : this(encloser)
    {
        TableName = table ?? throw new ArgumentNullException(nameof(table));
    }

    protected Dictionary<string, StringBuilder> Builders { get; } = [];

    public Encloser Encloser { get; set; } = encloser ?? new DummyEncloser();

    /// <summary>
    ///     Gets or sets the name of the table name that the to-be-generated statement should base on.
    /// </summary>
    public string? TableName
    {
        get => _tableName;
        set => _tableName = Encloser.Reformat(value);
    }

    /// <summary>
    ///     Registers the specified <paramref name="fields"/> in pairs similar to <c>field = @field</c>.
    /// </summary>
    /// <param name="fields">The field to pair up and register.</param>
    public virtual void RegisterPairFields(string[] fields)
    {
        var builder = GetOrCreateBuilder("pairs");
        if (builder.Length != 0)
            builder.Append($",\n{Tab}");

        for (var i = 0; i < fields.Length; i++)
        {
            builder.Append(Encloser.Reformat(fields[i]));
            builder.Append(" = ");
            builder.Append('@' + fields[i]);

            if (i != fields.Length - 1)
                builder.Append($",\n{Tab}");
        }
    }

    /// <summary>
    ///     Registers the specified <paramref name="pair"/>.
    /// </summary>
    /// <param name="pair">The pair to register.</param>
    public virtual void RegisterPairFields((string, string) pair)
    {
        var builder = GetOrCreateBuilder("pairs");
        if (builder.Length != 0)
            builder.Append($",\n{Tab}");

        builder.Append(Encloser.Reformat(pair.Item1));
        builder.Append(" = ");
        builder.Append(Encloser.Replace(pair.Item2));
    }

    /// <summary>
    ///     Registers the specified <paramref name="pairs"/>.
    /// </summary>
    /// <param name="pairs">The of collection of pairs to register.</param>
    public virtual void RegisterPairFields(Dictionary<string, string> pairs)
    {
        var builder = GetOrCreateBuilder("pairs");
        if (builder.Length != 0)
            builder.Append($",\n{Tab}");

        var count = 0;
        foreach (var (key, value) in pairs)
        {
            builder.Append(Encloser.Reformat(key));
            builder.Append(" = ");
            builder.Append(Encloser.Replace(value));

            if (count != pairs.Count - 1)
                builder.Append($",\n{Tab}");

            count++;
        }
    }

    /// <summary>
    ///     Registers the specified <paramref name="entity"/> as a <c>FROM</c> clause for the update statement.
    /// </summary>
    /// <param name="entity">The entity to register.</param>
    public virtual void From(string entity)
    {
        var builder = GetOrCreateBuilder("from");
        builder.AppendLine();
        builder.Append("FROM ");
        builder.Append(Encloser.Reformat(entity));
    }

    /// <summary>
    ///     Registers the specified <paramref name="row"/> as values.
    /// </summary>
    /// <param name="row">The row to register.</param>
    public virtual void RegisterValues(object[] row)
    {
        RegisterValues([row]);
    }

    /// <summary>
    ///     Registers the specified collection of <paramref name="rows"/> as values.
    /// </summary>
    /// <param name="rows">The collection of rows to register.</param>
    public virtual void RegisterValues(object[][] rows)
    {
        var builder = GetOrCreateBuilder("values");
        if (builder.Length != 0)
            builder.Append($",\n{Tab}");

        for (var i = 0; i < rows.Length; i++)
        {
            builder.Append('(');

            for (var j = 0; j < rows[i].Length; j++)
            {
                builder.Append(rows[i][j].ToString());

                if (j != rows[i].Length - 1)
                    builder.Append(", ");
            }

            builder.Append(')');

            if (i != rows.Length - 1)
                builder.Append($",\n{Tab}");
        }
    }

    /// <summary>
    ///     Registers (@p[n]) formatted paramters according to the spacified <paramref name="cols"/> and <paramref name="rows"/>.
    /// </summary>
    /// <param name="cols">The number of columns in each row.</param>
    /// <param name="rows">The number of rows.</param>
    /// <param name="configure">Optional lambda for customizing the rows.</param>
    public virtual void RegisterBulkValues(int cols, int rows, int gapSize = 0, Action<int, string[]>? configure = null)
    {
        var obj = new string[rows][];

        for (var i = 0; i < rows; i++)
        {
            obj[i] = new string[cols + gapSize];

            for (var j = 0; j < cols; j++)
                obj[i][j] = "@p" + (j + i * cols);

            for (var j = gapSize + cols - 1; j >= cols; j--)
                obj[i][j] = "";

            configure?.Invoke(i, obj[i]);
        }

        RegisterValues(obj);
    }

    /// <summary>
    ///     Registers an <c>INNER JOIN</c> on the specified <paramref name="table"/> with the given <paramref name="condition"/>.
    /// </summary>
    /// <param name="table">The table to join.</param>
    /// <param name="condition">The join conditions.</param>
    public virtual void InnerJoin(string table, string? condition = null)
    {
        Join("INNER", table, condition);
    }

    /// <summary>
    ///     Registers an <c>INNER JOIN</c> on the specified <paramref name="table"/> with the given <paramref name="condition"/>.
    /// </summary>
    /// <param name="table">The table to join.</param>
    /// <param name="condition">The join conditions.</param>
    public virtual void InnerJoin(string table, Action<ConditionalClause> condition)
    {
        Join("INNER", table, condition);
    }

    /// <summary>
    ///     Registers an <c>INNER JOIN</c> of the specified on the result of the <paramref name="configureSql"/>
    ///     with respect to the given <paramref name="condition"/>.
    /// </summary>
    /// <param name="alias">The alias of the subquery.</param>
    /// <param name="configureSql">The action to build the partial statement of the join.</param>
    /// <param name="condition">The condition of join operation.</param>
    public virtual void InnerJoin(string alias, string condition, Action<SqlBuilder> configureSql)
    {
        Join("INNER", alias, condition, configureSql);
    }

    /// <summary>
    ///     Registers a <c>LEFT JOIN</c> on the specified <paramref name="table"/> with the given <paramref name="condition"/>.
    /// </summary>
    /// <param name="table">The table to join.</param>
    /// <param name="condition">The join conditions.</param>
    public virtual void LeftJoin(string table, string? condition = null)
    {
        Join("LEFT", table, condition);
    }

    /// <summary>
    ///     Registers a <c>LEFT JOIN</c> on the specified <paramref name="table"/> with the given <paramref name="condition"/>.
    /// </summary>
    /// <param name="table">The table to join.</param>
    /// <param name="condition">The join conditions.</param>
    public virtual void LeftJoin(string table, Action<ConditionalClause> condition)
    {
        Join("LEFT", table, condition);
    }

    /// <summary>
    ///     Registers an <c>LEFT JOIN</c> of the specified on the result of the <paramref name="configureSql"/>
    ///     with respect to the given <paramref name="condition"/>.
    /// </summary>
    /// <param name="alias">The alias of the subquery.</param>
    /// <param name="configureSql">The action to build the partial statement of the join.</param>
    /// <param name="condition">The condition of join operation.</param>
    public virtual void LeftJoin(string alias, string condition, Action<SqlBuilder> configureSql)
    {
        Join("LEFT", alias, condition, configureSql);
    }

    /// <summary>
    ///     Registers a <c>RIGHT JOIN</c> on the specified <paramref name="table"/> with the given <paramref name="condition"/>.
    /// </summary>
    /// <param name="table">The table to join.</param>
    /// <param name="condition">The join conditions.</param>
    public virtual void RightJoin(string table, string? condition = null)
    {
        Join("RIGHT", table, condition);
    }

    /// <summary>
    ///     Registers a <c>RIGHT JOIN</c> on the specified <paramref name="table"/> with the given <paramref name="condition"/>.
    /// </summary>
    /// <param name="table">The table to join.</param>
    /// <param name="condition">The join conditions.</param>
    public virtual void RightJoin(string table, Action<ConditionalClause> condition)
    {
        Join("RIGHT", table, condition);
    }

    /// <summary>
    ///     Registers an <c>RIGHT JOIN</c> of the specified on the result of the <paramref name="configureSql"/>
    ///     with respect to the given <paramref name="condition"/>.
    /// </summary>
    /// <param name="alias">The alias of the subquery.</param>
    /// <param name="configureSql">The action to build the partial statement of the join.</param>
    /// <param name="condition">The condition of join operation.</param>
    public virtual void RightJoin(string alias, string condition, Action<SqlBuilder> configureSql)
    {
        Join("RIGHT", alias, condition, configureSql);
    }

    /// <summary>
    ///     Registers a <c>FULL JOIN</c> on the specified <paramref name="table"/> with the given <paramref name="condition"/>.
    /// </summary>
    /// <param name="table">The table to join.</param>
    /// <param name="condition">The join conditions.</param>
    public virtual void FullJoin(string table, string? condition = null)
    {
        Join("FULL", table, condition);
    }

    /// <summary>
    ///     Registers an <c>FULL JOIN</c> of the specified on the result of the <paramref name="configureSql"/>
    ///     with respect to the given <paramref name="condition"/>.
    /// </summary>
    /// <param name="alias">The alias of the subquery.</param>
    /// <param name="configureSql">The action to build the partial statement of the join.</param>
    /// <param name="condition">The condition of join operation.</param>
    public virtual void FullJoin(string alias, string condition, Action<SqlBuilder> configureSql)
    {
        Join("FULL", alias, condition, configureSql);
    }

    /// <summary>
    ///     Registers a join of the specified <paramref name="type"/> on the result of
    ///     the <paramref name="configureSql"/> with respect to the given <paramref name="condition"/>.
    /// </summary>
    /// <param name="type">The type of join being registered.</param>
    /// <param name="alias">The alias of the subquery.</param>
    /// <param name="configureSql">The action to build the partial statement of the join.</param>
    /// <param name="condition">The condition of join operation.</param>
    public virtual void Join(string type, string alias, string condition, Action<SqlBuilder> configureSql)
    {
        var sql = new SqlBuilder(Encloser);
        configureSql.Invoke(sql);

        var stmt = Indent(sql.ToSelect());

        Join(type, $"(\n{stmt}) AS {alias}", condition);
    }

    /// <summary>
    ///     Registers a join of the specified <paramref name="type"/> with the given <paramref name="entity"/> and <paramref name="condition"/>.
    /// </summary>
    /// <param name="type">The type of join being registered.</param>
    /// <param name="entity">The entity which to join with.</param>
    /// <param name="condition">The condition of join operation.</param>
    public virtual void Join(string type, string entity, string? condition = null)
    {
        Join($"{type} JOIN {Encloser.Reformat(entity)} ON " + (condition is null ? "true" : Encloser.Reformat(condition)));
    }

    /// <summary>
    ///     Registers a join of the specified <paramref name="type"/> with the given <paramref name="entity"/> and <paramref name="condition"/>.
    /// </summary>
    /// <param name="type">The type of join being registered.</param>
    /// <param name="entity">The entity which to join with.</param>
    /// <param name="condition">The condition of join operation.</param>
    public virtual void Join(string type, string entity, Action<ConditionalClause> condition)
    {
        var clause = new ConditionalClause();
        condition.Invoke(clause);

        Join($"{type} JOIN {Encloser.Reformat(entity)} ON " + Encloser.Reformat(clause.ToString()));
    }

    /// <summary>
    ///     Register the specified partial <paramref name="stmt"/> as a join.
    /// </summary>
    /// <param name="stmt">The statement to register.</param>
    public virtual void Join(string stmt)
    {
        Check.NotNullOrEmpty(stmt);

        GetOrCreateBuilder("joins").Append('\n' + stmt);
    }

    /// <summary>
    ///     Registers the <c>WHERE</c> statement with the given <paramref name="condition"/>.
    /// </summary>
    /// <param name="condition">The condition to register.</param>
    public virtual void Where(string condition)
    {
        Check.NotNullOrEmpty(condition);

        if (Builders.ContainsKey("where"))
        {
            AppendAnd(condition);
            return;
        }

        var builder = Builders["where"] = new StringBuilder();

        builder.Append("\nWHERE ");
        builder.Append(Encloser.Replace(condition));
    }

    /// <summary>
    ///     Registers the <c>WHERE</c> statement with the given <paramref name="condition"/>.
    /// </summary>
    /// <param name="condition">The condition to register.</param>
    public virtual void Where(Action<ConditionalClause> condition)
    {
        var clause = new ConditionalClause();
        condition.Invoke(clause);

        Where(clause.ToString());
    }

    /// <summary>
    ///     Appends an <c>OR</c> followed by the specified <paramref name="condition"/> to the <c>WHERE</c> statement.
    /// </summary>
    /// <param name="condition">The condition to append.</param>
    public virtual void AppendOr(string condition)
    {
        AppendCondition("OR " + condition);
    }

    /// <summary>
    ///     Appends an <c>OR NOT</c> followed by the specified <paramref name="condition"/> to the <c>WHERE</c> statement.
    /// </summary>
    /// <param name="condition">The condition to append.</param>
    public virtual void AppendOrNot(string condition)
    {
        AppendCondition("OR NOT " + condition);
    }

    /// <summary>
    ///     Appends an <c>AND</c> followed by the specified <paramref name="condition"/> to the <c>WHERE</c> statement.
    /// </summary>
    /// <param name="condition">The condition to append.</param>
    public virtual void AppendAnd(string condition)
    {
        AppendCondition("AND " + condition);
    }

    /// <summary>
    ///     Appends an <c>AND NOT</c> followed by the specified <paramref name="condition"/> to the <c>WHERE</c> statement.
    /// </summary>
    /// <param name="condition">The condition to append.</param>
    public virtual void AppendAndNot(string condition)
    {
        AppendCondition("AND NOT " + condition);
    }

    /// <summary>
    ///     Appends the given condition to the <c>WHERE</c> statement.
    /// </summary>
    /// <param name="condition">The condition to append.</param>
    public virtual void AppendCondition(string condition)
    {
        Check.NotNullOrEmpty(condition);

        if (!Builders.TryGetValue("where", out var builder))
            throw new InvalidOperationException("The 'where' condition wasn't initialized.");

        builder.Append(" " + Encloser.Reformat(condition));
    }

    /// <summary>
    ///     Registers the specified <paramref name="field"/>.
    /// </summary>
    /// <param name="field">The field to register.</param>
    /// <param name="reformat">Indicates whether to re-format.</param>
    public virtual void RegisterFields(string field, bool reformat = true)
    {
        Check.NotNullOrEmpty(field);

        var builder = GetOrCreateBuilder("fields");
        if (builder.Length != 0)
            builder.Append(", ");

        builder.Append(reformat ? Encloser.Reformat(field) : field);
    }

    /// <summary>
    ///     Registers the specified collection of <paramref name="fields"/>.
    /// </summary>
    /// <param name="fields">The fields to register.</param>
    public virtual void RegisterFields(string[] fields)
    {
        Check.NotEmpty(fields);

        var builder = GetOrCreateBuilder("fields");
        if (builder.Length != 0)
            builder.Append(", ");

        for (var i = 0; i < fields.Length; i++)
        {
            builder.Append(Encloser.Reformat(fields[i]));

            if (i != fields.Length - 1)
                builder.Append(", ");
        }
    }

    /// <summary>
    ///     Clears the existing fields.
    /// </summary>
    public virtual void ClearFields()
    {
        Builders.Remove("fields");
    }

    /// <summary>
    ///     Registers a <c>WINDOW</c> statement with the specified <paramref name="alias"/>.
    /// </summary>
    /// <param name="alias">The alias of the statement.</param>
    /// <param name="content">The conetnt of the statement.</param>
    public virtual void Window(string alias, string content)
    {
        Check.NotNullOrEmpty(alias);
        Check.NotNullOrEmpty(content);

        var builder = GetOrCreateBuilder("filters");

        builder.AppendLine();
        builder.Append($"WINDOW {Encloser.Replace(alias)} AS ({Encloser.Replace(content)})");
    }

    /// <summary>
    ///     Registers a <c>GROUP BY</c> statement with the specified <paramref name="field"/>.
    /// </summary>
    /// <param name="field">The field to group by.</param>
    public virtual void GroupBy(string field)
    {
        Check.NotNullOrEmpty(field);

        var builder = GetOrCreateBuilder("filters");

        builder.Append("\nGROUP BY ");
        builder.Append(Encloser.Reformat(field));
    }

    /// <summary>
    ///     Registers a <c>GROUP BY</c> statement with the specified collection of <paramref name="fields"/>.
    /// </summary>
    /// <param name="fields">The fields to group by.</param>
    public virtual void GroupBy(string[] fields)
    {
        Check.NotEmpty(fields);

        var result = string.Empty;
        for (var i = 0; i < fields.Length; i++)
        {
            result += Encloser.Wrap(fields[i]);

            if (i != fields.Length - 1)
                result += ", ";
        }

        var builder = GetOrCreateBuilder("filters");
        builder.Append("\nGROUP BY ");
        builder.Append(result);
    }

    /// <summary>
    ///     Registers an <c>ORDER BY</c> statement with the specified <paramref name="field"/>.
    /// </summary>
    /// <param name="field">The field to order by.</param>
    public virtual void OrderBy(string field)
    {
        Check.NotNullOrEmpty(field);

        var builder = GetOrCreateBuilder("filters");

        builder.Append("\nORDER BY ");
        builder.Append(Encloser.Reformat(field));
    }

    /// <summary>
    ///     Registers an <c>ORDER BY</c> statement with the specified collection of <paramref name="fields"/>.
    /// </summary>
    /// <param name="fields">The fields to order by.</param>
    public virtual void OrderBy(string[] fields)
    {
        Check.NotEmpty(fields);

        var result = string.Empty;
        for (var i = 0; i < fields.Length; i++)
        {
            result += Encloser.Wrap(fields[i]);

            if (i != fields.Length - 1)
                result += ", ";
        }

        var builder = GetOrCreateBuilder("filters");
        builder.Append("\nORDER BY ");
        builder.Append(result);
    }

    /// <summary>
    ///     Registers a query limit based on the specified <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The amout to limit by.</param>
    public virtual void Limit(string value)
    {
        Check.NotNullOrEmpty(value);

        var builder = GetOrCreateBuilder("filters");
        builder.Append("\nLIMIT ");
        builder.Append(value);
    }

    /// <summary>
    ///     Registers a query offset based on the specified <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value to offset from.</param>
    public virtual void Offset(string value)
    {
        Check.NotNullOrEmpty(value);

        var builder = GetOrCreateBuilder("filters");
        builder.Append("\nOFFSET ");
        builder.Append(value);
    }

    /// <summary>
    ///     Registers a <c>HAVING</c> statement with the specified <paramref name="condition"/>.
    /// </summary>
    /// <param name="condition">The field to group by.</param>
    public virtual void Having(string condition)
    {
        ThrowIfNull(condition);

        var builder = GetOrCreateBuilder("filters");

        builder.Append("\nHAVING ");
        builder.Append(Encloser.Reformat(condition));
    }

    /// <summary>
    ///     Registers a <c>WITH</c> query with the specified <paramref name="name"/> and <paramref name="body"/>.
    /// </summary>
    /// <param name="name">The name of the CTE.</param>
    /// <param name="body">The body as a separate <see cref="SqlBuilder"/>.</param>
    public virtual void With(string name, Func<SqlBuilder, string> body)
    {
        With(name, [], body);
    }

    /// <summary>
    ///     Registers a <c>WITH</c> query with the specified <paramref name="name"/>,
    ///     output <paramref name="fields"/>, and <paramref name="body"/>.
    /// </summary>
    /// <param name="name">The name of the CTE.</param>
    /// <param name="fields">The fields to output.</param>
    /// <param name="body">The body as a separate <see cref="SqlBuilder"/>.</param>
    public virtual void With(string name, string[] fields, Func<SqlBuilder, string> body)
    {
        var builder = new SqlBuilder(Encloser);
        var result = body.Invoke(builder);

        RegisterWith(name, fields, result);
    }

    /// <summary>
    ///     Registers a <c>RECURSIVE WITH</c> query with the specified <paramref name="name"/> and <paramref name="body"/>.
    /// </summary>
    /// <param name="name">The name of the CTE.</param>
    /// <param name="body">The body as a separate <see cref="SqlBuilder"/>.</param>
    public virtual void RecursiveWith(string name, Func<SqlBuilder, string> body)
    {
        RecursiveWith(name, [], body);
    }

    /// <summary>
    ///     Registers a <c>RECURSIVE WITH</c> query with the specified <paramref name="name"/>,
    ///     output <paramref name="fields"/>, and <paramref name="body"/>.
    /// </summary>
    /// <param name="name">The name of the CTE.</param>
    /// <param name="fields">The fields to output.</param>
    /// <param name="body">The body as a separate <see cref="SqlBuilder"/>.</param>
    public virtual void RecursiveWith(string name, string[] fields, Func<SqlBuilder, string> body)
    {
        var builder = new SqlBuilder(Encloser);
        var result = body.Invoke(builder);

        RegisterWith(name, fields, result, true);
    }

    /// <summary>
    ///     Registers a <c>WITH</c> query with the specified <paramref name="name"/>,
    ///     output <paramref name="fields"/>, and <paramref name="body"/>.
    /// </summary>
    /// <param name="name">The name of the CTE.</param>
    /// <param name="fields">The fields to output.</param>
    /// <param name="body">The body in string format.</param>
    /// <param name="recursive">The flag that indicates whether CTE is recursive.</param>
    public virtual void RegisterWith(string name, string[] fields, string body, bool recursive = false)
    {
        Check.NotNullOrEmpty(name);
        Check.NotNullOrEmpty(body);

        var builder = GetOrCreateBuilder("with_queries");

        builder.Append(builder.Length == 0 ? "WITH " : ",\n");

        if (recursive && (builder.Length == 5 || builder[5] != 'R'))
            builder.Insert(5, "RECURSIVE ");

        builder.Append(Encloser.Wrap(name));

        if (fields.Length != 0)
            builder.Append($" ({Encloser.Join(", ", fields)})");

        builder.AppendLine(" AS (");
        Indent(body, builder);

        builder.Append(')');
    }

    /// <summary>
    ///     Registers a <c>WITH</c> query with the specified <paramref name="name"/>,
    ///     output <paramref name="fields"/> that represents the data for the given number of <paramref name="rows"/>.
    /// </summary>
    /// <param name="name">The name of the CTE.</param>
    /// <param name="fields">The fields to output.</param>
    /// <param name="rows">The number of rows to generate parameters for.</param>
    public virtual void WithData(string name, string[] fields, int rows)
    {
        var len = fields.Length;
        var body = new StringBuilder("VALUES\n");

        for (var i = 0; i < rows; i++)
        {
            body.Append(Tab + "(");

            for (var j = 0; j < len; j++)
            {
                body.Append("@p" + (j + i * len));

                if (j < len - 1)
                    body.Append(", ");
            }

            body.Append(')');

            if (i < rows - 1)
                body.AppendLine(",\n");
        }

        RegisterWith(name, fields, body.ToString());
    }

    /// <summary>
    ///     Gets the builder of the specified <paramref name="name"/>, if any; otherwise, creates a new one with it.
    /// </summary>
    /// <param name="name">The name of the desired builder.</param>
    /// <returns>The builder of the specified <paramref name="name"/>.</returns>
    public virtual StringBuilder GetOrCreateBuilder(string name)
    {
        if (Builders.TryGetValue(name, out var builder))
            return builder;
        else
            Builders[name] = builder = new StringBuilder();

        return builder;
    }

    /// <summary>
    ///     Registers a <c>UNION</c> statement with the specified <paramref name="query"/>.
    /// </summary>
    /// <param name="query">The alt query as a separate <see cref="SqlBuilder"/>.</param>
    /// <param name="all">The flag that indicates whether to union all.</param>
    public virtual void RegisterUnionQuery(Func<SqlBuilder, string> query, bool all = false)
    {
        RegisterCombinedQuery("UNION", query, all);
    }

    /// <summary>
    ///     Registers a <c>UNION DISTINCT</c> statement with the specified <paramref name="query"/>.
    /// </summary>
    /// <param name="query">The alt query as a separate <see cref="SqlBuilder"/>.</param>
    public virtual void RegisterUnionDistinctQuery(Func<SqlBuilder, string> query)
    {
        RegisterCombinedQuery("UNION DISTINCT", query, false);
    }

    /// <summary>
    ///     Registers an <c>INTERSECT</c> statement with the specified <paramref name="query"/>.
    /// </summary>
    /// <param name="query">The alt query as a separate <see cref="SqlBuilder"/>.</param>
    /// <param name="all">The flag that indicates whether to union all.</param>
    public virtual void RegisterIntersectQuery(Func<SqlBuilder, string> query, bool all = false)
    {
        RegisterCombinedQuery("INTERSECT", query, all);
    }

    /// <summary>
    ///     Registers an <c>EXCEPT</c> statement with the specified <paramref name="query"/>.
    /// </summary>
    /// <param name="query">The alt query as a separate <see cref="SqlBuilder"/>.</param>
    /// <param name="all">The flag that indicates whether to union all.</param>
    public virtual void RegisterExceptQuery(Func<SqlBuilder, string> query, bool all = false)
    {
        RegisterCombinedQuery("EXCEPT", query, all);
    }

    /// <summary>
    ///     Registers a combined statement with the specified <paramref name="query"/> based on the given <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The type of the query; <c>UNION, INTERSECT, or EXCEPT</c>.</param>
    /// <param name="query">The alt query as a separate <see cref="SqlBuilder"/>.</param>
    /// <param name="all">The flag that indicates whether to union all.</param>
    protected void RegisterCombinedQuery(string type, Func<SqlBuilder, string> query, bool all = false)
    {
        Check.NotNullOrEmpty(type);
        Check.NotNull(query);

        var result = query.Invoke(new SqlBuilder(Encloser));
        if (string.IsNullOrEmpty(result))
            throw new InvalidOperationException("The query result cannot be null or empty");

        var builder = GetOrCreateBuilder("combine_queries");
        builder.Append('\n' + type + ' ');

        if (all)
            builder.Append("ALL ");

        builder.Append('\n' + result);
    }

    /// <summary>
    ///     Registers a "returning" statement.
    /// </summary>
    /// <param name="fields">The fields to be returned.</param>
    public virtual void RegisterReturningFields(params string[] fields)
    {
        var result = "\nRETURNING ";

        if (fields.Length == 0)
            result += "*";
        else
            for (var i = 0; i < fields.Length; i++)
            {
                result += Encloser.Reformat(fields[i]);

                if (i != fields.Length - 1)
                    result += ", ";
            }

        GetOrCreateBuilder("returning").Append(result);
    }

    /// <summary>
    ///     Registers a <c>NOT EXISTS</c> condition.
    /// </summary>
    /// <param name="stmt">the inner statement of the condition.</param>
    public virtual void WhereNotExists(string stmt)
    {
        var value = Indent(stmt).ToString();
        var result = $"NOT EXISTS (\n{value}\n)";

        if (!Builders.ContainsKey("where"))
            Where(result);

        else AppendAnd(result);
    }

    /// <summary>
    ///     Registers a <c>NOT EXISTS</c> condition.
    /// </summary>
    /// <param name="sql">the inner SQL content of the condition.</param>
    public virtual void WhereNotExists(Action<SqlBuilder> sql)
    {
        var builder = new SqlBuilder(Encloser);
        sql.Invoke(builder);

        WhereNotExists(builder.ToSelect());
    }

    /// <summary>
    ///     Gets and returns the content of the builder with the specified <paramref name="name"/>.
    /// </summary>
    /// <param name="name">The name of the builder to return from.</param>
    /// <returns>The content of the builder with the specified <paramref name="name"/>.</returns>
    protected string? GetBuilderContent(string name)
    {
        return Builders.TryGetValue(name, out var builder) ? builder.ToString() : null;
    }

    /// <summary>
    ///     Returns a <c>CAST</c> statement of the specified <paramref name="field"/> to the given <paramref name="type"/>.
    /// </summary>
    /// <param name="field">The column/value to be casted.</param>
    /// <param name="type">The type which to cast the field to.</param>
    /// <returns>A <c>CAST</c> statement of the specified <paramref name="field"/> to the given <paramref name="type"/>.</returns>
    public virtual string Cast(string field, string type)
    {
        return $"CAST ({Encloser.Reformat(field)} AS {Encloser.Replace(type)})";
    }

    /// <summary>
    ///     Generates and returns a <c>SELECT</c> statement from the specified configurations.
    /// </summary>
    /// <returns>A <c>SELECT</c> statement.</returns>
    public virtual string ToSelect()
    {
        var result = new StringBuilder();

        var withQueries = GetBuilderContent("with_queries");
        if (withQueries != null)
            result.AppendLine(withQueries);

        result.Append("SELECT ");
        result.Append(GetBuilderContent("fields") ?? "*");

        if (!string.IsNullOrEmpty(TableName))
            result.Append("\nFROM " + TableName);

        result.Append(GetBuilderContent("joins"));
        result.Append(GetBuilderContent("where"));
        result.Append(GetBuilderContent("filters"));
        result.Append(GetBuilderContent("combine_queries"));
        return result.ToString();
    }

    /// <summary>
    ///     Generates and returns an <c>UPDATE</c> statement from the specified configurations.
    /// </summary>
    /// <returns>A <c>UPDATE</c> statement.</returns>
    public virtual string ToUpdate()
    {
        var result = new StringBuilder();

        var withQueries = GetBuilderContent("with_queries");
        if (withQueries != null)
            result.AppendLine(withQueries);

        result.Append("UPDATE " + TableName);
        result.Append(" SET ");
        result.Append(GetBuilderContent("pairs"));

        var from = GetBuilderContent("from");
        if (from != null)
            result.Append(from);

        result.Append(GetBuilderContent("where"));
        result.Append(GetBuilderContent("returning"));

        return result.ToString();
    }

    /// <summary>
    ///     Generates and returns a <c>DELETE</c> statement from the specified configurations.
    /// </summary>
    /// <param name="cascade">The flag indicating whether to cascade depending records.</param>
    /// <returns>A <c>DELETE</c> statement.</returns>
    public virtual string ToDelete(bool cascade = false)
    {
        var result = new StringBuilder();

        var withQueries = GetBuilderContent("with_queries");
        if (withQueries != null)
            result.AppendLine(withQueries);

        result.Append("DELETE FROM " + TableName);

        if (cascade)
            result.Append(" CASCADE");

        result.Append(GetBuilderContent("where"));
        result.Append(GetBuilderContent("returning"));
        return result.ToString();
    }

    /// <summary>
    ///     Generates and returns an <c>INSERT</c> statement from the specified configurations.
    /// </summary>
    /// <param name="select">The action to specify the select body of the insertion.</param>
    /// <returns>A <c>iNSERT</c> statement.</returns>
    public virtual string ToInsert(Action<SqlBuilder>? select = null)
    {
        var result = new StringBuilder();

        var withQueries = GetBuilderContent("with_queries");
        if (withQueries != null)
            result.AppendLine(withQueries);

        result.Append($"INSERT INTO {TableName} (");
        result.Append(GetBuilderContent("fields"));
        result.AppendLine(")");

        if (select is null)
        {
            result.Append("VALUES ");
            result.Append(GetBuilderContent("values"));
            result.AppendLine(GetBuilderContent("where"));
        }
        else
        {
            var sql = new SqlBuilder(Encloser);

            select.Invoke(sql);
            result.Append(sql.ToSelect());
        }

        result.Append(GetBuilderContent("returning"));

        return result.ToString();
    }

    /// <summary>
    ///     Generates and returns a bulk <c>UPDATE</c> statement out of the specified configurations.
    /// </summary>
    /// <param name="alias">The alias used to retrieve the values.</param>
    /// <returns>A <c>UPDATE</c> statement.</returns>
    public virtual string ToBulkUpdate(string alias)
    {
        var builder = new StringBuilder("UPDATE " + TableName + " SET \n");
        builder.AppendLine(Tab + GetBuilderContent("pairs"));
        builder.AppendLine("FROM (VALUES");
        builder.AppendLine(Tab + GetBuilderContent("values"));
        builder.Append($") AS {alias}(" + GetBuilderContent("fields") + ")");
        builder.AppendLine(GetBuilderContent("where"));

        return builder.ToString();
    }

    /// <summary>
    ///     Generates and returns a <c>CREATE TABLE</c> statement out of the given <paramref name="table"/>.
    /// </summary>
    /// <param name="table">The table information.</param>
    /// <returns>A <c>CREATE TABLE</c> statement.</returns>
    public virtual string CreateTable(SqlTable table)
    {
        var builder = new StringBuilder("CREATE TABLE ");

        if (table.IfNotExists)
            builder.Append("IF NOT EXISTS ");

        if (!string.IsNullOrEmpty(table.Schema))
        {
            builder.Append(Encloser.Replace(table.Schema));
            builder.Append('.');
        }

        builder.Append(Encloser.Wrap(table.Name));
        builder.AppendLine(" (");

        ConfigureTablePrimaryKey(table);

        var length = table.Columns.Count;
        for (var i = 0; i < length; i++)
        {
            AppendColumn(builder, table.Columns[i]);
            builder.AppendLine(i != length - 1 ? "," : null);
        }

        AppendTableConstraints(builder, table.Constraints);

        builder.Append(");");
        return builder.ToString();
    }

    /// <summary>
    ///     Generates and returns a <c>CREATE TABLE</c> statement out of the given <paramref name="table"/>.
    /// </summary>
    /// <param name="table">The table information.</param>
    /// <returns>A <c>CREATE TABLE</c> statement.</returns>
    public virtual string DeleteTable(string table)
    {
        return $"DROP TABLE {Encloser.Wrap(table)};";
    }

    private void ConfigureTablePrimaryKey(SqlTable table)
    {
        var pkColumns = table.Columns.Where(col => col.PrimaryKey);
        if (pkColumns.Count() > 1)
        {
            if (table.Constraints.PrimaryKey != null)
                throw new InvalidOperationException("Cannot specify both an inline primary key and a primary key constraint at the same time.");

            var columns = pkColumns
                .Select(col =>
                {
                    col.PrimaryKey = false;
                    return Encloser.Wrap(col.Name);
                })
                .ToArray();

            table.Constraints.PrimaryKey = new PrimaryKeyConstraint(columns!);
        }
    }

    private void AppendTableConstraints(StringBuilder tableBuilder, TableConstraints constraints)
    {
        var builder = new StringBuilder();

        if (constraints.PrimaryKey != null)
            AppendPrimaryKey(builder, constraints.PrimaryKey);

        if (constraints.ForeignKeys?.Any() ?? false)
        {
            if (builder.Length != 0)
                builder.AppendLine(",");

            var length = constraints.ForeignKeys.Count;
            for (var i = 0; i < length; i++)
            {
                AppendForeignKey(builder, constraints.ForeignKeys[i]);

                if (i != length - 1)
                    builder.AppendLine(",");
            }
        }

        if (constraints.Uniques?.Any() ?? false)
        {
            if (builder.Length != 0)
                builder.AppendLine(",");

            var length = constraints.Uniques.Count;
            for (var i = 0; i < length; i++)
            {
                AppendUnique(builder, constraints.Uniques[i]);

                if (i != length - 1)
                    builder.AppendLine(",");
            }
        }

        if (constraints.Checks?.Any() ?? false)
        {
            if (builder.Length != 0)
                builder.AppendLine(",");

            var length = constraints.Checks.Count;
            for (var i = 0; i < length; i++)
            {
                AppendCheck(builder, constraints.Checks[i]);

                if (i != length - 1)
                    builder.AppendLine(",");
            }
        }

        if (builder.Length != 0)
        {
            tableBuilder.AppendLine(",");
            tableBuilder.Append(builder);
        }
    }

    protected virtual void AppendColumn(StringBuilder builder, SqlColumn column)
    {
        builder.Append(Tab);
        builder.Append(Encloser.Wrap(column.Name) + " " + column.Type);

        if (!string.IsNullOrWhiteSpace(column.Default))
        {
            builder.Append(" DEFAULT ");
            builder.Append(column.Default);
        }

        if (column.Unique)
            builder.Append(" UNIQUE");

        if (column.PrimaryKey)
            builder.Append(" PRIMARY KEY");

        if (column.Check != null)
        {
            builder.Append(" CHECK ");
            builder.Append(Encloser.Replace(column.Check.ToString()));
        }
    }

    protected virtual void AppendPrimaryKey(StringBuilder builder, PrimaryKeyConstraint constraint)
    {
        builder.Append(Tab);

        if (!string.IsNullOrEmpty(constraint.Name))
        {
            builder.Append(Tab);
            builder.Append("CONSTRAINT ");
            builder.Append(Encloser.Wrap(constraint.Name) + " ");
        }

        builder.Append("PRIMARY KEY (");
        builder.Append(Encloser.Join(", ", constraint.Columns));
        builder.Append(')');
    }

    protected virtual void AppendForeignKey(StringBuilder builder, ForeignKeyConstraint constraint)
    {
        builder.Append(Tab);

        if (!string.IsNullOrEmpty(constraint.Name))
        {
            builder.Append(Tab);
            builder.Append("CONSTRAINT ");
            builder.Append(Encloser.Wrap(constraint.Name) + " ");
        }

        builder.Append("FOREIGN KEY (");
        builder.Append(Encloser.Join(", ", constraint.Columns));
        builder.AppendLine(")");

        builder.Append(Tab);
        builder.Append("REFERENCES ");
        builder.Append(Encloser.Wrap(constraint.References.Table));
        builder.Append(" (");
        builder.Append(Encloser.Join(", ", constraint.References.Columns));
        builder.AppendLine(")");

        builder.Append(Tab);
        builder.Append("ON UPDATE ");
        builder.AppendLine(GetReferencialAction(constraint.OnUpdate));

        builder.Append(Tab);
        builder.Append("ON DELETE ");
        builder.Append(GetReferencialAction(constraint.OnDelete));
    }

    private static string GetReferencialAction(ReferencialAction action) => action switch
    {
        ReferencialAction.Restrict => "RESTRICT",
        ReferencialAction.Cascade => "CASCADE",
        ReferencialAction.SetDefault => "SET DEFAULT",
        ReferencialAction.SetNull => "SET NULL",
        _ => "NO ACTION"
    };

    protected virtual void AppendUnique(StringBuilder builder, UniqueConstraint constraint)
    {
        builder.Append(Tab);

        if (!string.IsNullOrEmpty(constraint.Name))
        {
            builder.Append(Tab);
            builder.Append("CONSTRAINT ");
            builder.Append(Encloser.Wrap(constraint.Name) + " ");
        }

        builder.Append("UNIQUE (");
        builder.Append(Encloser.Join(", ", constraint.Columns));
        builder.Append(')');
    }

    protected virtual void AppendCheck(StringBuilder builder, CheckConstraint constraint)
    {
        builder.Append(Tab);

        if (!string.IsNullOrEmpty(constraint.Name))
        {
            builder.Append(Tab);
            builder.Append("CONSTRAINT ");
            builder.Append(Encloser.Wrap(constraint.Name) + " ");
        }

        builder.Append("CHECK (");
        builder.Append(Encloser.Reformat(constraint.Condition?.ToString()));
        builder.Append(')');
    }

    /// <summary>
    ///     Clears the temporary configurations to start a fresh build.
    /// </summary>
    public void Clear() => Builders.Clear();

    /// <summary>
    ///     Generates and returns a <c>CRAETE DATABASE</c> statement for the specified <paramref name="database"/>.
    /// </summary>
    /// <param name="database">The name of the database to be created.</param>
    /// <returns>A <c>CREATE DATABASE</c> statement for the specified <paramref name="database"/>.</returns>
    public virtual string CreateDatabase(string database)
    {
        var builder = new StringBuilder("CREATE DATABASE ");

        builder.Append(Encloser.Wrap(database));
        builder.Append(';');

        return builder.ToString();
    }

    /// <summary>
    ///     Generates and returns a <c>DELETE DATABASE</c> statement for the specified <paramref name="database"/>.
    /// </summary>
    /// <param name="database">The name of the database to be created.</param>
    /// <returns>A <c>DELETE DATABASE</c> statement for the specified <paramref name="database"/>.</returns>
    public virtual string DeleteDatabase(string database)
    {
        var builder = new StringBuilder("DROP DATABASE ");

        builder.Append(Encloser.Wrap(database));
        builder.Append(';');

        return builder.ToString();
    }

    /// <summary>
    ///     Alters the <paramref name="currentName"/> of the database to be changed to the <paramref name="newName"/>.
    /// </summary>
    /// <param name="currentName">The name of the database to be renamed.</param>
    /// <param name="newName">The new name of the database.</param>
    /// <returns>The SQL statement to rename the database.</returns>
    /// <exception cref="NotImplementedException"></exception>
    public virtual string RenameDatabase(string currentName, string newName)
    {
        throw new NotImplementedException();
    }

    protected static StringBuilder Indent(string content, StringBuilder? builder = null)
    {
        builder ??= new StringBuilder();

        var reader = new StringReader(content);
        string? line;

        while ((line = reader.ReadLine()) is not null)
            builder.AppendLine(Tab + line);

        return builder;
    }
}
