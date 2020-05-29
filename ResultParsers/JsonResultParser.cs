using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Hcs.Extensions.OdataClient.ResultParsers
{
    public class JsonResultParser<TModel> : IApiResultParser<TModel>
    {
        public async Task<IEnumerable<TModel>> ParseAsync(HttpResponseMessage httpResponse)
        {
            if (httpResponse.Content.Headers.ContentType?.MediaType == "application/json")
            {

                return await JsonSerializer.DeserializeAsync<TModel[]>(await httpResponse.Content.ReadAsStreamAsync());
            }
            else
            {
                throw new NotSupportedException($"default {nameof(JsonResultParser<TModel>)} only support json response");
            }
        }

        public async Task<(long count, IEnumerable<TModel> data)> ParseCountedAsync(HttpResponseMessage httpResponse)
        {
            var parsed = await ParseAsync(httpResponse);
            return (parsed.Count(), parsed);
        }
    }
}
