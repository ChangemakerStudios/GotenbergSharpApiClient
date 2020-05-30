# <img src="https://github.com/ChangemakerStudios/GotenbergSharpApiClient/raw/master/lib/Resources/gotenbergSharpClient.PNG" width="24" height="24" /> Gotenberg.Sharp.Api.Client

[![NuGet version](https://badge.fury.io/nu/Gotenberg.Sharp.Api.Client.svg)](https://badge.fury.io/nu/Gotenberg.Sharp.Api.Client) [![Build status](https://ci.appveyor.com/api/projects/status/s8lvj93xewlsylxh/branch/master?svg=true)](https://ci.appveyor.com/project/Jaben/gotenbergsharpapiclient/branch/master)

.NET C# Client for interacting with the [Gotenberg](https://thecodingmachine.github.io/gotenberg) micro-service's API. [Gotenberg](https://github.com/thecodingmachine/gotenberg) is a [Docker-powered stateless API](https://hub.docker.com/r/thecodingmachine/gotenberg) for converting & merging HTML, Markdown and Office documents to PDF.

# Getting Started
*Pull the image from dockerhub.com*
```powershell
> docker pull thecodingmachine/gotenberg:latest
```
*Create & start a container*
```powershell
docker run --name gotenbee -e DEFAULT_WAIT_TIMEOUT=1800 -e MAXIMUM_WAIT_TIMEOUT=1800 -e LOG_LEVEL=DEBUG -p:3000:3000 "thecodingmachine/gotenberg:latest"
```
# .NET Core Project Setup
*Install nuget package into your project*
```powershell
PM> Install-Package Gotenberg.Sharp.Api.Client
```
## AppSettings
```json
  "GotenbergSharpClient": {
    "ServiceUrl": "http://localhost:3000",
    "HealthCheckUrl": "http://localhost:3000/ping",
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
	        .Bind(Configuration.GetSection("GotenbergSharpClient"));
    services.AddGotenbergSharpClient();
	.....    
}

```
# Using GotenbergSharpClient
*See the [linqPad folder](linqpad/)* for complete examples

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
         }).WithAsyncAssets(async assets => assets.AddItem("some-image.jpg", await GetImageBytes()))
         .ConfigureRequest(config =>
         {
             config.ChromeRpccBufferSize(1024)
                 .PageRanges("1");
         });

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
		.ConfigureRequest(b =>
		{
			b.PageRanges("1-2").ChromeRpccBufferSize(1048576);
		})
		.AddAsyncHeaderFooter(async
			b => b.SetHeader(await File.ReadAllTextAsync(headerPath))
				  .SetFooter(await File.ReadAllBytesAsync(footerPath)
		)).WithDimensions(b =>
		{
			b.SetPaperSize(PaperSizes.A4)
			 .SetMargins(Margins.None)
			 .SetScale(.90)
			 .LandScape();
		});

	var request = await builder.BuildAsync();
	return await sharpClient.UrlToPdfAsync(request);
}
```
## Merge Office Docs
*Merges office documents and configures the request time-out:*

```csharp
public async Task<Stream> DoOfficeMerge(string sourceDirectory)
{
	var builder = new MergeOfficeBuilder()
		.WithAsyncAssets(async b => b.AddItems(await GetDocsAsync(sourceDirectory)))
		.ConfigureRequest(b =>
		{
			b.TimeOut(100);
		});

	var request = await builder.BuildAsync();
	return await sharpClient.MergeOfficeDocsAsync(request);
}
```
## Markdown to Pdf
*Markdown to Pdf conversion with embedded assets:*

```csharp
public async Task<Stream> CreateFromMarkdown()
{
	var builder = new HtmlRequestBuilder()
		.AddAsyncDocument(async
			b => b.SetHeader(await this.GetHeaderAsync())
				  .SetBody(await GetBodyAsync())
				  .ContainsMarkdown()
				  .SetFooter(await GetFooterAsync())
		).WithDimensions(b =>
		{
			b.UseChromeDefaults().LandScape().SetScale(.90);
		}).WithAsyncAssets(async
			b => b.AddItems(await GetMarkdownAssets())
		).ConfigureRequest(b =>
		{
			b.ChromeRpccBufferSize(1048555);
		});

	var request = await builder.BuildAsync();
	return await sharpClient.HtmlToPdfAsync(request);
}
```
## Webhooks
*All requests support webhooks*
```csharp
public async Task SendUrlToWebhookEndpoint(string headerPath, string footerPath)
{
	var builder = new UrlRequestBuilder()
		.SetUrl("https://www.cnn.com")
		.ConfigureRequest(b =>
		{
			b.AddWebhook(hook =>
			{
				hook.SetTimeout(20)
					.SetUrl("http://host.docker.internal:5000/api/your/webhookReceiver")
					.AddRequestHeader("custom-header", "value");
			})
			.PageRanges("1-2")
			.ChromeRpccBufferSize(1048576);
		})
		.AddAsyncHeaderFooter(async
			b => b.SetHeader(await File.ReadAllTextAsync(headerPath))
				  .SetFooter(await File.ReadAllBytesAsync(footerPath)
		)).WithDimensions(b =>
		{
			b.SetPaperSize(PaperSizes.A4)
			 .SetMargins(Margins.None)
			 .SetScale(.90)
			 .LandScape();
		});

	var request = await builder.BuildAsync();
	
	await sharpClient.FireWebhookAndForgetAsync(request);
}

