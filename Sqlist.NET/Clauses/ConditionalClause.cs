using System;
using System.Text;

namespace Sqlist.NET.Clauses
{
    public class ConditionalClause
    {
        private readonly StringBuilder _builder = new StringBuilder();

        public void Init(string content)
        {
            _builder.Append(content);
        }

        public void Init(Action<ConditionalClause> inner) => Inner(inner);

        public void Not(string content)
        {
            _builder.Append("NOT ");
            _builder.Append(content);
        }

        public void Not(Action<ConditionalClause> inner) => Inner("NOT ", inner);

        #region AND
        public ConditionalClause And(string content) => Content("AND", content);
        public ConditionalClause And(Action<ConditionalClause> inner) => Inner("AND", inner);
        #endregion

        #region OR
        public ConditionalClause Or(string content) => Content("OR", content);
        public ConditionalClause Or(Action<ConditionalClause> inner) => Inner("OR", inner);
        #endregion

        #region AND NOT
        public ConditionalClause AndNot(string content) => Content("AND NOT", content);
        public ConditionalClause AndNot(Action<ConditionalClause> inner) => Inner("AND NOT", inner);
        #endregion

        #region OR NOT
        public ConditionalClause OrNot(string content) => Content("OR NOT", content);
        public ConditionalClause OrNot(Action<ConditionalClause> inner) => Inner("OR NOT", inner);
        #endregion

        public ConditionalClause IsNull(string content)
        {
            _builder.Append(content);
            _builder.Append(" IS NULL");

            return this;
        }

        public ConditionalClause AndIsNull(string content)
        {
            _builder.Append(" AND ");
            return IsNull(content);
        }

        public ConditionalClause OrIsNull(string content)
        {
            _builder.Append(" OR ");
            return IsNull(content);
        }

        public ConditionalClause NotNull(string content)
        {
            _builder.Append(content);
            _builder.Append(" IS NOT NULL");

            return this;
        }

        public ConditionalClause AndNotNull(string content)
        {
            _builder.Append(" AND ");
            return NotNull(content);
        }

        public ConditionalClause OrNotNull(string content)
        {
            _builder.Append(" OR ");
            return NotNull(content);
        }

        #region IN

        public ConditionalClause In(string content)
        {
            _builder.Append("IN (");
            _builder.Append(content);
            _builder.Append(" )");

            return this;
        }

        public ConditionalClause In(Action<ConditionalClause> inner)
        {
            _builder.Append("IN (");
            inner.Invoke(this);
            _builder.Append(" )");

            return this;
        }

        public ConditionalClause In(Action<SqlBuilder> select)
        {
            var sql = new SqlBuilder();
            select.Invoke(sql);

            return In(sql.ToSelect());
        }

        #endregion

        #region NOT IN

        public ConditionalClause NotIn(string content)
        {
            _builder.Append(" NOT ");
            return In(content);
        }

        public ConditionalClause NotIn(Action<ConditionalClause> inner)
        {
            _builder.Append(" NOT ");
            return In(inner);
        }

        public ConditionalClause NotIn(Action<SqlBuilder> select)
        {
            _builder.Append(" NOT ");
            return In(select);
        }

        #endregion

        #region AND IN

        public ConditionalClause AndIn(string content)
        {
            _builder.Append(" AND ");
            return In(content);
        }

        public ConditionalClause AndIn(Action<ConditionalClause> inner)
        {
            _builder.Append(" AND ");
            return In(inner);
        }

        public ConditionalClause AndIn(Action<SqlBuilder> select)
        {
            _builder.Append(" AND ");
            return In(select);
        }

        #endregion

        #region OR IN

        public ConditionalClause OrIn(string content)
        {
            _builder.Append(" OR ");
            return In(content);
        }

        public ConditionalClause OrIn(Action<ConditionalClause> inner)
        {
            _builder.Append(" OR ");
            return In(inner);
        }

        public ConditionalClause OrIn(Action<SqlBuilder> select)
        {
            _builder.Append(" OR ");
            return In(select);
        }

        #endregion

        #region AND NOT IN

        public ConditionalClause AndNotIn(string content)
        {
            _builder.Append(" AND NOT ");
            return In(content);
        }

        public ConditionalClause AndNotIn(Action<ConditionalClause> inner)
        {
            _builder.Append(" AND NOT ");
            return In(inner);
        }

        public ConditionalClause AndNotIn(Action<SqlBuilder> select)
        {
            _builder.Append(" AND NOT ");
            return In(select);
        }

        #endregion

        #region OR NOT IN

        public ConditionalClause OrNotIn(string content)
        {
            _builder.Append(" OR NOT ");
            return In(content);
        }

        public ConditionalClause OrNotIn(Action<ConditionalClause> inner)
        {
            _builder.Append(" OR NOT ");
            return In(inner);
        }

        public ConditionalClause OrNotIn(Action<SqlBuilder> select)
        {
            _builder.Append(" OR NOT ");
            return In(select);
        }

        #endregion

        private ConditionalClause Content(string @operator, string content)
        {
            _builder.Append(' ');
            _builder.Append(@operator);
            _builder.Append(" ");
            _builder.Append(content);

            return this;
        }

        private ConditionalClause Inner(Action<ConditionalClause> inner)
        {
            _builder.Append("(");
            inner.Invoke(this);
            _builder.Append(")");

            return this;
        }

        private ConditionalClause Inner(string @operator, Action<ConditionalClause> inner)
        {
            _builder.Append(' ');
            _builder.Append(@operator);
            _builder.Append(" (");
            inner.Invoke(this);
            _builder.Append(")");

            return this;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return _builder.ToString();
        }
    }
}
