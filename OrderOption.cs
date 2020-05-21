using System;
using System.Linq.Expressions;

namespace Hcs.Extensions.Odata.Queryable
{
    public class OrderOption<TModel>
    {
        public Expression<Func<TModel, object>> Expression { get; set; }
        public OrderDirection OrderDirection { get; set; }
    }
}
