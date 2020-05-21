using Hcs.Extensions.Odata.Queryable.Expressions;
using System.Linq.Expressions;

namespace Hcs.Extensions.Odata.Queryable.OdataParsers
{
    class BooleanConstantParser : IExpressionParser
    {
        public bool TryParse(ExpressionNodeContext context, bool withParameterName, out string output)
        {
            if (context.Node is ConstantExpression constant)
            {
                if (constant.Value is bool b)
                {
                    output = b ? "" : "1 eq 0";
                    return true;
                }
            }

            output = null;
            return false;
        }
    }

}
