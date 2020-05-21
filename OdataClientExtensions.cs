using Hcs.Extensions.Odata.Queryable;
using System;
using System.Linq.Expressions;

namespace Hcs.Extensions.OdataClient
{
    public static class IODataClientExtensions
    {

        public static IODataClient<TModel> OrderBy<TModel>(this IODataClient<TModel> client, Expression<Func<TModel, object>> expression) where TModel : class
        {
            var o = client.Clone();
            o.QueryOptions.OrderOptions.Clear();
            o.QueryOptions.OrderOptions.Add(new OrderOption<TModel>
            {
                Expression = expression,
                OrderDirection = OrderDirection.Asc
            });
            return o;
        }

        public static IODataClient<TModel> OrderByDesc<TModel>(this IODataClient<TModel> client, Expression<Func<TModel, object>> expression) where TModel : class
        {
            var o = client.Clone();
            o.QueryOptions.OrderOptions.Clear();
            o.QueryOptions.OrderOptions.Add(new OrderOption<TModel>
            {
                Expression = expression,
                OrderDirection = OrderDirection.Desc
            });
            return o;
        }
        public static IODataClient<TModel> ThenBy<TModel>(this IODataClient<TModel> client, Expression<Func<TModel, object>> expression) where TModel : class
        {
            var o = client.Clone();
            o.QueryOptions.OrderOptions.Add(new OrderOption<TModel>
            {
                Expression = expression,
                OrderDirection = OrderDirection.Asc
            });
            return o;
        }

        public static IODataClient<TModel> ThenByDesc<TModel>(this IODataClient<TModel> client, Expression<Func<TModel, object>> expression) where TModel : class
        {
            var o = client.Clone();
            o.QueryOptions.OrderOptions.Add(new OrderOption<TModel>
            {
                Expression = expression,
                OrderDirection = OrderDirection.Desc
            });
            return o;
        }
        public static IODataClient<TModel, TResult> Select<TModel, TResult>(this IODataClient<TModel> client, Expression<Func<TModel, TResult>> memberExpression) where TModel : class
        {
            var o = new OdataClient<TModel, TResult>(client.HttpClient, client.ApiUri, client.QueryOptions.Clone<TResult>());
            o.QueryOptions.SelectExpression = memberExpression;
            return o;
        }

        public static IODataClient<TModel> Skip<TModel>(this IODataClient<TModel> client, int skip) where TModel : class
        {
            var o = client.Clone();
            o.QueryOptions.Skip = skip;
            return o;
        }

        public static IODataClient<TModel> Take<TModel>(this IODataClient<TModel> client, int take) where TModel : class
        {
            var o = client.Clone();
            o.QueryOptions.Take = take;
            return o;
        }

        public static IODataClient<TModel> Where<TModel>(this IODataClient<TModel> client, Expression<Func<TModel, bool>> expression) where TModel : class
        {
            var o = client.Clone();
            o.QueryOptions.WhereExpressions.Add(expression);
            return o;
        }
    }
}