```
## Merge 15 Urls to one pdf
*Builds a 30 page pdf by merging the front two pages of 15 news sites. Takes about a minute to complete*

```csharp
public async Task<Stream> CreateWorldNewsSummary()
{
	var sites = new[] {"https://www.nytimes.com","https://www.axios.com/", "https://www.csmonitor.com",
		"https://www.wsj.com", "https://www.usatoday.com",  "https://www.irishtimes.com", 
		"https://www.lemonde.fr", "https://calgaryherald.com", "https://www.bbc.com/news/uk", 
		"https://www.thehindu.com", "https://www.theaustralian.com.au", 
		"https://www.welt.de", "https://www.cankaoxiaoxi.com", 
		"https://www.novinky.cz","https://www.elobservador.com.uy"}
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
			.SetRemoteUrlHeader("gotenberg-sharp-news-summary", $"{DateTime.Now.ToShortDateString()}")
			.ConfigureRequest(b =>
			{
			    b.PageRanges("1-2");
			}).AddHeaderFooter(b =>
			{
				b.SetHeader(GetHeadFoot(uri.Host.Replace("www.", string.Empty).ToUpper()))
				 .SetFooter(GetHeadFoot(uri.ToString()));
			} )
			.WithDimensions(b =>
			{
				b.UseChromeDefaults()
				.SetScale(.90)
				.LandScape()
				.MarginLeft(.5)
				.MarginRight(.5);
			});
	}

	static string GetHeadFoot(string heading)
		=> "<html><head> <style> body { font-size: 8rem; } h1 { margin-left: auto; margin-right: auto; } </style></head><body><h1>" + heading + "</h1></body></html>";
}

async Task<Stream> ExecuteRequestsAndMerge(IEnumerable<UrlRequest> requests)
{
	var sharpClient = new GotenbergSharpClient("http://localhost:3000");
	
	var tasks = requests.Select(r => sharpClient.UrlToPdfAsync(r));
	var results = await Task.WhenAll(tasks);

	var mergeBuilder = new MergeBuilder()
		.WithAssets(b => {
		    b.AddItems(results.Select((r, i) => KeyValuePair.Create($"{i}.pdf", r)));
		})
		.ConfigureRequest(b=> b.TimeOut(1799));
	
	var request = mergeBuilder.Build();
	return await sharpClient.MergePdfsAsync(request);
}
```