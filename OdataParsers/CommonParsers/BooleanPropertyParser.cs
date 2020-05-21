using Hcs.Extensions.Odata.Queryable.Expressions;
using Hcs.Extensions.OdataClient.Expressions;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Hcs.Extensions.Odata.Queryable.OdataParsers
{
    class BooleanPropertyParser : IExpressionParser
    {
        [Inject]
        public BinaryFilterParser Parser { get; set; }
        public bool TryParse(ExpressionNodeContext context, bool withParameterName, out string output)
        {
            if (context.Node is MemberExpression member && member.Member.MemberType == MemberTypes.Property)
            {
                if (member.Member.DeclaringType.IsGenericType && member.Member.DeclaringType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    if (Parser.TryParse(context.CreateChild(Expression.NotEqual(Expression.Convert(member, member.Member.DeclaringType), Expression.Constant(null))), withParameterName, out output))
                    {
                        return true;
                    }
                }
                if (Parser.TryParse(context.CreateChild(Expression.Equal(member, Expression.Constant(true))), withParameterName, out output))
                {
                    return true;
                }
            }
            output = null;
            return false;
        }
    }
}
