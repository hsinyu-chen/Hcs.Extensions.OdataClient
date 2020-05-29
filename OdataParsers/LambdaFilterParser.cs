using Hcs.Extensions.Odata.Queryable.Expressions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Hcs.Extensions.Odata.Queryable.OdataParsers
{
    class LambdaFilterParser : NestedParser
    {

        public LambdaFilterParser()
            : base(
              name: nameof(LambdaFilterParser),
              parsers: new IExpressionParser[]
              {
                    new BinaryFilterParser(),
                    new MethodCallParser(),
                    new BooleanPropertyParser(),
                    new UnaryParser(),
                    new BooleanConstantParser()
              },
              services: new object[]
              {
                  new ValueAndMemberParser(),
                  new BinaryFilterParser()
              }
            )
        {
        }
        readonly static LambdaFilterParser instance = new LambdaFilterParser();
        public static string Parse(LambdaExpression expression)
        {
            var root = new ExpressionNodeContext(expression, null);
            if (instance.TryParse(new ExpressionNodeContext(expression.Body, expression.Parameters[0]) { ParentContext = root }, false, out string output))
            {
                return output;
            }
            return null;
        }
    }
}
