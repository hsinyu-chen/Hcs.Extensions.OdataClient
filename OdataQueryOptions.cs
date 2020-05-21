using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Hcs.Extensions.Odata.Queryable
{
    public class OdataQueryOptions<TModel, TResult> : OdataQueryOptions<TModel>
    {
        public Expression<Func<TModel, TResult>> SelectExpression { get; set; }
        public new OdataQueryOptions<TModel, TResult> Clone()
        {
            var c = new OdataQueryOptions<TModel, TResult>();
            c.WhereExpressions.AddRange(WhereExpressions);
            c.OrderOptions.AddRange(OrderOptions);
            c.Take = Take;
            c.Skip = Skip;
            c.SelectExpression = SelectExpression;
            return c;
        }
    }
    public class OdataQueryOptions<TModel>
    {
        public List<Expression<Func<TModel, bool>>> WhereExpressions { get; } = new List<Expression<Func<TModel, bool>>>();
        public List<OrderOption<TModel>> OrderOptions { get; } = new List<OrderOption<TModel>>();

        public int? Take { get; set; }
        public int? Skip { get; set; }
        public OdataQueryOptions<TModel, TResult> Clone<TResult>()
        {
            var c = new OdataQueryOptions<TModel, TResult>();
            c.WhereExpressions.AddRange(WhereExpressions);
            c.OrderOptions.AddRange(OrderOptions);
            c.Take = Take;
            c.Skip = Skip;
            return c;
        }
        public OdataQueryOptions<TModel> Clone()
        {
            var c = new OdataQueryOptions<TModel>();
            c.WhereExpressions.AddRange(WhereExpressions);
            c.OrderOptions.AddRange(OrderOptions);
            c.Take = Take;
            c.Skip = Skip;
            return c;
        }

    }
}
