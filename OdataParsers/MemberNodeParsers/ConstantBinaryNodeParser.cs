using Hcs.Extensions.Odata.Queryable.Expressions;
using Hcs.Extensions.OdataClient.Expressions;
using System.Linq.Expressions;

namespace Hcs.Extensions.Odata.Queryable.OdataParsers
{
    class ConstantBinaryNodeParser : IExpressionParser
    {
        public bool TryParse(ExpressionNodeContext context, bool withParameterName, out string output)
        {
            if (context.Node is ConstantExpression constant)
            {
                output = ExpressionHelpers.GetEscapedValue(constant.Value, context.ReferenceNode);
                return true;
            }
            output = null;
            return false;
        }
    }

}
