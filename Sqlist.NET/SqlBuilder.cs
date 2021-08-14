using Sqlist.NET.Utilities;

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

        public virtual void RegisterUpdate(string[] fields)
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
        ///     Generates and returns a <c>SELECT</c> statement from the specified configurations.
        /// </summary>
        /// <returns>A <c>SELECT</c> statement.</returns>
        public virtual string ToSelect()
        {
            return GenerateTemplate(new[] { "@with_queries", "\nselect ", "@fields", "\nfrom ", TableName, "@joins", "@where", "@filters", "@combine_queries" });
        }

        /// <summary>
        ///     Generates and returns an <c>UPDATE</c> statement from the specified configurations.
        /// </summary>
        /// <returns>A <c>UPDATE</c> statement.</returns>
        public virtual string ToUpdate()
        {
            return GenerateTemplate(new[] { "update", TableName, "set", "@pairs", "@where" });
        }

        /// <summary>
        ///     Generates and returns a <c>DELETE</c> statement from the specified configurations.
        /// </summary>
        /// <returns>A <c>DELETE</c> statement.</returns>
        public virtual string ToDelete()
        {
            return GenerateTemplate(new[] { "delete from", TableName, "@where" });
        }

        /// <summary>
        ///     Generates and returns an <c>iNSERT</c> statement from the specified configurations.
        /// </summary>
        /// <returns>A <c>iNSERT</c> statement.</returns>
        public virtual string ToInsert()
        {
            return GenerateTemplate(new[] { "insert into ", TableName, " (", "@fields", ")\nvalues ", "@values", "where" });
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
