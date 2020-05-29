using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Hcs.Extensions.OdataClient.ResultParsers
{
    public class CountHeaderNotFoundException : Exception
    {
        public CountHeaderNotFoundException(string message) : base(message)
        {
        }

        public CountHeaderNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
    public class HeaderCountResultParser<TModel> : IApiResultParser<TModel>
    {
        private readonly string headerName;

        public IApiResultParser<TModel> BaseParser { get; set; } = new JsonResultParser<TModel>();
        public HeaderCountResultParser(string headerName = "x-total-count")
        {
            this.headerName = headerName;
        }
        public Task<IEnumerable<TModel>> ParseAsync(HttpResponseMessage httpResponse)
        {
            return BaseParser.ParseAsync(httpResponse);
        }

        public async Task<(long count, IEnumerable<TModel> data)> ParseCountedAsync(HttpResponseMessage httpResponse)
        {
            if (httpResponse.Headers.TryGetValues(headerName, out IEnumerable<string> values)
               && values.Any()
               && int.TryParse(values.First(), out int count))
            {
                return (count, await BaseParser.ParseAsync(httpResponse));
            }
            throw new CountHeaderNotFoundException($"count header {headerName} not found");
        }
    }
}
