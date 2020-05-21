using Hcs.Extensions.Odata.Queryable.Expressions;
using Hcs.Extensions.OdataClient.Expressions;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Hcs.Extensions.Odata.Queryable.OdataParsers
{
    class EnumerableMethodCallParser : IExpressionParser
    {
        [Inject]
        public ValueAndMemberParser ValueOrMemberParser { get; set; }
        [Inject]
        public LambdaFilterParser LambdaFilterParser { get; set; }

        public bool TryParse(ExpressionNodeContext context, bool withParameterName, out string output)
        {
            if (context.Node is MethodCallExpression call && call.Method.DeclaringType == typeof(Enumerable)
                && (
                call.Method.Name == nameof(Enumerable.Any)
                || call.Method.Name == nameof(Enumerable.All)
                ))
            {
                if (ValueOrMemberParser.TryParse(context.CreateChild(call.Arguments[0]), withParameterName, out string applyTo))
                {
                    var methodName = call.Method.Name.ToLowerInvariant();
                    if (call.Arguments.Count > 1)
                    {
                        var le = (LambdaExpression)call.Arguments[1];
                        if (LambdaFilterParser.TryParse(new ExpressionNodeContext(le.Body, le.Parameters[0]) { ParentContext = context }, true, out string innerOutput))
                        {
                            var lambda = $"{le.Parameters[0].Name}:{innerOutput}";
                            output = $"{applyTo}/{methodName}({lambda})";
                            return true;
                        }
                    }
                    else
                    {
                        output = $"{applyTo}/{methodName}()";
                        return true;
                    }
                }
            }
            output = null;
            return false;
        }
    }
}
