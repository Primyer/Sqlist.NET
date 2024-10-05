namespace Sqlist.NET.Sql;
public interface ISqlBuilder
{
    Enclosure Enclosure { get; set; }
    string? TableName { get; set; }

    void AppendAnd(string condition);
    void AppendAndNot(string condition);
    void AppendCondition(string condition);
    void AppendOr(string condition);
    void AppendOrNot(string condition);

    string Cast(string field, string type);

    void Clear();
    void ClearFields();

    void From(string entity);
    void Where(Action<IConditionalClause> condition);
    void Where(string condition);
    void WhereNotExists(Action<ISqlBuilder> sql);
    void WhereNotExists(string stmt);

    void GroupBy(string field);
    void GroupBy(string[] fields);
    void Having(string condition);
    void Limit(string value);
    void Offset(string value);
    void OrderBy(string field);
    void OrderBy(string[] fields);

    void RegisterBulkValues(int cols, int rows, int gapSize = 0, Action<int, string[]>? configure = null);
    void RegisterExceptQuery(Func<ISqlBuilder, string> query, bool all = false);
    void RegisterFields(string field, bool reformat = true);
    void RegisterFields(string[] fields);
    void RegisterIntersectQuery(Func<ISqlBuilder, string> query, bool all = false);
    void RegisterPairFields((string, string) pair);
    void RegisterPairFields(Dictionary<string, string> pairs);
    void RegisterPairFields(string[] fields);
    void RegisterReturningFields(params string[] fields);
    void RegisterUnionDistinctQuery(Func<ISqlBuilder, string> query);
    void RegisterUnionQuery(Func<ISqlBuilder, string> query, bool all = false);
    void RegisterValues(object[] row);
    void RegisterValues(object[][] rows);

    void InnerJoin(string table, string? condition = null);
    void InnerJoin(string table, Action<IConditionalClause> condition);
    void InnerJoin(string alias, string condition, Action<ISqlBuilder> configureSql);
    void RightJoin(string table, string? condition = null);
    void RightJoin(string table, Action<IConditionalClause> condition);
    void RightJoin(string alias, string condition, Action<ISqlBuilder> configureSql);
    void LeftJoin(string table, string? condition = null);
    void LeftJoin(string table, Action<IConditionalClause> condition);
    void LeftJoin(string alias, string condition, Action<ISqlBuilder> configureSql);
    void FullJoin(string table, string? condition = null);
    void FullJoin(string alias, string condition, Action<ISqlBuilder> configureSql);
    void Join(string stmt);
    void Join(string type, string entity, string? condition = null);
    void Join(string type, string entity, Action<IConditionalClause> condition);
    void Join(string type, string alias, string condition, Action<ISqlBuilder> configureSql);

    void Window(string alias, string content);

    void With(string name, Func<ISqlBuilder, string> body);
    void With(string name, string[] fields, Func<ISqlBuilder, string> body);
    void RecursiveWith(string name, Func<ISqlBuilder, string> body);
    void RecursiveWith(string name, string[] fields, Func<ISqlBuilder, string> body);
    void WithData(string name, string[] fields, int rows);
    void RegisterWith(string name, string[] fields, string body, bool recursive = false);

    string ToBulkUpdate(string alias);
    string ToDelete(bool cascade = false);
    string ToInsert(Action<ISqlBuilder>? select = null);
    string ToSelect();
    string ToUpdate();
}
