using System;

namespace Sqlist.NET.Sql.Constraints
{
    public class CheckConstraint
    {
        public CheckConstraint(string name, Action<ConditionalClause>? conditionAction = null)
        {
            Name = name;

            if (conditionAction != null)
            {
                Condition = new ConditionalClause();
                conditionAction(Condition);
            }
        }

        public string? Name { get; set; }

        public ConditionalClause? Condition { get; set; }
    }
}
