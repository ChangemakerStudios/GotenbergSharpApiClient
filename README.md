# <img src="https://github.com/ChangemakerStudios/GotenbergSharpApiClient/raw/master/lib/Resources/gotenbergSharpClient.PNG" width="24" height="24" /> Gotenberg.Sharp.Api.Client

[![NuGet version](https://badge.fury.io/nu/Gotenberg.Sharp.Api.Client.svg)](https://badge.fury.io/nu/Gotenberg.Sharp.Api.Client)
[![Downloads](https://img.shields.io/nuget/dt/Gotenberg.Sharp.API.Client.svg?logo=nuget&color=purple)](https://www.nuget.org/packages/Gotenberg.Sharp.API.Client) 
![Build status](https://github.com/ChangemakerStudios/GotenbergSharpApiClient/actions/workflows/deploy.yml/badge.svg)

⭐ For Gotenberg v7+.⭐

.NET C# Client for interacting with the [Gotenberg](https://gotenberg.dev/) v7 micro-service's API. [Gotenberg](https://github.com/gotenberg/gotenberg) is a [Docker-powered stateless API](https://hub.docker.com/r/gotenberg/gotenberg/) for converting & merging HTML, Markdown and Office documents to PDF. The client supports a configurable [Polly](http://www.thepollyproject.org/) **retry policy** with exponential backoff for handling transient exceptions.


# Getting Started
*Pull the image from dockerhub.com*
```powershell
> docker pull gotenberg/gotenberg:latest
```
*Create & start a container*
```powershell
docker run --name gotenbee7x --rm -p 3000:3000 gotenberg/gotenberg:latest gotenberg --api-timeout=1800s --log-level=debug
```

# .NET Core Project Setup
*Install nuget package into your project*
```powershell
PM> Install-Package Gotenberg.Sharp.Api.Client
```

*Note: Use v1.x nugets for Gotenberg v6.*

## AppSettings
```json
  "GotenbergSharpClient": {
    "ServiceUrl": "http://localhost:3000",
    "HealthCheckUrl": "http://localhost:3000/health",
    "RetryPolicy": {
      "Enabled": true,
      "RetryCount": 4,
      "BackoffPower": 1.5,
      "LoggingEnabled": true
    }
  }
```

## Configure Services In Startup.cs
```csharp
public void ConfigureServices(IServiceCollection services)
{
	.....
    services.AddOptions<GotenbergSharpClientOptions>()
	        .Bind(Configuration.GetSection(nameof(GotenbergSharpClient)));
    services.AddGotenbergSharpClient();
	.....    
}

```
# Using GotenbergSharpClient.
*See the [linqPad folder](linqpad/)* for complete examples. 

### Note: Samples below are currently stale. Linqpad scripts are fresh.

## Html To Pdf 
*With embedded assets:*

```csharp
 [HttpGet]
 public async Task<ActionResult> HtmlToPdf([FromServices] GotenbergSharpClient sharpClient)
 {
     var builder = new HtmlRequestBuilder()
         .AddDocument(doc => 
             doc.SetBody(GetBody()).SetFooter(GetFooter())
         ).WithDimensions(dims =>
         {
             dims.SetPaperSize(PaperSizes.A3)
                 .SetMargins(Margins.None)
                 .SetScale(.99);
         }).WithAsyncAssets(async assets => assets.AddItem("some-image.jpg", await GetImageBytes()));

     var req = await builder.BuildAsync();

     var result = await sharpClient.HtmlToPdfAsync(req);

     return this.File(result, "application/pdf", "gotenbergFromHtml.pdf");
 }
```

## Url To Pdf
*Url to Pdf with custom page range, header & footer:*

```csharp
public async Task<Stream> CreateFromUrl(string headerPath, string footerPath)
{
	var builder = new UrlRequestBuilder()
		.SetUrl("https://www.cnn.com")
		.ConfigureRequest(config =>
		{
			config.SetPageRanges("1-2");
		})
		.AddAsyncHeaderFooter(async
			doc => doc.SetHeader(await File.ReadAllTextAsync(headerPath))
				  .SetFooter(await File.ReadAllBytesAsync(footerPath)
		)).WithDimensions(dims =>
		{
			dims.SetPaperSize(PaperSizes.A4)
			 .SetMargins(Margins.None)
			 .SetScale(.90)
			 .LandScape();
		});

	var request = await builder.BuildAsync();
	return await _sharpClient.UrlToPdfAsync(request);
}
```
## Merge Office Docs
*Merges office documents and configures the request time-out:*

```csharp
public async Task<Stream> DoOfficeMerge(string sourceDirectory)
{
	var builder = new MergeOfficeBuilder()
		.WithAsyncAssets(async a => a.AddItems(await GetDocsAsync(sourceDirectory)));

	var request = await builder.BuildAsync();
	return await _sharpClient.MergeOfficeDocsAsync(request);
}
```
## Markdown to Pdf
*Markdown to Pdf conversion with embedded assets:*

```csharp
public async Task<Stream> CreateFromMarkdown()
{
	var builder = new HtmlRequestBuilder()
		.AddAsyncDocument(async
			doc => doc.SetHeader(await this.GetHeaderAsync())
				  .SetBody(await GetBodyAsync())
				  .ContainsMarkdown()
				  .SetFooter(await GetFooterAsync())
		).WithDimensions(dims =>
		{
			dims.UseChromeDefaults().LandScape().SetScale(.90);
		}).WithAsyncAssets(async
			a => a.AddItems(await GetMarkdownAssets())
		));

	var request = await builder.BuildAsync();
	return await _sharpClient.HtmlToPdfAsync(request);
}
```
## Webhook
*All request types support webhooks*

```csharp
 public async Task SendUrlToWebhookEndpoint(string headerPath, string footerPath)
 {
     var builder = new UrlRequestBuilder()
         .SetUrl("https://www.cnn.com")
         .ConfigureRequest(reqBuilder =>
         {
             reqBuilder.AddWebhook(hook =>
                 {
                     hook.SetUrl("http://host.docker.internal:5000/api/your/webhookReceiver")
                         .SetErrorUrl("http://host.docker.internal:5000/api/your/webhookReceiver/error")
                         .AddExtraHeader("custom-header", "value");
                 })
                 .SetPageRanges("1-2");
         })
         .AddAsyncHeaderFooter(async
             b => b.SetHeader(await System.IO.File.ReadAllTextAsync(headerPath))
                   .SetFooter(await System.IO.File.ReadAllBytesAsync(footerPath))
         ).WithDimensions(dimBuilder =>
         {
             dimBuilder.SetPaperSize(PaperSizes.A4)
                 .SetMargins(Margins.None)
                 .SetScale(.90)
                 .LandScape();
         });

     var request = await builder.BuildAsync();

     await _sharpClient.FireWebhookAndForgetAsync(request);
 }

```
## Merge 15 Urls to one pdf
*Builds a 30 page pdf by merging the front two pages of 15 news sites. Takes about a minute to complete*

```csharp
public async Task<Stream> CreateWorldNewsSummary()
{
    var sites = new[]
        {
            "https://www.nytimes.com", "https://www.axios.com/", "https://www.csmonitor.com",
            "https://www.wsj.com", "https://www.usatoday.com", "https://www.irishtimes.com",
            "https://www.lemonde.fr", "https://calgaryherald.com", "https://www.bbc.com/news/uk",
            "https://www.thehindu.com", "https://www.theaustralian.com.au",
            "https://www.welt.de", "https://www.cankaoxiaoxi.com",
            "https://www.novinky.cz", "https://www.elobservador.com.uy"
        }
        .Select(u => new Uri(u));

    var builders = CreateBuilders(sites);
    var requests = builders.Select(b => b.Build());

    return await ExecuteRequestsAndMerge(requests);
}

IEnumerable<UrlRequestBuilder> CreateBuilders(IEnumerable<Uri> uris)
{
    foreach (var uri in uris)
    {
        yield return new UrlRequestBuilder()
            .SetUrl(uri)
            .ConfigureRequest(req => { req.SetPageRanges("1-2"); })
            .AddHeaderFooter(docBuilder =>
            {
                docBuilder.SetHeader(GetHeadFoot(uri.Host.Replace("www.", string.Empty).ToUpper()))
                   .SetFooter(GetHeadFoot(uri.ToString()));
            })
            .WithDimensions(dimBuilder =>
            {
                dimBuilder.UseChromeDefaults()
                    .SetScale(.90)
                    .LandScape()
                    .MarginLeft(.5)
                    .MarginRight(.5);
            });
    }

    static string GetHeadFoot(string heading)
        => "<html><head> <style> body { font-size: 8rem; } h1 { margin-left: auto; margin-right: auto; } </style></head><body><h1>" +
           heading + "</h1></body></html>";
}

async Task<Stream> ExecuteRequestsAndMerge(IEnumerable<UrlRequest> requests)
{
    var tasks = requests.Select(r => _sharpClient.UrlToPdfAsync(r));
    var results = await Task.WhenAll(tasks);

    var mergeBuilder = new MergeBuilder()
        .WithAssets(b => { 
            b.AddItems(results.Select((r, i) => KeyValuePair.Create($"{i}.pdf", r))); 
        });

    var request = mergeBuilder.Build();
    return await _sharpClient.MergePdfsAsync(request);
}
``` 
