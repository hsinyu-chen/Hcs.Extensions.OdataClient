using Hcs.Extensions.Odata.Queryable.Expressions;
using Hcs.Extensions.OdataClient.Expressions;
using System.Linq.Expressions;

namespace Hcs.Extensions.Odata.Queryable.OdataParsers
{
    class BinaryBinaryNodeParser : IExpressionParser
    {
        [InjectParent]
        public IExpressionParser Parent { get; set; }
        public bool TryParse(ExpressionNodeContext context, bool withParameterName, out string output)
        {
            if (context.Node is BinaryExpression binary)
            {
                switch (context.Node.NodeType)
                {
                    case ExpressionType.Add:
                    case ExpressionType.AddChecked:
                    case ExpressionType.Subtract:
                    case ExpressionType.SubtractChecked:
                    case ExpressionType.Multiply:
                    case ExpressionType.MultiplyChecked:
                    case ExpressionType.Divide:
                    case ExpressionType.Modulo:
                        if (Parent.TryParse(context.CreateChild(binary.Left, binary.Right), withParameterName, out string left)
                            && Parent.TryParse(context.CreateChild(binary.Right, binary.Left), withParameterName, out string right))
                        {
                            output = $"({left} {context.Node.NodeType.GetMathOperator()} {right})";
                            return true;
                        }
                        break;
                }
            }
            output = null;
            return false;
        }
    }
    
}
