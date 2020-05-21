using System.Collections;
using System.Collections.Generic;
using System.Net.Http;

namespace Hcs.Extensions.OdataClient
{
    public class OdataApiResponse<TModel> : IEnumerable<TModel>
    {
        public IEnumerable<TModel> Data { get; }
        public HttpResponseMessage HttpResponse { get; }
        public OdataApiResponse(IEnumerable<TModel> data, HttpResponseMessage response)
        {
            Data = data;
            HttpResponse = response;
        }

        public IEnumerator<TModel> GetEnumerator() => Data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Data.GetEnumerator();
    }
}
