using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Hcs.Extensions.OdataClient.ResultParsers
{
    public interface IApiResultParser<TModel>
    {
        public Task<IEnumerable<TModel>> ParseAsync(HttpResponseMessage httpResponse);

        public Task<(long count, IEnumerable<TModel> data)> ParseCountedAsync(HttpResponseMessage httpResponse);
    }
}
