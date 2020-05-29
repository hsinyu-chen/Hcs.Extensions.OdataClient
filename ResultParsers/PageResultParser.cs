using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Hcs.Extensions.OdataClient.ResultParsers
{
    class OdataPageResult<TModel>
    {
        public IEnumerable<TModel> Items { get; set; }
        public long Count { get; set; }
    }
    public class PageResultParser<TModel> : IApiResultParser<TModel>
    {
        public IApiResultParser<TModel> BaseParser { get; set; } = new JsonResultParser<TModel>();
        public Task<IEnumerable<TModel>> ParseAsync(HttpResponseMessage httpResponse)
        {
            return BaseParser.ParseAsync(httpResponse);
        }

        public async Task<(long count, IEnumerable<TModel> data)> ParseCountedAsync(HttpResponseMessage httpResponse)
        {
            if (httpResponse.Content.Headers.ContentType?.MediaType == "application/json")
            {

                var pageResult = await JsonSerializer.DeserializeAsync<OdataPageResult<TModel>>(await httpResponse.Content.ReadAsStreamAsync());
                return (pageResult.Count, pageResult.Items);
            }
            else
            {
                throw new NotSupportedException($"default {nameof(PageResultParser<TModel>)} only support json response");
            }
        }
    }
}
