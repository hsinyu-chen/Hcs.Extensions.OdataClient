using Hcs.Extensions.Odata.Queryable.Expressions;
using Hcs.Extensions.OdataClient.Expressions;
using System.Linq.Expressions;
using System.Reflection;

namespace Hcs.Extensions.Odata.Queryable.OdataParsers
{
    class StringMethodCallParser : IExpressionParser
    {
        [Inject]
        public ValueAndMemberParser ValueOrMemberParser { get; set; }
        public bool TryParse(ExpressionNodeContext context, bool withParameterName, out string output)
        {
            if (context.Node is MethodCallExpression call && call.Method.DeclaringType == typeof(string))
            {
                if (ValueOrMemberParser.TryParse(context.CreateChild(call.Object, call.Arguments[0]), withParameterName, out string left)
                    && ValueOrMemberParser.TryParse(context.CreateChild(call.Arguments[0], call.Object), withParameterName, out string right))
                {
                    output = $"{call.Method.Name.ToLowerInvariant()}({left},{right})";
                    return true;
                }

            }
            output = null;
            return false;
        }
    }
}
