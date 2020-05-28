using System.Linq.Expressions;

namespace Hcs.Extensions.Odata.Queryable.Expressions
{
    public class ExpressionNodeContext
    {
        public Expression Node { get; }
        public Expression ReferenceNode { get; set; }
        public ExpressionNodeContext ParentContext { get; set; }
        public ParameterExpression ModelParameter { get; }
        public ExpressionNodeContext CreateChild(Expression node, Expression referenceNode = null)
        {
            return new ExpressionNodeContext(node, ModelParameter, referenceNode) { ParentContext = this };
        }
        public ExpressionNodeContext(Expression node, ParameterExpression modelParameter, Expression referenceNode = null)
        {
            Node = node;
            ModelParameter = modelParameter;
            ReferenceNode = referenceNode;
        }
    }
}
