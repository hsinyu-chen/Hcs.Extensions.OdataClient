# Hcs.Extensions.OdataClient
C# simple odata client with no $metadata needed 
for someone only use OdataQueryOptions for Query api like me
## install package
```Install-Package Hcs.Extensions.OdataClient```
## use
```csharp
    var maxId = 100;
    var req = httpClient.GetOdataClient<File>("/api/FileManage")
                .Where(x => x.CategoryId.HasValue)
                .Where(x => x.CategoryId.HasValue && x.Category.Id + 10 < maxId)
                .OrderBy(x => x.Path).ThenBy(x => x.Name)
                .Take(100)
                .Select(x => new { brand_new_id_property = x.Id, x.Path, xxxx = new { x.Category.Name } });

    Console.WriteLine(req.GetQueryString(encode: false));

    foreach (var result in await req.SendReqeust())
    {
        Console.WriteLine($"{result.brand_new_id_property} {result.xxxx.Name}");
    }
```

## excute result
```
?$filter=CategoryId ne null and (CategoryId ne null and (Category/Id add 10) lt 100)&$orderby=Path asc,Name asc&$top=100&$count=true&$select=Id,Path&$expand=Category($select=Name)

3 A
2 A
4 A
5 A

```
## server side configuration for .net core 
### install package
```Microsoft.AspNetCore.OData```
### add services
```csharp
services.AddOData();
services.AddODataQueryFilter();
services.AddSingleton<ODataUriResolver, StringAsEnumResolver>();
//for .net core 3.1 you need replace built in json serializer for odata $select/$expand working
services.AddControllersWithViews().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
    options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
    options.SerializerSettings.StringEscapeHandling = Newtonsoft.Json.StringEscapeHandling.EscapeHtml;
});
```
### endpoint
```csharp
app.UseEndpoints(endpoints =>
{
    endpoints.EnableDependencyInjectionForOdata();
    endpoints.Count().Filter().OrderBy().Expand().Select().MaxTop(100); //allow complex query function
    // ....
});
```
### controller
```csharp
[Route("api/[controller]")]
[ApiController]
public class FileManageController : ControllerBase
{
    DbContext context;
    public FileManageController(DbContext context)
    {
        this.context = context;
    }
    [EnableQuery]
    public IActionResult Get()
    {
        return Ok(context.Set<File>().AsQueryable());
    }
}
```
## count
you can replace odata's EnableQueryAttribute to custom ```ActionFilter``` like this
```csharp
public class HcsEnableQueryAttribute : EnableQueryAttribute
{
    public override void OnActionExecuted(ActionExecutedContext actionExecutedContext)
    {
        base.OnActionExecuted(actionExecutedContext);
        var odata = actionExecutedContext.HttpContext.ODataFeature();

        if (odata.TotalCount.HasValue)
        {
            actionExecutedContext.HttpContext.Response.Headers.Add("x-total-count", odata.TotalCount.Value.ToString("#0"));
        }
    }
}
```
and read count from response header
```csharp
var response = await req.SendReqeust();
if (response.HttpResponse.Headers.TryGetValues("x-total-count", out IEnumerable<string> values)
        && values.Any()
        && int.TryParse(values.First(), out int count))
{
    // do things...
}
```
you can create an extension methodfor this

## WARNING
I wrote this lib in rush,so use with your own risk
