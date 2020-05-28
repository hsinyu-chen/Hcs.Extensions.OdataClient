using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Hcs.Extensions.OdataClient.Expressions
{
    public class CheckHasParameterVisitor : ExpressionVisitor
    {
        protected CheckHasParameterVisitor(ParameterExpression parameter)
        {
            this.parameter = parameter;
        }
        bool hasParameter = false;
        private readonly ParameterExpression parameter;

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (node == parameter)
            {
                hasParameter = true;
                return node;
            }
            return base.VisitParameter(node);
        }
        public override Expression Visit(Expression node)
        {
            if (hasParameter)
            {
                return node;
            }
            return base.Visit(node);
        }
        public static bool Check(Expression expression, ParameterExpression parameter)
        {
            var v = new CheckHasParameterVisitor(parameter);
            v.Visit(expression);
            return v.hasParameter;
        }
    }
    public class SafeSelectVisitor : ExpressionVisitor
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
