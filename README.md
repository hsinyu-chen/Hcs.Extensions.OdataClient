# Hcs.Extensions.OdataClient
C# simple odata client with no $metadata needed 
for someone only use OdataQueryOptions for Query api like me
## Install package
```Install-Package Hcs.Extensions.OdataClient```
## Use
```csharp
var httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:44326/") };
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

### excute result
```
?$filter=CategoryId ne null and (CategoryId ne null and (Category/Id add 10) lt 100)&$orderby=Path asc,Name asc&$top=100&$count=true&$select=Id,Path&$expand=Category($select=Name)

3 A
2 A
4 A
5 A

```
## Limitations
for current version of this lib `IS NOT IQueryable` implementation
### Take/Skip
use `Take` and `Skip` will give you new query instance but just simple replace skip/take value,behavior is not like linq (eq ```[1,2,3,4,5].Take(5).Take(3) will get  [1,2,3]```)
### OrderBy
same as `Take/Skip` but you still can use `ThenBy/ThenByDesc` for `$orderby=A asc,B asc`
combine with take/skip will work like following
```csharp
// if all data is [{a:1},{a:2},{a:3},{a:4},{a:5}]
query.Take(4) //this will not affect query result
.OrderBy(x=>x.a) //this will not affect query result
.Take(2) // replace $top value to 2
.Skip(1)
.OrderByDesc(x=>x.a) // replace old order expressions
```
will give you `$orderby=a desc&$top=2&$skip=1`
[{a=5},{a=4}]
### Select
for reduce complexity of expression parser `Select` can only apply once to the query, `select expression` is only for generate odata $select/$expand,then lib compile the expression for local projection use(`Enumerable.Select`),lib will insert nested member access null check for you , for example
```csharp
x = >new{ x.Category.Name }
```
will modify to
```cs
x => new { Name = x.Category != null ? x.Category.Name : default(string) }
```
when use on result projection, so don't worry about null check.

# Server side configuration hints for .net core 
### Install package
```Microsoft.AspNetCore.OData```
### Add services
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
### Config endpoint
```csharp
app.UseEndpoints(endpoints =>
{
    endpoints.EnableDependencyInjectionForOdata();
    endpoints.Count().Filter().OrderBy().Expand().Select().MaxTop(100); //allow complex query function
    // ....
});
```
### Controller
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
# Count
Default ResultParser get count from response header
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
you can parse other result parser to OdataClient constructor for your endpoint response format
```csharp
new OdataClient<File>(httpClient,"/api/FileManage",resultParser:new PageResultParser<File>())
```
PageResultParser is built-in parser for starndard odata endpoint
# WARNING
I wrote this lib in rush (about 20ish hours),so use with your own risk,
feel free to file a PR if you encounter any bugs
