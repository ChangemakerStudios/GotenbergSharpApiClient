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

async Task Main()
{
    var path = await CreateUrlScreenshot(@"C:\Temp\Gotenberg\Dumps");
    
    var info = new ProcessStartInfo { FileName = path, UseShellExecute = true };
    Process.Start(info);
    
    path.Dump("Screenshot saved");
}

public async Task<string> CreateUrlScreenshot(string destinationDirectory)
{
    var sharpClient = new GotenbergSharpClient("http://localhost:3000");

    var builder = new UrlScreenshotRequestBuilder()
        .SetUrl("https://www.google.com")
        .SetScreenshotBehaviors(b =>
        {
            b.SetDimensions(1920, 1080)
             .SetFormat(ScreenshotImageFormat.Png)
             .SetClip(true);
        })
        .SetConversionBehaviors(b =>
        {
            b.AddAdditionalHeaders("User-Agent", "GotenbergScreenshotTest/1.0")
             .EmulateAsScreen();
        });

    var request = await builder.BuildAsync();

    var response = await sharpClient.UrlToScreenshotAsync(request);

    var resultPath = @$"{destinationDirectory}\GotenbergUrlScreenshot-{Rand.Next()}.png";

    using (var destinationStream = File.Create(resultPath))
    {
        await response.CopyToAsync(destinationStream);
    }

    return resultPath;
}
