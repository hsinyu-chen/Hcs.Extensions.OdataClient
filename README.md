# Hcs.Extensions.OdataClient
C# simple odata client with no $metadata needed 

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