using Hcs.Extensions.Odata.Queryable.Expressions;
using Hcs.Extensions.OdataClient.Expressions;
using System.Linq.Expressions;

namespace Hcs.Extensions.Odata.Queryable.OdataParsers
{
    class UnaryParser : IExpressionParser
    {
        [InjectParent]
        public IExpressionParser Parent { get; set; }
        public bool TryParse(ExpressionNodeContext context, bool withParameterName, out string output)
        {
            if (context.Node is UnaryExpression u)
            {
                if (u.NodeType == ExpressionType.Not)
                {
                    if (Parent.TryParse(context.CreateChild(u.Operand), withParameterName, out string inner))
                    {
                        output = $"not({inner})";
                        return true;
                    }
                }
                else if (u.NodeType == ExpressionType.Convert)
                {
                    if (Parent.TryParse(context.CreateChild(u.Operand), withParameterName, out string inner))
                    {
                        output = inner;
                        return true;
                    }
                }
            }
            output = null;
            return false;
        }
    }
}
