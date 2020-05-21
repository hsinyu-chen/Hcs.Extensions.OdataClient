using Hcs.Extensions.Odata.Queryable;

namespace Hcs.Extensions.OdataClient
{
    public interface IODataClientQueryable<TModel> : IODataClient<TModel>
    {
        OdataQueryOptions<TModel> QueryOptions { get; }
        IODataClientQueryable<TModel> Clone();
    }
}
