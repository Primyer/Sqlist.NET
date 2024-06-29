using Sqlist.NET.Sql.Constraints;
using Sqlist.NET.Sql.Metadata;

using System;
using System.Linq;
using System.Text;

namespace Sqlist.NET.Sql;

/// <summary>
///     Initializes a new instance of the <see cref="NpgsqlSchemaBuilder"/> class.
/// </summary>
/// <param name="encloser">The appopertiate <see cref="Encloser"/> implementation according to the target DBMS.</param>
internal class NpgsqlSchemaBuilder(Encloser encloser) : ISchemaBuilder
{
    const string Tab = "    ";

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
            builder.Append(encloser.Replace(table.Schema));
            builder.Append('.');
        }

        builder.Append(encloser.Wrap(table.Name));
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
        return $"DROP TABLE {encloser.Wrap(table)};";
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
                    return encloser.Wrap(col.Name);
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
        builder.Append(encloser.Wrap(column.Name) + " " + column.Type);

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
            builder.Append(encloser.Replace(column.Check.ToString()));
        }
    }

    protected virtual void AppendPrimaryKey(StringBuilder builder, PrimaryKeyConstraint constraint)
    {
        builder.Append(Tab);

        if (!string.IsNullOrEmpty(constraint.Name))
        {
            builder.Append(Tab);
            builder.Append("CONSTRAINT ");
            builder.Append(encloser.Wrap(constraint.Name) + " ");
        }

        builder.Append("PRIMARY KEY (");
        builder.Append(encloser.Join(", ", constraint.Columns));
        builder.Append(')');
    }

    protected virtual void AppendForeignKey(StringBuilder builder, ForeignKeyConstraint constraint)
    {
        builder.Append(Tab);

        if (!string.IsNullOrEmpty(constraint.Name))
        {
            builder.Append(Tab);
            builder.Append("CONSTRAINT ");
            builder.Append(encloser.Wrap(constraint.Name) + " ");
        }

        builder.Append("FOREIGN KEY (");
        builder.Append(encloser.Join(", ", constraint.Columns));
        builder.AppendLine(")");

        builder.Append(Tab);
        builder.Append("REFERENCES ");
        builder.Append(encloser.Wrap(constraint.References.Table));
        builder.Append(" (");
        builder.Append(encloser.Join(", ", constraint.References.Columns));
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
            builder.Append(encloser.Wrap(constraint.Name) + " ");
        }

        builder.Append("UNIQUE (");
        builder.Append(encloser.Join(", ", constraint.Columns));
        builder.Append(')');
    }

    protected virtual void AppendCheck(StringBuilder builder, CheckConstraint constraint)
    {
        builder.Append(Tab);

        if (!string.IsNullOrEmpty(constraint.Name))
        {
            builder.Append(Tab);
            builder.Append("CONSTRAINT ");
            builder.Append(encloser.Wrap(constraint.Name) + " ");
        }

        builder.Append("CHECK (");
        builder.Append(encloser.Reformat(constraint.Condition?.ToString()));
        builder.Append(')');
    }

    /// <summary>
    ///     Generates and returns a <c>CRAETE DATABASE</c> statement for the specified <paramref name="database"/>.
    /// </summary>
    /// <param name="database">The name of the database to be created.</param>
    /// <returns>A <c>CREATE DATABASE</c> statement for the specified <paramref name="database"/>.</returns>
    public virtual string CreateDatabase(string database)
    {
        var builder = new StringBuilder("CREATE DATABASE ");

        builder.Append(encloser.Wrap(database));
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

        builder.Append(encloser.Wrap(database));
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
        var builder = new StringBuilder("ALTER DATABASE ");

        builder.Append(encloser.Wrap(currentName));
        builder.Append(" RENAME TO ");
        builder.Append(encloser.Wrap(newName));
        builder.Append(';');

        return builder.ToString();
    }
}
