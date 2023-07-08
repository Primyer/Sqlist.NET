using Sqlist.NET.Metadata;

using System;
using System.Text;

namespace Sqlist.NET.Sql
{
    public class NpgsqlBuilder : SqlBuilder
    {
        /// <summary>
        ///     Initalizes a new instance of the <see cref="NpgsqlBuilder"/> class.
        /// </summary>
        public NpgsqlBuilder() : base(new NpgsqlEncloser())
        {
        }

        /// <summary>
        ///     Initalizes a new instance of the <see cref="NpgsqlBuilder"/> class.
        /// </summary>
        /// <param name="encloser">The appopertiate <see cref="Encloser"/> implementation according to the target DBMS.</param>
        public NpgsqlBuilder(Encloser? encloser) : base(encloser)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="NpgsqlBuilder"/> class.
        /// </summary>
        /// <param name="table">The name of table to base the statement on.</param>
        public NpgsqlBuilder(string table) : base(new NpgsqlEncloser(), table)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="NpgsqlBuilder"/> class.
        /// </summary>
        /// <param name="schema">The name of the schema where the table exists.</param>
        /// <param name="table">The name of table to base the statement on.</param>
        public NpgsqlBuilder(string? schema, string table) : base(new NpgsqlEncloser(), schema, table)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="NpgsqlBuilder"/> class.
        /// </summary>
        /// <param name="encloser">The appopertiate <see cref="Sql.Encloser"/> implementation according to the target DBMS.</param>
        /// <param name="schema">The name of the schema where the table exists.</param>
        /// <param name="table">The name of table to base the statement on.</param>
        public NpgsqlBuilder(Encloser? encloser, string? schema, string table) : base(encloser ?? new NpgsqlEncloser(), schema, table)
        {
        }

        /// <summary>
        ///     Registers an <c>CROSS JOIN</c> on the specified <paramref name="table"/>.
        /// </summary>
        /// <param name="table">The table to join.</param>
        public void CrossJoin(string table)
        {
            Join($"CROSS JOIN " + (table.IndexOf(' ') != -1 ? Encloser.Replace(table) : Encloser.Wrap(table)));
        }

        /// <summary>
        ///     Registers an <c>CROSS JOIN</c> of the specified on the result of the <paramref name="configureSql"/>.
        /// </summary>
        /// <param name="alias">The alias of the subquery.</param>
        /// <param name="configureSql">The action to build the partial statement of the join.</param>
        public void CrossJoin(string alias, Action<SqlBuilder> configureSql)
        {
            var sql = new NpgsqlBuilder();
            configureSql.Invoke(sql);

            var stmt = Indent(sql.ToSelect());

            CrossJoin($"(\n{stmt}) AS {alias}");
        }

        public void RegisterConflictConstraint(string constraint)
        {
            Builders["conflict"] = new StringBuilder($"ON CONSTRAINT \"{constraint}\"");
        }

        /// <summary>
        ///     Registers the fields for the conflict statement.
        /// </summary>
        /// <param name="keys">The conflict fields.</param>
        public void RegisterConflictFields(params string[] keys)
        {
            var builder = Builders["conflict"] = new StringBuilder("(");

            for (var i = 0; i < keys.Length; i++)
            {
                builder.Append(Encloser.Wrap(keys[i]));

                if (i != keys.Length - 1)
                    builder.Append(", ");
            }
            builder.Append(")");
        }

        /// <summary>
        ///     Generates and returns an <c>INSERT ON CONFLICT</c> statement from the specified configurations.
        /// </summary>
        /// <returns>An <c>INSERT ON CONFLICT</c> statement.</returns>
        public string ToInsertOrUpdate()
        {
            var result = new StringBuilder();

            var withQueries = GetBuilderContent("with_queries");
            if (withQueries != null)
                result.AppendLine(withQueries);

            result.Append($"INSERT INTO {TableName} (");
            result.Append(GetBuilderContent("fields"));
            result.Append(")\nVALUES ");
            result.Append(GetBuilderContent("values"));
            result.AppendLine(GetBuilderContent("where"));
            result.Append("ON CONFLICT ");
            result.Append(GetBuilderContent("conflict"));

            var pairs = GetBuilderContent("pairs");
            if (pairs != null)
            {
                result.Append(" DO UPDATE SET ");
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
        ///     Generates and returns a <c>COPY TO</c> statement from the specified configurations.
        /// </summary>
        /// <param name="source">The copy destination.</param>
        /// <param name="options">The configation options of the copy operation.</param>
        /// <param name="query">
        ///     A SELECT, VALUES, INSERT, UPDATE, or DELETE command whose results are to be copied.
        ///     If none specifed, table columns are assumed.
        /// </param>
        /// <returns>A <c>COPY TO</c> statement.</returns>
        public string ToCopyTo(CopySource source, CopyOptions? options = null, Func<NpgsqlBuilder, string>? query = null)
        {
            return ToCopy("TO", source, options, query);
        }

        /// <summary>
        ///     Generates and returns a <c>COPY FROM</c> statement from the specified configurations.
        /// </summary>
        /// <param name="source">The copy source.</param>
        /// <param name="options">The configation options of the copy operation.</param>
        /// <param name="query">
        ///     A SELECT, VALUES, INSERT, UPDATE, or DELETE command whose results are to be copied.
        ///     If none specifed, table columns are assumed.
        /// </param>
        /// <returns>A <c>COPY FROM</c> statement.</returns>
        public string ToCopyFrom(CopySource source, CopyOptions? options = null, Func<NpgsqlBuilder, string>? query = null)
        {
            return ToCopy("FROM", source, options, query);
        }

        private string ToCopy(string direction, CopySource source, CopyOptions? options, Func<NpgsqlBuilder, string>? query)
        {
            var result = new StringBuilder("COPY ");


            if (query is null)
            {
                result.Append(TableName);
                result.Append(" (");
                result.Append(GetBuilderContent("fields"));
            }
            else
            {
                result.AppendLine("(");
                Indent(query(new NpgsqlBuilder()), result);
            }

            result.AppendLine(")");
            result.Append(direction + " ");
            result.AppendLine(source.ToString());

            if (options != null)
            {
                result.AppendLine("WITH (");
                Indent(options.ToString(), result);
                result.Append(")");
            }

            return result.ToString();
        }

        /// <inheritdoc />
        public override string RenameDatabase(string currentName, string newName)
        {
            var builder = new StringBuilder("ALTER DATABASE ");

            builder.Append(Encloser.Wrap(currentName));
            builder.Append(" RENAME TO ");
            builder.Append(Encloser.Wrap(newName));
            builder.Append(";");

            return builder.ToString();
        }
    }
}
