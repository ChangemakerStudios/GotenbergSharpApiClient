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
    var destinationDirectory = @"C:\Temp\Gotenberg\Screenshots";
    Directory.CreateDirectory(destinationDirectory);
    
    // Test URL Screenshot with PNG format
    await CreateUrlScreenshotPng(destinationDirectory);
    
    // Test URL Screenshot with JPEG format and quality settings
    await CreateUrlScreenshotJpeg(destinationDirectory);
    
    // Test URL Screenshot with WebP format
    await CreateUrlScreenshotWebp(destinationDirectory);
    
    // Test HTML Screenshot with transparency
    await CreateHtmlScreenshotWithTransparency(destinationDirectory);
    
    "All screenshots created successfully!".Dump();
}

/// <summary>
/// Creates a URL screenshot in PNG format with full page capture
/// </summary>
public async Task<string> CreateUrlScreenshotPng(string destinationDirectory)
{
    var sharpClient = new GotenbergSharpClient("http://localhost:3000");

    var builder = new UrlScreenshotRequestBuilder()
        .SetUrl("https://www.github.com")
        .SetScreenshotBehaviors(b =>
        {
            b.SetDimensions(1920, 1080)
             .SetFormat(ScreenshotImageFormat.Png)
             .SetClip(false) // Full page screenshot
             .SetOmitBackground(false);
        })
        .SetConversionBehaviors(b =>
        {
            b.AddAdditionalHeaders("User-Agent", "GotenbergScreenshotBot/1.0")
             .EmulateAsScreen()
             .SetBrowserWaitDelay(3); // Wait 3 seconds for page to load
        });

    var request = await builder.BuildAsync();
    var response = await sharpClient.UrlToScreenshotAsync(request);

    var resultPath = Path.Combine(destinationDirectory, $"GitHub-FullPage-{Rand.Next()}.png");
    
    using (var destinationStream = File.Create(resultPath))
    {
        await response.CopyToAsync(destinationStream);
    }

    $"PNG Screenshot saved: {resultPath}".Dump();
    return resultPath;
}

/// <summary>
/// Creates a URL screenshot in JPEG format with quality settings
/// </summary>
public async Task<string> CreateUrlScreenshotJpeg(string destinationDirectory)
{
    var sharpClient = new GotenbergSharpClient("http://localhost:3000");

    var builder = new UrlScreenshotRequestBuilder()
        .SetUrl("https://www.stackoverflow.com")
        .SetScreenshotBehaviors(b =>
        {
            b.SetDimensions(1200, 800)
             .SetFormat(ScreenshotImageFormat.Jpeg)
             .SetQuality(85) // JPEG compression quality
             .SetClip(true) // Clip to viewport dimensions
             .SetOptimizeForSpeed(true);
        })
        .SetConversionBehaviors(b =>
        {
            b.EmulateAsScreen()
             .SetBrowserWaitExpression("window.status === 'ready' || document.readyState === 'complete'");
        });

    var request = await builder.BuildAsync();
    var response = await sharpClient.UrlToScreenshotAsync(request);

    var resultPath = Path.Combine(destinationDirectory, $"StackOverflow-Clipped-{Rand.Next()}.jpg");
    
    using (var destinationStream = File.Create(resultPath))
    {
        await response.CopyToAsync(destinationStream);
    }

    $"JPEG Screenshot saved: {resultPath}".Dump();
    return resultPath;
}

/// <summary>
/// Creates a URL screenshot in WebP format
/// </summary>
public async Task<string> CreateUrlScreenshotWebp(string destinationDirectory)
{
    var sharpClient = new GotenbergSharpClient("http://localhost:3000");

    var builder = new UrlScreenshotRequestBuilder()
        .SetUrl("https://www.google.com")
        .SetScreenshotBehaviors(b =>
        {
            b.SetDimensions(1440, 900)
             .SetFormat(ScreenshotImageFormat.Webp)
             .SetClip(true)
             .SkipNetworkIdleEvent(); // Don't wait for network idle
        })
        .SetConversionBehaviors(b =>
        {
            b.EmulateAsScreen();
        });

    var request = await builder.BuildAsync();
    var response = await sharpClient.UrlToScreenshotAsync(request);

    var resultPath = Path.Combine(destinationDirectory, $"Google-WebP-{Rand.Next()}.webp");
    
    using (var destinationStream = File.Create(resultPath))
    {
        await response.CopyToAsync(destinationStream);
    }

    $"WebP Screenshot saved: {resultPath}".Dump();
    return resultPath;
}

/// <summary>
/// Creates an HTML screenshot with transparent background
/// </summary>
public async Task<string> CreateHtmlScreenshotWithTransparency(string destinationDirectory)
{
    var sharpClient = new GotenbergSharpClient("http://localhost:3000");

    var htmlContent = @"
    <!DOCTYPE html>
    <html>
    <head>
        <style>
            body { 
                background: transparent;
                font-family: Arial, sans-serif;
                margin: 50px;
            }
            .container {
                background: linear-gradient(45deg, #3498db, #9b59b6);
                padding: 40px;
                border-radius: 20px;
                color: white;
                text-align: center;
                box-shadow: 0 10px 30px rgba(0,0,0,0.3);
            }
            h1 { font-size: 2em; margin: 0; }
            p { font-size: 1.2em; margin: 10px 0; }
        </style>
    </head>
    <body>
        <div class='container'>
            <h1>Gotenberg Screenshot Test</h1>
            <p>This is a transparent background screenshot</p>
            <p>Generated with Gotenberg Sharp API Client</p>
        </div>
    </body>
    </html>";

    var builder = new HtmlScreenshotRequestBuilder()
        .AddDocument(doc =>
            doc.SetBody(htmlContent)
        )
        .SetScreenshotBehaviors(b =>
        {
            b.SetDimensions(800, 600)
             .SetFormat(ScreenshotImageFormat.Png)
             .SetOmitBackground(true) // Transparent background
             .SetClip(true);
        });

    var request = await builder.BuildAsync();
    var response = await sharpClient.HtmlToScreenshotAsync(request);

    var resultPath = Path.Combine(destinationDirectory, $"HTML-Transparent-{Rand.Next()}.png");
    
    using (var destinationStream = File.Create(resultPath))
    {
        await response.CopyToAsync(destinationStream);
    }

    $"Transparent HTML Screenshot saved: {resultPath}".Dump();
    return resultPath;
}
