<Query Kind="Program">
  <Reference Relative="..\lib\bin\Debug\net6.0\Gotenberg.Sharp.API.Client.dll">C:\Code\GotenbergSharpApiClient\lib\bin\Debug\net6.0\Gotenberg.Sharp.API.Client.dll</Reference>
  <Namespace>Gotenberg.Sharp.API.Client</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Domain.Builders</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Domain.Builders.Faceted</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Extensions</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Infrastructure</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

static Random Rand = new Random(Math.Abs((int)DateTime.Now.Ticks));
static string ResourcePath = @$"{Path.GetDirectoryName(Util.CurrentQueryPath)}\Resources\Html";

async Task Main()
{
    var path = await CreateHtmlScreenshot(@"C:\Temp\Gotenberg\Dumps");
    
    var info = new ProcessStartInfo { FileName = path, UseShellExecute = true };
    Process.Start(info);
    
    path.Dump("Screenshot saved");
}

public async Task<string> CreateHtmlScreenshot(string destinationDirectory)
{
    var sharpClient = new GotenbergSharpClient("http://localhost:3000");

    var builder = new HtmlScreenshotRequestBuilder()
        .AddAsyncDocument(async doc =>
            doc.SetBody(await GetHtmlFile("body.html"))
        )
        .SetScreenshotBehaviors(b =>
        {
            b.SetDimensions(1200, 800)
             .SetFormat(ScreenshotImageFormat.Jpeg)
             .SetQuality(90)
             .SetClip(true);
        })
        .WithAsyncAssets(async assets => 
            assets.AddItem("ear-on-beach.jpg", await GetImageBytes())
        );

    var request = await builder.BuildAsync();

    var response = await sharpClient.HtmlToScreenshotAsync(request);

    var resultPath = @$"{destinationDirectory}\GotenbergHtmlScreenshot-{Rand.Next()}.jpg";

    using (var destinationStream = File.Create(resultPath))
    {
        await response.CopyToAsync(destinationStream);
    }

    return resultPath;
}

static Task<byte[]> GetImageBytes()
{
    return File.ReadAllBytesAsync($@"{ResourcePath}\ear-on-beach.jpg");
}

static Task<byte[]> GetHtmlFile(string fileName)
{
    return File.ReadAllBytesAsync($@"{ResourcePath}\ConvertExample\{fileName}");
}
