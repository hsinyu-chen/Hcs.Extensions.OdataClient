using Hcs.Extensions.Odata.Queryable;
using Hcs.Extensions.Odata.Queryable.Expressions;
using Hcs.Extensions.OdataClient.Expressions;
using Hcs.Extensions.OdataClient.ResultParsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Hcs.Extensions.OdataClient
{
    public class OdataClient<TModel, TResult> : OdataClient<TModel>, IODataClient<TModel, TResult>
    {
        public OdataClient(HttpClient client, string apiUri, OdataQueryOptions<TModel, TResult> queryOptions = null) : base(client, apiUri, queryOptions)
        {
            QueryOptions = queryOptions;
        }

        new public OdataQueryOptions<TModel, TResult> QueryOptions { get; }


        OdataQueryOptions<TModel, TResult> IODataClient<TModel, TResult>.QueryOptions => throw new NotImplementedException();

        protected virtual IEnumerable<TResult> Projection(IEnumerable<TModel> model)
        {
            if (QueryOptions.SelectExpression == null)
            {
                throw new InvalidOperationException($"No SelectExpression for result conversion");
            }
            var visted = (Expression<Func<TModel, TResult>>)new SafeSelectVisitor().Visit(QueryOptions.SelectExpression);
            var m = visted.Compile();
            return model.Select(m);
        }
        protected override IEnumerable<KeyValuePair<string, string>> GetQuery()
        {
            var queryBuilder = new OdataQueryBuilder<TModel, TResult>();
            return queryBuilder.Build(QueryOptions);
        }
        public async Task<OdataApiResponse<TResult>> SendReqeust()
        {
            var uri = CreateRequestUrl();
            var response = await SendRequestAsnyc(HttpClient, uri);
            if (QueryOptions.Take.HasValue || QueryOptions.Skip.HasValue)
            {
                var (count, data) = await ResultParser.ParseCountedAsync(response);
                return new OdataApiResponse<TResult>(Projection(data), count, response);
            }
            else
            {
                var data = await ResultParser.ParseAsync(response);
                return new OdataApiResponse<TResult>(Projection(data), null, response);
            }
        }

        public Task<OdataApiResponse<TModel>> SendRequestWithoutProjection()
        {
            return base.SendRequet();
        }
    }
    public class OdataClient<TModel> : IODataClient<TModel>
    {
        public IApiResultParser<TModel> ResultParser { get; set; }

        public OdataQueryOptions<TModel> QueryOptions { get; }

        public HttpClient HttpClient { get; }

        public string ApiUri { get; }

        public IODataClient<TModel> Clone()
        {
            return new OdataClient<TModel>(HttpClient, ApiUri, QueryOptions.Clone());
        }
        public OdataClient(
            HttpClient client,
            string apiUri,
            OdataQueryOptions<TModel> queryOptions = null,
            IApiResultParser<TModel> resultParser = null)
        {
            this.HttpClient = client;
            this.ApiUri = apiUri;
            QueryOptions = queryOptions ?? new OdataQueryOptions<TModel>();
            ResultParser = resultParser ?? new HeaderCountResultParser<TModel>();
        }
        protected virtual async ValueTask<HttpResponseMessage> SendRequestAsnyc(HttpClient client, Uri odataUrl)
        {
            return await client.GetAsync(odataUrl);
        }

        public virtual async Task<OdataApiResponse<TModel>> SendRequet()
        {
            var uri = CreateRequestUrl();
            var response = await SendRequestAsnyc(HttpClient, uri);
            if (QueryOptions.Take.HasValue || QueryOptions.Skip.HasValue)
            {
                var (count, data) = await ResultParser.ParseCountedAsync(response);
                return new OdataApiResponse<TModel>(data, count, response);
            }
            else
            {
                var data = await ResultParser.ParseAsync(response);
                return new OdataApiResponse<TModel>(data, null, response);
            }
        }

        protected virtual Uri CreateRequestUrl()
        {
            var query = GetQuery();
            var uri = new Uri(HttpClient.BaseAddress, ApiUri);
            var q = new List<string>(uri.Query.Trim('?').Split('&').Where(x => !string.IsNullOrWhiteSpace(x)));
            foreach (var kv in query)
            {
                q.Add($"{kv.Key}={Uri.EscapeUriString(kv.Value)}");
            }

            if (q.Count > 0)
            {
                var builder = new UriBuilder(uri);
                builder.Query = "?" + string.Join("&", q);
                uri = builder.Uri;
            }

            return uri;
        }

        protected virtual IEnumerable<KeyValuePair<string, string>> GetQuery()
        {
            var queryBuilder = new OdataQueryBuilder<TModel>();
            return queryBuilder.Build(QueryOptions);
        }

        IODataClient<TModel> IODataClient<TModel>.Clone() => Clone();

        public string GetQueryString(bool encode = true) => $"?{(string.Join("&", GetQuery().Select(kv => $"{kv.Key}={(encode ? Uri.EscapeUriString(kv.Value) : kv.Value)}")))}";
    }
}
