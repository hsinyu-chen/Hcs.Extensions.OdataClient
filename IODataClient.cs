using Hcs.Extensions.Odata.Queryable;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Hcs.Extensions.OdataClient
{
    public interface IODataClient<TModel, TResult>
    {
        public HttpClient HttpClient { get; }
        public string ApiUri { get; }
        public OdataQueryOptions<TModel, TResult> QueryOptions { get; }
        public string GetQueryString(bool encode = true);

        public Task<OdataApiResponse<TResult>> SendReqeust();
    }
    public interface IODataClient<TModel>
    {
        public HttpClient HttpClient { get; }
        public string ApiUri { get; }
        public OdataQueryOptions<TModel> QueryOptions { get; }
        public IODataClient<TModel> Clone();
        public string GetQueryString(bool encode = true);
        public Task<OdataApiResponse<TModel>> SendRequet();
    }
}
