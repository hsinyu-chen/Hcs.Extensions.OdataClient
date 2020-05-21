using Hcs.Extensions.Odata.Queryable.Expressions;
using System.Linq.Expressions;

namespace Hcs.Extensions.Odata.Queryable.OdataParsers
{
    class MethodCallParser : NestedParser
    {
        public MethodCallParser()
            : base(
                 name: nameof(MethodCallParser),
                 isMatch: node => node is MethodCallExpression,
                 parsers: new IExpressionParser[]
                 {
                       new EnumerableMethodCallParser(),
                       new StringMethodCallParser()
                 }
              )
        { }
    }
}
