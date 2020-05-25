using Hcs.Extensions.Odata.Queryable.OdataParsers;
using Hcs.Extensions.OdataClient.Expressions;
using Hcs.Extensions.OdataClient.OdataParsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Hcs.Extensions.Odata.Queryable.Expressions
{
    public class OdataQueryBuilder<TModel>
    {
        public IEnumerable<KeyValuePair<string, string>> Build(OdataQueryOptions<TModel> options)
        {

            if (options.WhereExpressions.Any())
            {
                var filter = string.Join(" and ", options.WhereExpressions
                    .Select(x => LambdaFilterParser.Parse(x))
                    .Where(x => !string.IsNullOrEmpty(x))
                    );
                yield return new KeyValuePair<string, string>("$filter", filter);
            }
            if (options.OrderOptions.Any())
            {
                var orders = options.OrderOptions.Select(x =>
                $"{(string.Join("/", ExpressionHelpers.GetMemberName(x.Expression)))} {x.OrderDirection.ToString().ToLowerInvariant()}");
                yield return new KeyValuePair<string, string>("$orderby", string.Join(",", orders));

            }
            if (options.Skip.HasValue)
            {
                yield return new KeyValuePair<string, string>("$skip", options.Skip.Value.ToString());
            }
            if (options.Take.HasValue)
            {
                yield return new KeyValuePair<string, string>("$top", options.Take.Value.ToString());
            }
            if (options.Take.HasValue || options.Skip.HasValue)
            {
                yield return new KeyValuePair<string, string>("$count", "true");
            }
            if (options.CustomQueryParamsFactory != null)
            {
                var custom = options.CustomQueryParamsFactory();
                foreach (var kv in custom)
                {
                    yield return kv;
                }
            }
        }
    }
    public class OdataQueryBuilder<TModel, TResult> : OdataQueryBuilder<TModel>
    {
        public IEnumerable<KeyValuePair<string, string>> Build(OdataQueryOptions<TModel, TResult> options)
        {
            foreach (var q in base.Build(options))
            {
                yield return q;
            }
            var visitor = new OdataSelectExpressionVisitor();
            OdataSelectExpand root = null;
            if (options.SelectExpression != null)
            {
                root = visitor.Parse(options.SelectExpression);
                if (root != null)
                {
                    var clean = root.GetCleanTree();
                    var select = clean.GetSelect().ToArray();
                    var expand = clean.GetExpand().ToArray();
                    if (select.Any())
                    {
                        yield return new KeyValuePair<string, string>("$select", string.Join(",", select));
                    }
                    if (expand.Any())
                    {
                        yield return new KeyValuePair<string, string>("$expand", string.Join(",", expand));
                    }
                }
            }
        }
    }
}
