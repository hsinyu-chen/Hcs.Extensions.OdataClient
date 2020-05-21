using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Hcs.Extensions.OdataClient.Expressions
{
    class SafeSelectVisitor : ExpressionVisitor
    {
        protected override Expression VisitMember(MemberExpression node)
        {
            if (node != null)
            {
                if (node.Member.DeclaringType.IsClass)
                {
                    var nullCheck = Expression.Equal(node.Expression, Expression.Constant(null));
                    var newNode = Expression.Condition(nullCheck, Expression.Default(node.Member.GetUnderlyingType()), node);
                    return newNode;
                }
            }
            return base.VisitMember(node);
        }
    }
}
