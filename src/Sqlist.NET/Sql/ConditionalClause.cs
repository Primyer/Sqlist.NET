using System.Text;

namespace Sqlist.NET.Sql;
public class ConditionalClause : IConditionalClause
{
    private readonly StringBuilder _builder = new();

    public IConditionalClause Init(string content)
    {
        _builder.Append(content);
        return this;
    }

    public IConditionalClause Init(Action<IConditionalClause> inner)
    {
        Inner(inner);
        return this;
    }

    public IConditionalClause Not(string content)
    {
        _builder.Append("NOT ");
        _builder.Append(content);

        return this;
    }

    public IConditionalClause Not(Action<IConditionalClause> inner)
    {
        Inner("NOT ", inner);
        return this;
    }

    #region AND
    public IConditionalClause And(string content) => Content("AND", content);
    public IConditionalClause And(Action<IConditionalClause> inner) => Inner("AND", inner);
    #endregion

    #region OR
    public IConditionalClause Or(string content) => Content("OR", content);
    public IConditionalClause Or(Action<IConditionalClause> inner) => Inner("OR", inner);
    #endregion

    #region AND NOT
    public IConditionalClause AndNot(string content) => Content("AND NOT", content);
    public IConditionalClause AndNot(Action<IConditionalClause> inner) => Inner("AND NOT", inner);
    #endregion

    #region OR NOT
    public IConditionalClause OrNot(string content) => Content("OR NOT", content);
    public IConditionalClause OrNot(Action<IConditionalClause> inner) => Inner("OR NOT", inner);
    #endregion

    public IConditionalClause IsNull(string content)
    {
        _builder.Append(content);
        _builder.Append(" IS NULL");

        return this;
    }

    public IConditionalClause AndIsNull(string content)
    {
        _builder.Append(" AND ");
        return IsNull(content);
    }

    public IConditionalClause OrIsNull(string content)
    {
        _builder.Append(" OR ");
        return IsNull(content);
    }

    public IConditionalClause NotNull(string content)
    {
        _builder.Append(content);
        _builder.Append(" IS NOT NULL");

        return this;
    }

    public IConditionalClause AndNotNull(string content)
    {
        _builder.Append(" AND ");
        return NotNull(content);
    }

    public IConditionalClause OrNotNull(string content)
    {
        _builder.Append(" OR ");
        return NotNull(content);
    }

    #region IN

    public IConditionalClause In(string content)
    {
        _builder.Append("IN (");
        _builder.Append(content);
        _builder.Append(" )");

        return this;
    }

    public IConditionalClause In(Action<IConditionalClause> inner)
    {
        _builder.Append("IN (");
        inner.Invoke(this);
        _builder.Append(" )");

        return this;
    }

    public IConditionalClause In(Action<ISqlBuilder> select)
    {
        var sql = new SqlBuilder();
        select.Invoke(sql);

        return In(sql.ToSelect());
    }

    #endregion

    #region NOT IN

    public IConditionalClause NotIn(string content)
    {
        _builder.Append(" NOT ");
        return In(content);
    }

    public IConditionalClause NotIn(Action<IConditionalClause> inner)
    {
        _builder.Append(" NOT ");
        return In(inner);
    }

    public IConditionalClause NotIn(Action<ISqlBuilder> select)
    {
        _builder.Append(" NOT ");
        return In(select);
    }

    #endregion

    #region AND IN

    public IConditionalClause AndIn(string content)
    {
        _builder.Append(" AND ");
        return In(content);
    }

    public IConditionalClause AndIn(Action<IConditionalClause> inner)
    {
        _builder.Append(" AND ");
        return In(inner);
    }

    public IConditionalClause AndIn(Action<ISqlBuilder> select)
    {
        _builder.Append(" AND ");
        return In(select);
    }

    #endregion

    #region OR IN

    public IConditionalClause OrIn(string content)
    {
        _builder.Append(" OR ");
        return In(content);
    }

    public IConditionalClause OrIn(Action<IConditionalClause> inner)
    {
        _builder.Append(" OR ");
        return In(inner);
    }

    public IConditionalClause OrIn(Action<ISqlBuilder> select)
    {
        _builder.Append(" OR ");
        return In(select);
    }

    #endregion

    #region AND NOT IN

    public IConditionalClause AndNotIn(string content)
    {
        _builder.Append(" AND NOT ");
        return In(content);
    }

    public IConditionalClause AndNotIn(Action<IConditionalClause> inner)
    {
        _builder.Append(" AND NOT ");
        return In(inner);
    }

    public IConditionalClause AndNotIn(Action<ISqlBuilder> select)
    {
        _builder.Append(" AND NOT ");
        return In(select);
    }

    #endregion

    #region OR NOT IN

    public IConditionalClause OrNotIn(string content)
    {
        _builder.Append(" OR NOT ");
        return In(content);
    }

    public IConditionalClause OrNotIn(Action<IConditionalClause> inner)
    {
        _builder.Append(" OR NOT ");
        return In(inner);
    }

    public IConditionalClause OrNotIn(Action<ISqlBuilder> select)
    {
        _builder.Append(" OR NOT ");
        return In(select);
    }

    #endregion

    private ConditionalClause Content(string @operator, string content)
    {
        _builder.Append(' ');
        _builder.Append(@operator);
        _builder.Append(' ');
        _builder.Append(content);

        return this;
    }

    private ConditionalClause Inner(Action<IConditionalClause> inner)
    {
        _builder.Append('(');
        inner.Invoke(this);
        _builder.Append(')');

        return this;
    }

    private ConditionalClause Inner(string @operator, Action<IConditionalClause> inner)
    {
        _builder.Append(' ');
        _builder.Append(@operator);
        _builder.Append(" (");
        inner.Invoke(this);
        _builder.Append(')');

        return this;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return _builder.ToString();
    }
}