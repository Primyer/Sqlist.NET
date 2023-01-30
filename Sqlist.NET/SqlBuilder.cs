#region License
// Copyright (c) 2021, Saleh Kawaf Kulla
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Sqlist.NET.Clauses;
using Sqlist.NET.Serialization;
using Sqlist.NET.Utilities;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Sqlist.NET
{
    /// <summary>
    ///     Provides the API to automatically generate SQL statements.
    /// </summary>
    /// <remarks>
    ///     The default style used in this class is <see cref="SqlStyle.PL_pgSQL"/>.
    /// </remarks>
    public class SqlBuilder
    {
        private const string Tab = "    ";

        private readonly SqlStyle _sqlStyle = default;
        private readonly Encloser _enc;
        private readonly Dictionary<string, StringBuilder> _builders = new Dictionary<string, StringBuilder>();

        private string _tableName;

        /// <summary>
        ///     Initalizes a new instance of the <see cref="SqlBuilder"/> class.
        /// </summary>
        /// <param name="style">The SQL syntax style to be used.</param>
        public SqlBuilder(SqlStyle style)
        {
            _sqlStyle = style;
            _enc = new Encloser(_sqlStyle);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SqlBuilder"/> class.
        /// </summary>
        public SqlBuilder() : this(default(SqlStyle))
        { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SqlBuilder"/> class.
        /// </summary>
        /// <param name="tableName">The name of table to base the statement on.</param>
        public SqlBuilder(string tableName) : this()
        {
            Check.NotNullOrEmpty(tableName, nameof(tableName));

            TableName = tableName;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SqlBuilder"/> class.
        /// </summary>
        /// <param name="tableName">The name of table to base the statement on.</param>
        /// <param name="style">The SQL syntax style to be used.</param>
        public SqlBuilder(string tableName, SqlStyle style) : this(style)
        {
            TableName = tableName;
        }

        /// <summary>
        ///     Gets or sets the name of the table name that the to-be-generated statement should base on.
        /// </summary>
        public string TableName
        {
            get => _tableName;
            set
            {
                _tableName = _enc.Reformat(value);
            }
        }

        /// <summary>
        ///     Registers the specified <paramref name="fields"/> in pairs similar to <c>field = @field</c>.
        /// </summary>
        /// <param name="fields">The field to pair up and register.</param>
        public void RegisterPairFields(string[] fields)
        {
            var builder = GetOrCreateBuilder("pairs");
            if (builder.Length != 0)
                builder.Append($",\n{Tab}");

            for (var i = 0; i < fields.Length; i++)
            {
                builder.Append(_enc.Reformat(fields[i]));
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
        public void RegisterPairFields((string, string) pair)
        {
            var builder = GetOrCreateBuilder("pairs");
            if (builder.Length != 0)
                builder.Append($",\n{Tab}");

            builder.Append(_enc.Reformat(pair.Item1));
            builder.Append(" = ");
            builder.Append(_enc.Replace(pair.Item2));
        }

        /// <summary>
        ///     Registers the specified <paramref name="pairs"/>.
        /// </summary>
        /// <param name="pairs">The of collection of pairs to register.</param>
        public void RegisterPairFields(Dictionary<string, string> pairs)
        {
            var builder = GetOrCreateBuilder("pairs");
            if (builder.Length != 0)
                builder.Append($",\n{Tab}");

            var count = 0;
            foreach (var (key, value) in pairs)
            {
                builder.Append(_enc.Reformat(key));
                builder.Append(" = ");
                builder.Append(_enc.Replace(value));

                if (count != pairs.Count - 1)
                    builder.Append($",\n{Tab}");

                count++;
            }
        }

        /// <summary>
        ///     Registers the specified <paramref name="entity"/> as a <c>FROM</c> clause for the update statement.
        /// </summary>
        /// <param name="entity">The entity to register.</param>
        public void From(string entity)
        {
            var builder = GetOrCreateBuilder("from");
            builder.AppendLine();
            builder.Append("FROM ");
            builder.Append(_enc.Reformat(entity));
        }

        /// <summary>
        ///     Registers the specified <paramref name="row"/> as values.
        /// </summary>
        /// <param name="row">The row to register.</param>
        public void RegisterValues(object[] row)
        {
            RegisterValues(new[] { row });
        }

        /// <summary>
        ///     Registers the specified collection of <paramref name="rows"/> as values.
        /// </summary>
        /// <param name="rows">The collection of rows to register.</param>
        public void RegisterValues(object[][] rows)
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
        public void RegisterBulkValues(int cols, int rows, int gapSize = 0, Action<int, string[]> configure = null)
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
        ///     Registers an <c>CROSS JOIN</c> on the specified <paramref name="table"/>.
        /// </summary>
        /// <param name="table">The table to join.</param>
        public void CrossJoin(string table)
        {
            table = table.IndexOf(' ') != -1 ? _enc.Replace(table) : _enc.Wrap(table);

            Join($"CROSS JOIN " + table);
        }

        /// <summary>
        ///     Registers an <c>CROSS JOIN</c> of the specified on the result of the <paramref name="configureSql"/>.
        /// </summary>
        /// <param name="alias">The alias of the subquery.</param>
        /// <param name="configureSql">The action to build the partial statement of the join.</param>
        public async void CrossJoin(string alias, Action<SqlBuilder> configureSql)
        {
            var sql = new SqlBuilder();
            configureSql.Invoke(sql);

            var stmt = await IndentAsync(sql.ToSelect());

            CrossJoin($"(\n{stmt}) AS {alias}");
        }

        /// <summary>
        ///     Registers an <c>INNER JOIN</c> on the specified <paramref name="table"/> with the given <paramref name="condition"/>.
        /// </summary>
        /// <param name="table">The table to join.</param>
        /// <param name="condition">The join conditions.</param>
        public void InnerJoin(string table, string condition = null)
        {
            Join("INNER", table, condition);
        }

        /// <summary>
        ///     Registers an <c>INNER JOIN</c> on the specified <paramref name="table"/> with the given <paramref name="condition"/>.
        /// </summary>
        /// <param name="table">The table to join.</param>
        /// <param name="condition">The join conditions.</param>
        public void InnerJoin(string table, Action<ConditionalClause> condition)
        {
            Join("INNER", table, condition.ToString());
        }

        /// <summary>
        ///     Registers an <c>INNER JOIN</c> of the specified on the result of the <paramref name="configureSql"/>
        ///     with respect to the given <paramref name="condition"/>.
        /// </summary>
        /// <param name="alias">The alias of the subquery.</param>
        /// <param name="configureSql">The action to build the partial statement of the join.</param>
        /// <param name="condition">The condition of join operation.</param>
        public void InnerJoin(string alias, Action<SqlBuilder> configureSql, string condition = null)
        {
            Join("INNER", alias, configureSql, condition);
        }

        /// <summary>
        ///     Registers a <c>LEFT JOIN</c> on the specified <paramref name="table"/> with the given <paramref name="condition"/>.
        /// </summary>
        /// <param name="table">The table to join.</param>
        /// <param name="condition">The join conditions.</param>
        public void LeftJoin(string table, string condition = null)
        {
            Join("LEFT", table, condition);
        }

        /// <summary>
        ///     Registers a <c>LEFT JOIN</c> on the specified <paramref name="table"/> with the given <paramref name="condition"/>.
        /// </summary>
        /// <param name="table">The table to join.</param>
        /// <param name="condition">The join conditions.</param>
        public void LeftJoin(string table, Action<ConditionalClause> condition)
        {
            Join("LEFT", table, condition.ToString());
        }

        /// <summary>
        ///     Registers an <c>LEFT JOIN</c> of the specified on the result of the <paramref name="configureSql"/>
        ///     with respect to the given <paramref name="condition"/>.
        /// </summary>
        /// <param name="alias">The alias of the subquery.</param>
        /// <param name="configureSql">The action to build the partial statement of the join.</param>
        /// <param name="condition">The condition of join operation.</param>
        public void LeftJoin(string alias, Action<SqlBuilder> configureSql, string condition = null)
        {
            Join("LEFT", alias, configureSql, condition);
        }

        /// <summary>
        ///     Registers a <c>RIGHT JOIN</c> on the specified <paramref name="table"/> with the given <paramref name="condition"/>.
        /// </summary>
        /// <param name="table">The table to join.</param>
        /// <param name="condition">The join conditions.</param>
        public void RightJoin(string table, string condition = null)
        {
            Join("RIGHT", table, condition);
        }

        /// <summary>
        ///     Registers a <c>RIGHT JOIN</c> on the specified <paramref name="table"/> with the given <paramref name="condition"/>.
        /// </summary>
        /// <param name="table">The table to join.</param>
        /// <param name="condition">The join conditions.</param>
        public void RightJoin(string table, Action<ConditionalClause> condition)
        {
            Join("RIGHT", table, condition.ToString());
        }

        /// <summary>
        ///     Registers an <c>RIGHT JOIN</c> of the specified on the result of the <paramref name="configureSql"/>
        ///     with respect to the given <paramref name="condition"/>.
        /// </summary>
        /// <param name="alias">The alias of the subquery.</param>
        /// <param name="configureSql">The action to build the partial statement of the join.</param>
        /// <param name="condition">The condition of join operation.</param>
        public void RightJoin(string alias, Action<SqlBuilder> configureSql, string condition = null)
        {
            Join("RIGHT", alias, configureSql, condition);
        }

        /// <summary>
        ///     Registers a <c>FULL JOIN</c> on the specified <paramref name="table"/> with the given <paramref name="condition"/>.
        /// </summary>
        /// <param name="table">The table to join.</param>
        /// <param name="condition">The join conditions.</param>
        public void FullJoin(string table, string condition = null)
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
        public void FullJoin(string alias, Action<SqlBuilder> configureSql, string condition = null)
        {
            Join("FULL", alias, configureSql, condition);
        }

        /// <summary>
        ///     Registers a join of the specified <paramref name="type"/> on the result of
        ///     the <paramref name="configureSql"/> with respect to the given <paramref name="condition"/>.
        /// </summary>
        /// <param name="type">The type of join being registered.</param>
        /// <param name="alias">The alias of the subquery.</param>
        /// <param name="configureSql">The action to build the partial statement of the join.</param>
        /// <param name="condition">The condition of join operation.</param>
        public async void Join(string type, string alias, Action<SqlBuilder> configureSql, string condition = null)
        {
            var sql = new SqlBuilder();
            configureSql.Invoke(sql);

            var stmt = await IndentAsync(sql.ToSelect());

            Join(type, $"(\n{stmt}) AS {alias}", condition);
        }

        /// <summary>
        ///     Registers a join of the specified <paramref name="type"/> with the given <paramref name="entity"/> and <paramref name="condition"/>.
        /// </summary>
        /// <param name="type">The type of join being registered.</param>
        /// <param name="entity">The entity which to join with.</param>
        /// <param name="condition">The condition of join operation.</param>
        public void Join(string type, string entity, string condition = null)
        {
            Join($"{type} JOIN {_enc.Reformat(entity)} ON " + (condition is null ? "true" : _enc.Reformat(condition)));
        }

        /// <summary>
        ///     Register the specified partial <paramref name="stmt"/> as a join.
        /// </summary>
        /// <param name="stmt">The statement to register.</param>
        public void Join(string stmt)
        {
            Check.NotNullOrEmpty(stmt, nameof(stmt));

            GetOrCreateBuilder("joins").Append('\n' + stmt);
        }

        /// <summary>
        ///     Registers the <c>WHERE</c> statement with the given <paramref name="condition"/>.
        /// </summary>
        /// <param name="condition">The condition to register.</param>
        public void Where(string condition)
        {
            Check.NotNullOrEmpty(condition, nameof(condition));

            var builder = _builders["where"] = new StringBuilder();

            builder.Append("\nWHERE ");
            _enc.Replace(ref condition);
            builder.Append(condition);
        }

        /// <summary>
        ///     Registers the <c>WHERE</c> statement with the given <paramref name="condition"/>.
        /// </summary>
        /// <param name="condition">The condition to register.</param>
        public void Where(Action<ConditionalClause> condition)
        {
            Where(condition.ToString());
        }

        /// <summary>
        ///     Appends an <c>OR</c> followed by the specified <paramref name="condition"/> to the <c>WHERE</c> statement.
        /// </summary>
        /// <param name="condition">The condition to append.</param>
        public void AppendOr(string condition)
        {
            AppendCondition("OR " + condition);
        }

        /// <summary>
        ///     Appends an <c>OR NOT</c> followed by the specified <paramref name="condition"/> to the <c>WHERE</c> statement.
        /// </summary>
        /// <param name="condition">The condition to append.</param>
        public void AppendOrNot(string condition)
        {
            AppendCondition("OR NOT " + condition);
        }

        /// <summary>
        ///     Appends an <c>AND</c> followed by the specified <paramref name="condition"/> to the <c>WHERE</c> statement.
        /// </summary>
        /// <param name="condition">The condition to append.</param>
        public void AppendAnd(string condition)
        {
            AppendCondition("AND " + condition);
        }

        /// <summary>
        ///     Appends an <c>AND NOT</c> followed by the specified <paramref name="condition"/> to the <c>WHERE</c> statement.
        /// </summary>
        /// <param name="condition">The condition to append.</param>
        public void AppendAndNot(string condition)
        {
            AppendCondition("AND NOT " + condition);
        }

        /// <summary>
        ///     Appends the given condition to the <c>WHERE</c> statement.
        /// </summary>
        /// <param name="condition">The condition to append.</param>
        public void AppendCondition(string condition)
        {
            Check.NotNullOrEmpty(condition, nameof(condition));

            if (!_builders.TryGetValue("where", out var builder))
                throw new InvalidOperationException("The 'where' condition wasn't initialized.");

            _enc.Reformat(ref condition);
            builder.Append(" " + condition);
        }

        /// <summary>
        ///     Registers the specified <paramref name="field"/>.
        /// </summary>
        /// <param name="field">The field to register.</param>
        /// <param name="reformat">Indicates whether to re-format.</param>
        public void RegisterFields(string field, bool reformat = true)
        {
            Check.NotNullOrEmpty(field, nameof(field));

            var builder = GetOrCreateBuilder("fields");
            if (builder.Length != 0)
                builder.Append(", ");

            builder.Append(reformat ? _enc.Reformat(field) : field);
        }

        /// <summary>
        ///     Registers the specified collection of <paramref name="fields"/>.
        /// </summary>
        /// <param name="fields">The fields to register.</param>
        public void RegisterFields(string[] fields)
        {
            Check.NotEmpty(fields, nameof(fields));

            var builder = GetOrCreateBuilder("fields");
            if (builder.Length != 0)
                builder.Append(", ");

            for (var i = 0; i < fields.Length; i++)
            {
                builder.Append(_enc.Reformat(fields[i]));

                if (i != fields.Length - 1)
                    builder.Append(", ");
            }
        }

        /// <summary>
        ///     Registers a <c>GROUP BY</c> statement with the specified <paramref name="field"/>.
        /// </summary>
        /// <param name="field">The field to group by.</param>
        public void GroupBy(string field)
        {
            Check.NotNullOrEmpty(field, nameof(field));

            var builder = GetOrCreateBuilder("filters");
            builder.Append("\nGROUP BY ");

            _enc.Reformat(ref field);
            builder.Append(field);
        }

        /// <summary>
        ///     Registers a <c>WINDOW</c> statement with the specified <paramref name="alias"/>.
        /// </summary>
        /// <param name="alias">The alias of the statement.</param>
        /// <param name="content">The conetnt of the statement.</param>
        public void Window(string alias, string content)
        {
            Check.NotNullOrEmpty(alias, nameof(alias));
            Check.NotNullOrEmpty(content, nameof(content));

            var builder = GetOrCreateBuilder("filters");
            
            builder.AppendLine();
            builder.Append($"WINDOW {_enc.Replace(alias)} AS ({_enc.Replace(content)})");
        }

        /// <summary>
        ///     Registers a <c>GROUP BY</c> statement with the specified collection of <paramref name="fields"/>.
        /// </summary>
        /// <param name="fields">The fields to group by.</param>
        public void GroupBy(string[] fields)
        {
            Check.NotEmpty(fields, nameof(fields));

            var result = string.Empty;
            for (var i = 0; i < fields.Length; i++)
            {
                _enc.Wrap(ref fields[i]);
                result += fields[i];

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
        public void OrderBy(string field)
        {
            Check.NotNullOrEmpty(field, nameof(field));

            var builder = GetOrCreateBuilder("filters");
            builder.Append("\nORDER BY ");

            _enc.Reformat(ref field);
            builder.Append(field);
        }

        /// <summary>
        ///     Registers an <c>ORDER BY</c> statement with the specified collection of <paramref name="fields"/>.
        /// </summary>
        /// <param name="fields">The fields to order by.</param>
        public void OrderBy(string[] fields)
        {
            Check.NotEmpty(fields, nameof(fields));

            var result = string.Empty;
            for (var i = 0; i < fields.Length; i++)
            {
                _enc.Wrap(ref fields[i]);
                result += fields[i];

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
        public void Limit(string value)
        {
            Check.NotNullOrEmpty(value, nameof(value));

            var builder = GetOrCreateBuilder("filters");
            builder.Append("\nLIMIT ");
            builder.Append(value);
        }

        /// <summary>
        ///     Registers a query offset based on the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value to offset from.</param>
        public void Offset(string value)
        {
            Check.NotNullOrEmpty(value, nameof(value));

            var builder = GetOrCreateBuilder("filters");
            builder.Append("\nOFFSET ");
            builder.Append(value);
        }

        /// <summary>
        ///     Registers a <c>WITH</c> query with the specified <paramref name="name"/> and <paramref name="body"/>.
        /// </summary>
        /// <param name="name">The name of the CTE.</param>
        /// <param name="body">The body as a separate <see cref="SqlBuilder"/>.</param>
        public void With(string name, Func<SqlBuilder, string> body)
        {
            With(name, Array.Empty<string>(), body);
        }

        /// <summary>
        ///     Registers a <c>WITH</c> query with the specified <paramref name="name"/>,
        ///     output <paramref name="fields"/>, and <paramref name="body"/>.
        /// </summary>
        /// <param name="name">The name of the CTE.</param>
        /// <param name="fields">The fields to output.</param>
        /// <param name="body">The body as a separate <see cref="SqlBuilder"/>.</param>
        public void With(string name, string[] fields, Func<SqlBuilder, string> body)
        {
            var builder = new SqlBuilder(_sqlStyle);
            var result = body.Invoke(builder);

            RegisterWith(name, fields, result);
        }

        /// <summary>
        ///     Registers a <c>RECURSIVE WITH</c> query with the specified <paramref name="name"/> and <paramref name="body"/>.
        /// </summary>
        /// <param name="name">The name of the CTE.</param>
        /// <param name="body">The body as a separate <see cref="SqlBuilder"/>.</param>
        public void RecursiveWith(string name, Func<SqlBuilder, string> body)
        {
            RecursiveWith(name, Array.Empty<string>(), body);
        }

        /// <summary>
        ///     Registers a <c>RECURSIVE WITH</c> query with the specified <paramref name="name"/>,
        ///     output <paramref name="fields"/>, and <paramref name="body"/>.
        /// </summary>
        /// <param name="name">The name of the CTE.</param>
        /// <param name="fields">The fields to output.</param>
        /// <param name="body">The body as a separate <see cref="SqlBuilder"/>.</param>
        public void RecursiveWith(string name, string[] fields, Func<SqlBuilder, string> body)
        {
            var builder = new SqlBuilder(_sqlStyle);
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
        public async void RegisterWith(string name, string[] fields, string body, bool recursive = false)
        {
            Check.NotNullOrEmpty(name, nameof(name));
            Check.NotNullOrEmpty(body, nameof(body));

            var builder = GetOrCreateBuilder("with_queries");
            
            builder.Append(builder.Length == 0 ? "WITH " : ",\n");

            if (recursive && (builder.Length == 5 || builder[5] != 'r'))
                builder.Insert(5, "RECURSIVE ");

            builder.Append(_enc.Wrap(name));

            if (fields.Length != 0)
                builder.Append($" ({_enc.Join(", ", fields)})");

            builder.AppendLine(" AS (");
            await IndentAsync(body, builder);

            builder.Append($")");
        }

        /// <summary>
        ///     Registers a <c>WITH</c> query with the specified <paramref name="name"/>,
        ///     output <paramref name="fields"/> that represents the data for the given number of <paramref name="rows"/>.
        /// </summary>
        /// <param name="name">The name of the CTE.</param>
        /// <param name="fields">The fields to output.</param>
        /// <param name="rows">The number of rows to generate parameters for.</param>
        public void WithData(string name, string[] fields, int rows)
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

                body.Append(")");

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
        public StringBuilder GetOrCreateBuilder(string name)
        {
            if (_builders.TryGetValue(name, out var builder))
                return builder;
            else
                _builders[name] = builder = new StringBuilder();

            return builder;
        }

        /// <summary>
        ///     Registers a <c>UNION</c> statement with the specified <paramref name="query"/>.
        /// </summary>
        /// <param name="query">The alt query as a separate <see cref="SqlBuilder"/>.</param>
        /// <param name="all">The flag that indicates whether to union all.</param>
        public void RegisterUnionQuery(Func<SqlBuilder, string> query, bool all = false)
        {
            RegisterCombinedQuery("UNION", query, all);
        }

        /// <summary>
        ///     Registers an <c>INTERSECT</c> statement with the specified <paramref name="query"/>.
        /// </summary>
        /// <param name="query">The alt query as a separate <see cref="SqlBuilder"/>.</param>
        /// <param name="all">The flag that indicates whether to union all.</param>
        public void RegisterIntersectQuery(Func<SqlBuilder, string> query, bool all = false)
        {
            RegisterCombinedQuery("INTERSECT", query, all);
        }

        /// <summary>
        ///     Registers an <c>EXCEPT</c> statement with the specified <paramref name="query"/>.
        /// </summary>
        /// <param name="query">The alt query as a separate <see cref="SqlBuilder"/>.</param>
        /// <param name="all">The flag that indicates whether to union all.</param>
        public void RegisterExceptQuery(Func<SqlBuilder, string> query, bool all = false)
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
            Check.NotNullOrEmpty(type, nameof(type));
            Check.NotNull(query, nameof(query));

            var result = query.Invoke(new SqlBuilder(_sqlStyle));
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
        public void RegisterReturningFields(params string[] fields)
        {
            var result = "\nRETURNING ";

            if (fields.Length == 0)
                result += "*";
            else
                for (var i = 0; i < fields.Length; i++)
                {
                    result += _enc.Reformat(fields[i]);

                    if (i != fields.Length - 1)
                        result += ", ";
                }

            GetOrCreateBuilder("returning").Append(result);
        }

        public void RegisterConflictConstraint(string constraint)
        {
            _builders["conflict"] = new StringBuilder($"ON CONSTRAINT \"{constraint}\"");
        }

        /// <summary>
        ///     Registers the fields for the conflict statement.
        /// </summary>
        /// <param name="keys">The conflict fields.</param>
        public void RegisterConflictFields(params string[] keys)
        {
            var builder = _builders["conflict"] = new StringBuilder("(");

            for (var i = 0; i < keys.Length; i++)
            {
                _enc.Wrap(ref keys[i]);
                builder.Append(keys[i]);

                if (i != keys.Length - 1)
                    builder.Append(", ");
            }
            builder.Append(")");
        }

        /// <summary>
        ///     Registers a <c>NOT EXISTS</c> condition.
        /// </summary>
        /// <param name="stmt">the inner statement of the condition.</param>
        public async void WhereNotExists(string stmt)
        {
            var value = (await IndentAsync(stmt)).ToString();
            var result = $"NOT EXISTS (\n{value}\n)";

            if (!_builders.ContainsKey("where"))
                Where(result);

            else AppendAnd(result);
        }

        /// <summary>
        ///     Registers a <c>NOT EXISTS</c> condition.
        /// </summary>
        /// <param name="sql">the inner SQL content of the condition.</param>
        public void WhereNotExists(Action<SqlBuilder> sql)
        {
            var builder = new SqlBuilder();
            sql.Invoke(builder);

            WhereNotExists(builder.ToSelect());
        }

        /// <summary>
        ///     Gets and returns the content of the builder with the specified <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the builder to return from.</param>
        /// <returns>The content of the builder with the specified <paramref name="name"/>.</returns>
        protected string GetBuilderContent(string name)
        {
            return _builders.TryGetValue(name, out var builder) ? builder.ToString() : null;
        }

        /// <summary>
        ///     Generates and returns a <c>SELECT</c> statement from the specified configurations.
        /// </summary>
        /// <returns>A <c>SELECT</c> statement.</returns>
        public string ToSelect()
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
        public string ToUpdate()
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
        public string ToDelete(bool cascade = false)
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
        public string ToInsert(Action<SqlBuilder> select = null)
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
                var sql = new SqlBuilder();

                select.Invoke(sql);
                result.Append(sql.ToSelect());
            }

            result.Append(GetBuilderContent("returning"));

            return result.ToString();
        }

        /// <summary>
        ///     Generates and returns an <c>INSERT ON CONFLICT</c> statement from the specified configurations.
        /// </summary>
        /// <returns>A <c>INSERT ON CONFLICT</c> statement.</returns>
        public string ToInsertOrUpdate()
        {
            var result = new StringBuilder($"INSERT INTO {TableName} (");

            result.Append(GetBuilderContent("fields"));
            result.Append(")\nVALUES ");
            result.Append(GetBuilderContent("values"));
            result.AppendLine(GetBuilderContent("where"));
            result.Append("ON CONFLICT ");
            result.Append(GetBuilderContent("conflict"));

            var pairs = GetBuilderContent("pairs");
            if (pairs != null)
            {
                result.Append("DO UPDATE SET ");
                result.Append(pairs);
            }
            else
            {
                result.Append("DO NOTHING");
            }

            result.Append(GetBuilderContent("returning"));

            return result.ToString();
        }

        /// <summary>
        ///     Generates and returns a bulk <c>UPDATE</c> statement out of the specified configurations.
        /// </summary>
        /// <param name="alias">The alias used to retrieve the values.</param>
        /// <returns>A <c>UPDATE</c> statement.</returns>
        public string ToBulkUpdate(string alias)
        {
            if (_sqlStyle != SqlStyle.PL_pgSQL)
                throw new NotSupportedException($"ToBulkUpdate() is not supported for '{_sqlStyle}'");

            var builder = new StringBuilder("update " + TableName + " SET \n");
            builder.AppendLine(Tab + GetBuilderContent("pairs"));
            builder.AppendLine("FROM (VALUES");
            builder.AppendLine(Tab + GetBuilderContent("values"));
            builder.Append($") AS {alias}(" + GetBuilderContent("fields") + ")");
            builder.AppendLine(GetBuilderContent("where"));

            return builder.ToString();
        }

        /// <summary>
        ///     Clears the temporary configurations to start a fresh build.
        /// </summary>
        public void Clear()
        {
            _builders.Clear();
        }

        private async Task<StringBuilder> IndentAsync(string content, StringBuilder builder = null)
        {
            builder ??= new StringBuilder();

            var reader = new StringReader(content);
            string line;

            while (null != (line = await reader.ReadLineAsync()))
                builder.AppendLine(Tab + line);

            return builder;
        }
    }
}
