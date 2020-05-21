using System.Linq.Expressions;

namespace Hcs.Extensions.Odata.Queryable.Expressions
{
    public interface IExpressionParser
    {
        bool TryParse(ExpressionNodeContext context, bool withParameterName, out string output);
    }
}
