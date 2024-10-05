using System;
using System.Linq;
using System.Text;

using Sqlist.NET.Sql.Constraints;
using Sqlist.NET.Sql.Metadata;

namespace Sqlist.NET.Sql;

/// <summary>
///     Initializes a new instance of the <see cref="NpgsqlSchemaBuilder"/> class.
/// </summary>
/// <param name="enclosure">The appropriate <see cref="Enclosure"/> implementation according to the target DBMS.</param>
internal class NpgsqlSchemaBuilder(Enclosure enclosure) : ISchemaBuilder
{
    private const string Tab = "    ";

    /// <summary>
    ///     Generates and returns a <c>CREATE TABLE</c> statement out of the given <paramref name="table"/>.
    /// </summary>
    /// <param name="table">The table information.</param>
    /// <returns>A <c>CREATE TABLE</c> statement.</returns>
    public string CreateTable(SqlTable table)
    {
        var builder = new StringBuilder("CREATE TABLE ");

        if (table.IfNotExists)
            builder.Append("IF NOT EXISTS ");

        if (!string.IsNullOrEmpty(table.Schema))
        {
            builder.Append(enclosure.Replace(table.Schema));
            builder.Append('.');
        }

        builder.Append(enclosure.Wrap(table.Name));
        builder.AppendLine(" (");

        ConfigureTablePrimaryKey(table);

        var length = table.Columns.Count;
        var aiCount = 0;
        
        for (var i = 0; i < length; i++)
        {
            var column = table.Columns[i];
            if (column.AutoIncrement)
            {
                if (aiCount > 1)
                    throw new InvalidOperationException("Only one column can be auto-incremented.");

                if (column.Type != "int" && column.Type != "bigint")
                    throw new InvalidOperationException("Auto-incremented column must be of type int or bigint.");
                
                aiCount++;
            }
            
            column.Type = GetSequentialType(column.Type);
            AppendColumn(builder, column);
            
            builder.AppendLine(i != length - 1 ? "," : null);
        }

        AppendTableConstraints(builder, table.Constraints);

        builder.Append(");");
        return builder.ToString();
    }
    
    private static string GetSequentialType(string type) => type switch
    {
        "smallint" => "SMALLSERIAL",
        "int" => "SERIAL",
        "bigint" => "BIGSERIAL",
        _ => throw new InvalidOperationException("Auto-incremented column must be of type smallint, int, or bigint.")
    };

    /// <summary>
    ///     Generates and returns a <c>CREATE TABLE</c> statement out of the given <paramref name="table"/>.
    /// </summary>
    /// <param name="table">The table information.</param>
    /// <returns>A <c>CREATE TABLE</c> statement.</returns>
    public string DeleteTable(string table)
    {
        return $"DROP TABLE {enclosure.Wrap(table)};";
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
                    return enclosure.Wrap(col.Name);
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

        if (builder.Length == 0) return;
        
        tableBuilder.AppendLine(",");
        tableBuilder.Append(builder);
    }

