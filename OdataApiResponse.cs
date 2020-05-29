using System.Collections;
using System.Collections.Generic;
using System.Net.Http;

namespace Hcs.Extensions.OdataClient
{
    public class OdataApiResponse<TModel> : IEnumerable<TModel>
    {
        public IEnumerable<TModel> Data { get; }
        public HttpResponseMessage HttpResponse { get; }
        public long? Count { get; }
        public OdataApiResponse(IEnumerable<TModel> data, long? count, HttpResponseMessage response)
        {
            HttpResponse = response;
            Data = data;
            Count = count;
        }

        public IEnumerator<TModel> GetEnumerator() => Data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Data.GetEnumerator();
    }
}
