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

using Sqlist.NET.Utilities;

using System;
using System.Collections.Generic;
using System.Text;

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
        public virtual void RegisterPairFields(string[] fields)
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
        ///     Registers the specified <paramref name="row"/> as values.
        /// </summary>
        /// <param name="row">The row to register.</param>
        public virtual void RegisterValues(object[] row)
        {
            RegisterValues(new[] { row });
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

        public virtual void RegisterBulkValues(int cols, int rows)
        {
            var obj = new string[rows][];
            for (var i = 0; i < rows; i++)
            {
                obj[i] = new string[cols];
                for (var j = 0; j < cols; j++)
                {
                    obj[i][j] = "@p" + (j + i * cols);
                }
            }
            RegisterValues(obj);
        }

        /// <summary>
        ///     Registers an <c>INNER JOIN</c> on the specified <paramref name="table"/> with the given <paramref name="condition"/>.
        /// </summary>
        /// <param name="table">The table to join.</param>
        /// <param name="condition">The join conditions.</param>
        public virtual void RegisterInnerJoin(string table, string condition)
        {
            _enc.Reformat(ref table);
            _enc.Reformat(ref condition);
            RegisterJoin($"inner join {table} on {condition}");
        }

        /// <summary>
        ///     Registers a <c>LEFT JOIN</c> on the specified <paramref name="table"/> with the given <paramref name="condition"/>.
        /// </summary>
        /// <param name="table">The table to join.</param>
        /// <param name="condition">The join conditions.</param>
        public virtual void RegisterLeftJoin(string table, string condition)
        {
            _enc.Reformat(ref table);
            _enc.Reformat(ref condition);
            RegisterJoin($"left join {table} on {condition}");
        }

        /// <summary>
        ///     Registers a <c>RIGHT JOIN</c> on the specified <paramref name="table"/> with the given <paramref name="condition"/>.
        /// </summary>
        /// <param name="table">The table to join.</param>
        /// <param name="condition">The join conditions.</param>
        public virtual void RegisterRightJoin(string table, string condition)
        {
            _enc.Reformat(ref table);
            _enc.Reformat(ref condition);
            RegisterJoin($"right join {table} on {condition}");
        }

        /// <summary>
        ///     Registers a <c>FULL JOIN</c> on the specified <paramref name="table"/> with the given <paramref name="condition"/>.
        /// </summary>
        /// <param name="table">The table to join.</param>
        /// <param name="condition">The join conditions.</param>
        public virtual void RegisterFullJoin(string table, string condition)
        {
            _enc.Reformat(ref table);
            _enc.Reformat(ref condition);
            RegisterJoin($"full join {table} on {condition}");
        }

        /// <summary>
        ///     Register the specified partial <paramref name="stmt"/> as a join.
        /// </summary>
        /// <param name="stmt">The statement to register.</param>
        public virtual void RegisterJoin(string stmt)
        {
            Check.NotNullOrEmpty(stmt, nameof(stmt));

            GetOrCreateBuilder("joins").Append('\n' + stmt);
        }

        /// <summary>
        ///     Registers the <c>WHERE</c> statement with the given <paramref name="condition"/>.
        /// </summary>
        /// <param name="condition">The condition to register.</param>
        public virtual void RegisterWhere(string condition)
        {
            Check.NotNullOrEmpty(condition, nameof(condition));

            var builder = _builders["where"] = new StringBuilder();

            builder.Append("\nwhere ");
            _enc.Reformat(ref condition);
            builder.Append(condition);
        }

        /// <summary>
        ///     Appends an <c>OR</c> followed by the specified <paramref name="condition"/> to the <c>WHERE</c> statement.
        /// </summary>
        /// <param name="condition">The condition to append.</param>
        public virtual void AppendOr(string condition)
        {
            AppendCondition("or " + condition);
        }

        /// <summary>
        ///     Appends an <c>OR NOT</c> followed by the specified <paramref name="condition"/> to the <c>WHERE</c> statement.
        /// </summary>
        /// <param name="condition">The condition to append.</param>
        public virtual void AppendOrNot(string condition)
        {
            AppendCondition("or not " + condition);
        }

        /// <summary>
        ///     Appends an <c>AND</c> followed by the specified <paramref name="condition"/> to the <c>WHERE</c> statement.
        /// </summary>
        /// <param name="condition">The condition to append.</param>
        public virtual void AppendAnd(string condition)
        {
            AppendCondition("and " + condition);
        }

        /// <summary>
        ///     Appends an <c>AND NOT</c> followed by the specified <paramref name="condition"/> to the <c>WHERE</c> statement.
        /// </summary>
        /// <param name="condition">The condition to append.</param>
        public virtual void AppendAndNot(string condition)
        {
            AppendCondition("and not " + condition);
        }

        /// <summary>
        ///     Appends the given condition to the <c>WHERE</c> statement.
        /// </summary>
        /// <param name="condition">The condition to append.</param>
        public virtual void AppendCondition(string condition)
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
        public virtual void RegisterFields(string field)
        {
            Check.NotNullOrEmpty(field, nameof(field));

            var builder = GetOrCreateBuilder("fields");
            if (builder.Length != 0)
                builder.Append(",\n" + Tab);

            _enc.Reformat(ref field);
            builder.Append(field);
        }

        /// <summary>
        ///     Registers the specified collection of <paramref name="fields"/>.
        /// </summary>
        /// <param name="fields">The fields to register.</param>
        public virtual void RegisterFields(string[] fields)
        {
            Check.NotEmpty(fields, nameof(fields));

            var builder = GetOrCreateBuilder("fields");
            if (builder.Length != 0)
                builder.Append(",\n" + Tab);

            for (var i = 0; i < fields.Length; i++)
            {
                _enc.Wrap(ref fields[i]);
                builder.Append(fields[i]);

                if (i != fields.Length - 1)
                    builder.Append(",\n" + Tab);
            }
        }

        /// <summary>
        ///     Registers a <c>GROUP BY</c> statement with the specified <paramref name="field"/>.
        /// </summary>
        /// <param name="field">The field to group by.</param>
        public virtual void GroupBy(string field)
        {
            Check.NotNullOrEmpty(field, nameof(field));

            var builder = GetOrCreateBuilder("filters");
            builder.Append("\ngroup by ");

            _enc.Reformat(ref field);
            builder.Append(field);
        }

        /// <summary>
        ///     Registers a <c>GROUP BY</c> statement with the specified collection of <paramref name="fields"/>.
        /// </summary>
        /// <param name="fields">The fields to group by.</param>
        public virtual void GroupBy(string[] fields)
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
            builder.Append("\ngroup by ");
            builder.Append(result);
        }

        /// <summary>
        ///     Registers an <c>ORDER BY</c> statement with the specified <paramref name="field"/>.
        /// </summary>
        /// <param name="field">The field to order by.</param>
        public virtual void OrderBy(string field)
        {
            Check.NotNullOrEmpty(field, nameof(field));

            var builder = GetOrCreateBuilder("filters");
            builder.Append("\norder by ");

            _enc.Reformat(ref field);
            builder.Append(field);
        }

        /// <summary>
        ///     Registers an <c>ORDER BY</c> statement with the specified collection of <paramref name="fields"/>.
        /// </summary>
        /// <param name="fields">The fields to order by.</param>
        public virtual void OrderBy(string[] fields)
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
            builder.Append("\norder by ");
            builder.Append(result);
        }

        /// <summary>
        ///     Registers a query limit based on the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The amout to limit by.</param>
        public virtual void Limit(string value)
        {
            Check.NotNullOrEmpty(value, nameof(value));

            var builder = GetOrCreateBuilder("filters");
            builder.Append("\nlimit ");
            builder.Append(value);
        }

        /// <summary>
        ///     Registers a query offset based on the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value to offset from.</param>
        public virtual void Offset(string value)
        {
            Check.NotNullOrEmpty(value, nameof(value));

            var builder = GetOrCreateBuilder("filters");
            builder.Append("\noffset ");
            builder.Append(value);
        }

        /// <summary>
        ///     Registers a <c>WITH</c> query with the specified <paramref name="name"/> and <paramref name="body"/>.
        /// </summary>
        /// <param name="name">The name of the CTE.</param>
        /// <param name="body">The body as a separate <see cref="SqlBuilder"/>.</param>
        public virtual void RegisterWith(string name, Func<SqlBuilder, string> body)
        {
            RegisterWith(name, Array.Empty<string>(), body);
        }

        /// <summary>
        ///     Registers a <c>WITH</c> query with the specified <paramref name="name"/>,
        ///     output <paramref name="fields"/>, and <paramref name="body"/>.
        /// </summary>
        /// <param name="name">The name of the CTE.</param>
        /// <param name="fields">The fields to output.</param>
        /// <param name="body">The body as a separate <see cref="SqlBuilder"/>.</param>
        public virtual void RegisterWith(string name, string[] fields, Func<SqlBuilder, string> body)
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
        public virtual void RegisterRecursiveWith(string name, Func<SqlBuilder, string> body)
        {
            RegisterRecursiveWith(name, Array.Empty<string>(), body);
        }

        /// <summary>
        ///     Registers a <c>RECURSIVE WITH</c> query with the specified <paramref name="name"/>,
        ///     output <paramref name="fields"/>, and <paramref name="body"/>.
        /// </summary>
        /// <param name="name">The name of the CTE.</param>
        /// <param name="fields">The fields to output.</param>
        /// <param name="body">The body as a separate <see cref="SqlBuilder"/>.</param>
        public virtual void RegisterRecursiveWith(string name, string[] fields, Func<SqlBuilder, string> body)
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
        public virtual void RegisterWith(string name, string[] fields, string body, bool recursive = false)
        {
            Check.NotNullOrEmpty(name, nameof(name));
            Check.NotNullOrEmpty(body, nameof(body));

            var builder = GetOrCreateBuilder("with_queries");
            if (builder.Length == 0)
            {
                builder.Append("with ");
            }
            else
                builder.Append(",\n");

            if (recursive)
            {
                if (builder.Length == 5 || builder[5] != 'r')
                    builder.Insert(5, "recursive ");
            }

            _enc.Wrap(ref name);
            builder.Append(name);

            if (fields.Length != 0)
                builder.Append($" ({_enc.Join(", ", fields)})");

            builder.Append($" as (\n{body}\n)");
        }

        /// <summary>
        ///     Gets the builder of the specified <paramref name="name"/>, if any; otherwise, creates a new one with it.
        /// </summary>
        /// <param name="name">The name of the desired builder.</param>
        /// <returns>The builder of the specified <paramref name="name"/>.</returns>
        public virtual StringBuilder GetOrCreateBuilder(string name)
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
        public virtual void RegisterUnionQuery(Func<SqlBuilder, string> query, bool all = false)
        {
            RegisterCombinedQuery("union", query, all);
        }

        /// <summary>
        ///     Registers an <c>INTERSECT</c> statement with the specified <paramref name="query"/>.
        /// </summary>
        /// <param name="query">The alt query as a separate <see cref="SqlBuilder"/>.</param>
        /// <param name="all">The flag that indicates whether to union all.</param>
        public virtual void RegisterIntersectQuery(Func<SqlBuilder, string> query, bool all = false)
        {
            RegisterCombinedQuery("Intersect", query, all);
        }

        /// <summary>
        ///     Registers an <c>EXCEPT</c> statement with the specified <paramref name="query"/>.
        /// </summary>
        /// <param name="query">The alt query as a separate <see cref="SqlBuilder"/>.</param>
        /// <param name="all">The flag that indicates whether to union all.</param>
        public virtual void RegisterExceptQuery(Func<SqlBuilder, string> query, bool all = false)
        {
            RegisterCombinedQuery("Except", query, all);
        }

        /// <summary>
        ///     Registers a combined statement with the specified <paramref name="query"/> based on the given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type of the query; <c>UNION, INTERSECT, or EXCEPT</c>.</param>
        /// <param name="query">The alt query as a separate <see cref="SqlBuilder"/>.</param>
        /// <param name="all">The flag that indicates whether to union all.</param>
        protected virtual void RegisterCombinedQuery(string type, Func<SqlBuilder, string> query, bool all = false)
        {
            Check.NotNullOrEmpty(type, nameof(type));
            Check.NotNull(query, nameof(query));

            var result = query.Invoke(new SqlBuilder(_sqlStyle));
            if (string.IsNullOrEmpty(result))
                throw new InvalidOperationException("The query result cannot be null or empty");

            var builder = GetOrCreateBuilder("combine_queries");
            builder.Append('\n' + type + ' ');

            if (all)
                builder.Append("all ");

            builder.Append('\n' + result);
        }

        /// <summary>
        ///     Registers a "returning" statement.
        /// </summary>
        /// <param name="fields">The fields to be returned.</param>
        public virtual void RegisterReturningFields(params string[] fields)
        {
            var result = "\nreturning ";

            if (fields.Length == 0)
                result += "*";
            else
                for (var i = 0; i < fields.Length; i++)
                {
                    _enc.Wrap(ref fields[i]);
                    result += fields[i];

                    if (i != fields.Length - 1)
                        result += ", ";
                }

            GetOrCreateBuilder("returning").Append(result);
        }

        /// <summary>
        ///     Registers the fields for the conflict statement.
        /// </summary>
        /// <param name="keys">The conflict fields.</param>
        public virtual void RegisterConflictFields(params string[] keys)
        {
            var builder = _builders["conflict"] = new StringBuilder();

            for (var i = 0; i < keys.Length; i++)
            {
                _enc.Wrap(ref keys[i]);
                builder.Append(keys[i]);

                if (i != keys.Length - 1)
                    builder.Append(", ");
            }
        }

        /// <summary>
        ///     Gets and returns the content of the builder with the specified <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the builder to return from.</param>
        /// <returns>The content of the builder with the specified <paramref name="name"/>.</returns>
        protected virtual string GetBuilderContent(string name)
        {
            return _builders.TryGetValue(name, out var builder) ? builder.ToString() : null;
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

            result.Append("select ");
            result.AppendLine(GetBuilderContent("fields") ?? "*");

            if (!string.IsNullOrEmpty(TableName))
                result.Append("from " + TableName);

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
            return GenerateTemplate(new[] { "update ", TableName, " set ", "@pairs", "@where", "@returning" });
        }

        /// <summary>
        ///     Generates and returns a <c>DELETE</c> statement from the specified configurations.
        /// </summary>
        /// <returns>A <c>DELETE</c> statement.</returns>
        public virtual string ToDelete()
        {
            var result = new StringBuilder();

            var withQueries = GetBuilderContent("with_queries");
            if (withQueries != null)
                result.AppendLine(withQueries);

            result.Append("delete from " + TableName);
            result.Append(GetBuilderContent("where"));
            result.Append(GetBuilderContent("returning"));
            return result.ToString();
        }

        /// <summary>
        ///     Generates and returns an <c>iNSERT</c> statement from the specified configurations.
        /// </summary>
        /// <returns>A <c>iNSERT</c> statement.</returns>
        public virtual string ToInsert()
        {
            return GenerateTemplate(new[] { "insert into ", TableName, " (", "@fields", ")\nvalues ", "@values", "@where", "@returning" });
        }

        /// <summary>
        ///     Generates and returns an <c>INSERT ON CONFLICT</c> statement from the specified configurations.
        /// </summary>
        /// <returns>A <c>INSERT ON CONFLICT</c> statement.</returns>
        public virtual string ToInsertOrUpdate()
        {
            return GenerateTemplate(new[]
            {
                "insert into ", TableName, " (", "@fields", ")\nvalues ", "@values", "@where", "\non conflict (", "@conflict", ") do\n", "update set ", "@pairs"
            });
        }

        /// <summary>
        ///     Generates a statement that's based on the specified <paramref name="template"/> considering the previously configured options.
        /// </summary>
        /// <param name="template">The rules to follow regarding the process of generating the SQL statement.</param>
        /// <returns>The generated statement.</returns>
        public virtual string GenerateTemplate(string[] template)
        {
            var result = new StringBuilder();
            foreach (var val in template)
            {
                if (val[0] != '@')
                    result.Append(val);

                else if (_builders.TryGetValue(val[1..], out var builder))
                    result.Append(builder.ToString());
            }
            return result.ToString();
        }

        /// <summary>
        ///     Generates a statement that's based on the specified <paramref name="template"/> considering the previously configured options.
        /// </summary>
        /// <param name="template">The rules to follow regarding the process of generating the SQL statement.</param>
        /// <param name="parts">The parts to be inserted into the statement according to the template.</param>
        /// <returns>The generated statement.</returns>
        public virtual string GenerateTemplate(string template, params string[] parts)
        {
            for (var i = 0; i < parts.Length; i++)
            {
                var value = _builders.TryGetValue(parts[i], out var builder) ? builder.ToString() : null;
                template = template.Replace("{" + i + "}", value);
            }
            return template;
        }

        /// <summary>
        ///     Clears the temporary configurations to start a fresh build.
        /// </summary>
        public virtual void Clear()
        {
            _builders.Clear();
        }
    }
}
