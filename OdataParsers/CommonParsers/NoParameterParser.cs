using Hcs.Extensions.Odata.Queryable.Expressions;
using Hcs.Extensions.OdataClient.Expressions;
using System;
using System.Linq.Expressions;

namespace Hcs.Extensions.Odata.Queryable.OdataParsers
{
    class NoParameterParser : IExpressionParser
    {
        public bool TryParse(ExpressionNodeContext context, bool withParameterName, out string output)
        {
            if (!(context.Node is LambdaExpression) && !context.Node.CheckHasParameter(context.ModelParameter))
            {
                var n = Expression.Default(context.Node.Type);
                var eq = Expression.Equal(context.Node, n);
                var cnv = Expression.Convert(context.Node, typeof(object));
                var body = Expression.Condition(eq, Expression.Convert(n, typeof(object)), cnv);
                var v = Expression.Lambda<Func<object>>(body).Compile();
                context.Node = Expression.Constant(v(), typeof(object));
            }
            output = null;
            return false;
        }
    }
}
