using Gotenberg.Sharp.API.Client;
using Gotenberg.Sharp.API.Client.Domain.Builders;
using Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;

var destinationDirectory = args.Length > 0 ? args[0] : Path.Combine(Directory.GetCurrentDirectory(), "output");
Directory.CreateDirectory(destinationDirectory);

var resourcePath = Path.Combine(AppContext.BaseDirectory, "resources", "Html", "ConvertExample");
var path = await CreateFromHtml(destinationDirectory, resourcePath);

Console.WriteLine($"PDF created: {path}");

static async Task<string> CreateFromHtml(string destinationDirectory, string resourcePath)
{
    var sharpClient = new GotenbergSharpClient("http://localhost:3000");

    var builder = new HtmlRequestBuilder()
       .AddAsyncDocument(async doc =>
            doc.SetBody(await GetHtmlFile(resourcePath, "body.html"))
               .SetFooter(await GetHtmlFile(resourcePath, "footer.html"))
        ).WithPageProperties(dims => dims.UseChromeDefaults())
        .WithAsyncAssets(async assets =>
            assets.AddItem("ear-on-beach.jpg", await GetImageBytes(resourcePath))
        )
        .SetConversionBehaviors(b =>
            b.AddAdditionalHeaders("hello", "from-earth")
        )
        .ConfigureRequest(b => b.SetPageRanges("1"));

    var request = await builder.BuildAsync();

    var resultPath = Path.Combine(destinationDirectory, $"GotenbergFromHtml-{DateTime.Now:yyyyMMddHHmmss}.pdf");
    var response = await sharpClient.HtmlToPdfAsync(request);

    using (var destinationStream = File.Create(resultPath))
    {
        await response.CopyToAsync(destinationStream);
    }

    return resultPath;
}

static Task<byte[]> GetImageBytes(string resourcePath)
{
    return File.ReadAllBytesAsync(Path.Combine(resourcePath, "ear-on-beach.jpg"));
}

static Task<byte[]> GetHtmlFile(string resourcePath, string fileName)
{
    return File.ReadAllBytesAsync(Path.Combine(resourcePath, fileName));
}
