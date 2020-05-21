using Hcs.Extensions.Odata.Queryable.Expressions;
using Hcs.Extensions.OdataClient.Expressions;
using System;
using System.Linq.Expressions;

namespace Hcs.Extensions.Odata.Queryable.OdataParsers
{
    class BinaryFilterParser : IExpressionParser
    {
        [Inject]
        public ValueAndMemberParser ValueOrMemberParser { get; set; }
        [Inject]
        public LambdaFilterParser LambdaFilterParser { get; set; }
        public bool TryParse(ExpressionNodeContext context, bool withParameterName, out string output)
        {
            if (context.Node is BinaryExpression binary)
            {
                switch (context.Node.NodeType)
                {
                    case ExpressionType.Equal:
                    case ExpressionType.NotEqual:
                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                        if (ValueOrMemberParser.TryParse(context.CreateChild(binary.Left, binary.Right), withParameterName, out string left1)
                            && ValueOrMemberParser.TryParse(context.CreateChild(binary.Right, binary.Left), withParameterName, out string right1))
                        {
                            output = $"({left1} {context.Node.NodeType.ToCompareOperator()} {right1})";
                            return true;
                        }
                        break;
                    case ExpressionType.OrElse:
                    case ExpressionType.AndAlso:
                        if (LambdaFilterParser.TryParse(context.CreateChild(binary.Left), withParameterName, out string left)
                            && LambdaFilterParser.TryParse(context.CreateChild(binary.Right), withParameterName, out string right))
                        {
                            output = $"({left} {context.Node.NodeType.ToLogicOperator()} {right})";
                            return true;
                        }
                        break;
                }
                if (ValueOrMemberParser.TryParse(context.CreateChild(binary.Left, binary.Right), withParameterName, out string left2))
                {
                    output = left2;
                    return true;
                }
            }
            output = null;
            return false;
        }
    }
}