    private void AppendColumn(StringBuilder builder, SqlColumn column)
    {
        builder.Append(Tab);
        builder.Append(enclosure.Wrap(column.Name) + " " + column.Type);
        
        if (column.NotNull)
            builder.Append(" NOT NULL");

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
            builder.Append(enclosure.Replace(column.Check.ToString()));
        }
    }

    private void AppendPrimaryKey(StringBuilder builder, PrimaryKeyConstraint constraint)
    {
        builder.Append(Tab);

        if (!string.IsNullOrEmpty(constraint.Name))
        {
            builder.Append(Tab);
            builder.Append("CONSTRAINT ");
            builder.Append(enclosure.Wrap(constraint.Name) + " ");
        }

        builder.Append("PRIMARY KEY (");
        builder.Append(enclosure.Join(", ", constraint.Columns));
        builder.Append(')');
    }

    private void AppendForeignKey(StringBuilder builder, ForeignKeyConstraint constraint)
    {
        builder.Append(Tab);

        if (!string.IsNullOrEmpty(constraint.Name))
        {
            builder.Append(Tab);
            builder.Append("CONSTRAINT ");
            builder.Append(enclosure.Wrap(constraint.Name) + " ");
        }

        builder.Append("FOREIGN KEY (");
        builder.Append(enclosure.Join(", ", constraint.Columns));
        builder.AppendLine(")");

        builder.Append(Tab);
        builder.Append("REFERENCES ");
        builder.Append(enclosure.Wrap(constraint.References.Table));
        builder.Append(" (");
        builder.Append(enclosure.Join(", ", constraint.References.Columns));
        builder.AppendLine(")");

        builder.Append(Tab);
        builder.Append("ON UPDATE ");
        builder.AppendLine(GetReferentialAction(constraint.OnUpdate));

        builder.Append(Tab);
        builder.Append("ON DELETE ");
        builder.Append(GetReferentialAction(constraint.OnDelete));
    }

    private static string GetReferentialAction(ReferencialAction action) => action switch
    {
        ReferencialAction.Restrict => "RESTRICT",
        ReferencialAction.Cascade => "CASCADE",
        ReferencialAction.SetDefault => "SET DEFAULT",
        ReferencialAction.SetNull => "SET NULL",
        _ => "NO ACTION"
    };

    private void AppendUnique(StringBuilder builder, UniqueConstraint constraint)
    {
        builder.Append(Tab);

        if (!string.IsNullOrEmpty(constraint.Name))
        {
            builder.Append(Tab);
            builder.Append("CONSTRAINT ");
            builder.Append(enclosure.Wrap(constraint.Name) + " ");
        }

        builder.Append("UNIQUE (");
        builder.Append(enclosure.Join(", ", constraint.Columns));
        builder.Append(')');
    }

    private void AppendCheck(StringBuilder builder, CheckConstraint constraint)
    {
        builder.Append(Tab);

        if (!string.IsNullOrEmpty(constraint.Name))
        {
            builder.Append(Tab);
            builder.Append("CONSTRAINT ");
            builder.Append(enclosure.Wrap(constraint.Name) + " ");
        }

        builder.Append("CHECK (");
        builder.Append(enclosure.Reformat(constraint.Condition?.ToString()));
        builder.Append(')');
    }

    /// <summary>
    ///     Generates and returns a <c>CREATE DATABASE</c> statement for the specified <paramref name="database"/>.
    /// </summary>
    /// <param name="database">The name of the database to be created.</param>
    /// <returns>A <c>CREATE DATABASE</c> statement for the specified <paramref name="database"/>.</returns>
    public string CreateDatabase(string database)
    {
        var builder = new StringBuilder("CREATE DATABASE ");

        builder.Append(enclosure.Wrap(database));
        builder.Append(';');

        return builder.ToString();
    }

    /// <summary>
    ///     Generates and returns a <c>DELETE DATABASE</c> statement for the specified <paramref name="database"/>.
    /// </summary>
    /// <param name="database">The name of the database to be created.</param>
    /// <returns>A <c>DELETE DATABASE</c> statement for the specified <paramref name="database"/>.</returns>
    public string DeleteDatabase(string database)
    {
        var builder = new StringBuilder("DROP DATABASE ");

        builder.Append(enclosure.Wrap(database));
        builder.Append(';');

        return builder.ToString();
    }

    /// <summary>
    ///     Alters the <paramref name="currentName"/> of the database to be changed to the <paramref name="newName"/>.
    /// </summary>
    /// <param name="currentName">The name of the database to be renamed.</param>
    /// <param name="newName">The new name of the database.</param>
    /// <returns>The SQL statement to rename the database.</returns>
    public string RenameDatabase(string currentName, string newName)
    {
        var builder = new StringBuilder("ALTER DATABASE ");

        builder.Append(enclosure.Wrap(currentName));
        builder.Append(" RENAME TO ");
        builder.Append(enclosure.Wrap(newName));
        builder.Append(';');

        return builder.ToString();
    }
}
