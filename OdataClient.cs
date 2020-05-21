using Hcs.Extensions.Odata.Queryable;
using Hcs.Extensions.Odata.Queryable.Expressions;
using Hcs.Extensions.OdataClient.Expressions;
using System;
using System.Collections;
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

        HttpClient IODataClient<TModel, TResult>.HttpClient => throw new NotImplementedException();

        string IODataClient<TModel, TResult>.ApiUri => throw new NotImplementedException();

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
        public async Task<IEnumerable<TResult>> SendReqeust()
        {
            var baseResult = await base.SendRequet();
            return Projection(baseResult);
        }

    }
    public class OdataClient<TModel> : IODataClient<TModel>, IODataClientQueryable<TModel>
    {
        public OdataQueryOptions<TModel> QueryOptions { get; }

        public HttpClient HttpClient { get; }

        public string ApiUri { get; }

        public IODataClientQueryable<TModel> Clone()
        {
            return new OdataClient<TModel>(HttpClient, ApiUri, QueryOptions.Clone());
        }
        public OdataClient(
            HttpClient client,
            string apiUri,
            OdataQueryOptions<TModel> queryOptions = null)
        {
            this.HttpClient = client;
            this.ApiUri = apiUri;
            QueryOptions = queryOptions ?? new OdataQueryOptions<TModel>();
        }
        protected virtual async ValueTask<IEnumerable<TModel>> ParseResponseAsync(HttpResponseMessage httpResponse)
        {
            var content = httpResponse.Content;
            if (content.Headers.ContentType?.MediaType == "application/json")
            {
#if DEBUG
                var js = await content.ReadAsStringAsync();
#endif
                return await JsonSerializer.DeserializeAsync<TModel[]>(await content.ReadAsStreamAsync());
            }
            else
            {
                throw new NotSupportedException($"default {nameof(ParseResponseAsync)} only support json response");
            }
        }
        protected virtual async ValueTask<HttpResponseMessage> SendRequestAsnyc(HttpClient client, Uri odataUrl)
        {
            return await client.GetAsync(odataUrl);
        }

        public virtual async Task<IEnumerable<TModel>> SendRequet()
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
            var response = await SendRequestAsnyc(HttpClient, uri);
            var data = await ParseResponseAsync(response);
            return data;
        }

        protected virtual IEnumerable<KeyValuePair<string, string>> GetQuery()
        {
            var queryBuilder = new OdataQueryBuilder<TModel, TModel>();
            return queryBuilder.Build(QueryOptions);
        }

        IODataClient<TModel> IODataClient<TModel>.Clone() => Clone();

        public string GetQueryString(bool encode = true) => $"?{(string.Join("&", GetQuery().Select(kv => $"{kv.Key}={(encode ? Uri.EscapeUriString(kv.Value) : kv.Value)}")))}";
    }
}
