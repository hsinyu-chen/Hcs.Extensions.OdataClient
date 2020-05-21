using Hcs.Extensions.Odata.Queryable.Expressions;
using Hcs.Extensions.OdataClient.Expressions;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Hcs.Extensions.Odata.Queryable.OdataParsers
{
    class MemberBinaryNodeParser : IExpressionParser
    {
        public bool TryParse(ExpressionNodeContext context, bool withParameterName, out string output)
        {
            if (context.Node is MemberExpression member)
            {
                var root = member.GetRoot();
                if (root == context.ModelParameter)
                {
                    var names = member.GetMemberName();
                    if (withParameterName)
                    {
                        names = names.Prepend(context.ModelParameter.Name);
                    }
                   
                    output = string.Join("/", names);
                    return true;
                }
                else if (root is ConstantExpression)
                {
                    var value = Expression.Lambda<Func<object>>(Expression.Convert(member, typeof(object))).Compile()();
                    output = ExpressionHelpers.GetEscapedValue(value, context.ReferenceNode);
                    return true;
                }
            }
            output = null;
            return false;
        }
    }
    
}
