using Hcs.Extensions.Odata.Queryable.Expressions;

namespace Hcs.Extensions.Odata.Queryable.OdataParsers
{
    class ValueAndMemberParser : NestedParser
    {
        public ValueAndMemberParser()
            : base(
               name: nameof(ValueAndMemberParser),
               parsers: new IExpressionParser[]
               {
                       new LocalExpressionParser(),
                       new ConstantBinaryNodeParser(),
                       new MemberBinaryNodeParser(),
                       new UnaryParser(),
                       new BinaryBinaryNodeParser()
               }
            )
        { }
    }
}
