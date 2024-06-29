namespace Sqlist.NET.Sql;
public interface ISchemaBuilder
{
    string CreateDatabase(string database);
    string CreateTable(SqlTable table);
    string DeleteDatabase(string database);
    string DeleteTable(string table);
    string RenameDatabase(string currentName, string newName);
}
