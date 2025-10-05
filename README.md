<h1>
<img src="https://raw.githubusercontent.com/ChangemakerStudios/GotenbergSharpApiClient/refs/heads/develop/resources/gotenberg-sharp-client.png" width="48" height="48" align="top" /> Gotenberg Sharp API Client
</h1>

[![NuGet version](https://badge.fury.io/nu/Gotenberg.Sharp.Api.Client.svg)](https://badge.fury.io/nu/Gotenberg.Sharp.Api.Client)
[![Downloads](https://img.shields.io/nuget/dt/Gotenberg.Sharp.API.Client.svg?logo=nuget&color=purple)](https://www.nuget.org/packages/Gotenberg.Sharp.API.Client) 
![Build status](https://github.com/ChangemakerStudios/GotenbergSharpApiClient/actions/workflows/deploy.yml/badge.svg)

⭐ For Gotenberg v7 & v8 ⭐

.NET C# Client for interacting with the [Gotenberg](https://gotenberg.dev/) v7 & v8 micro-service's API. [Gotenberg](https://github.com/gotenberg/gotenberg) is a [Docker-powered stateless API](https://hub.docker.com/r/gotenberg/gotenberg/) for converting & merging HTML, Markdown and Office documents to PDF. The client supports a configurable [Polly](http://www.thepollyproject.org/) **retry policy** with exponential backoff for handling transient exceptions.

# Getting Started

## Using Docker Run
*Pull the image from dockerhub.com*
```powershell
> docker pull gotenberg/gotenberg:latest
```
*Create & start a container*
```powershell
docker run --name gotenbee8x --rm -p 3000:3000 gotenberg/gotenberg:latest gotenberg --api-timeout=1800s --log-level=debug
```

## Using Docker Compose (with Basic Auth)
For local development with basic authentication enabled, use the provided docker-compose file:

```powershell
docker-compose -f docker/docker-compose-basic-auth.yml up -d
```

Pre-configured with test credentials:
- **Username:** `testuser`
- **Password:** `testpass`

# .NET Core Project Setup
*Install nuget package into your project*
```powershell
PM> Install-Package Gotenberg.Sharp.Api.Client
```

*Note: Use v1.x nugets for Gotenberg v6.*

## IntelliSense Documentation
All public APIs include comprehensive XML documentation with clear descriptions, parameter details, and links to official Gotenberg documentation. IntelliSense provides:
- Method descriptions with Gotenberg route documentation links
- Parameter explanations and valid value ranges
- Exception documentation for error handling
- Usage notes and best practices

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

### Optional: Basic Authentication
**Gotenberg v8+** - If your Gotenberg instance is configured with basic authentication (using `--api-enable-basic-auth`), you can provide credentials in the settings:

```json
  "GotenbergSharpClient": {
    "ServiceUrl": "http://localhost:3000",
    "HealthCheckUrl": "http://localhost:3000/health",
    "BasicAuthUsername": "your-username",
    "BasicAuthPassword": "your-password",
    "RetryPolicy": {
      "Enabled": true,
      "RetryCount": 4,
      "BackoffPower": 1.5,
      "LoggingEnabled": true
    }
  }
```

## Configure Services In Startup.cs

### Basic Configuration (using appsettings.json)
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

### Programmatic Configuration
```csharp
public void ConfigureServices(IServiceCollection services)
{
	.....
    // Configure with an action
    services.AddGotenbergSharpClient(options =>
    {
        options.ServiceUrl = new Uri("http://localhost:3000");
        options.TimeOut = TimeSpan.FromMinutes(5);
        options.BasicAuthUsername = "username";
        options.BasicAuthPassword = "password";
        // Configure retry policy
        options.RetryPolicy = new RetryPolicyOptions
        {
            Enabled = true,
            RetryCount = 4,
            BackoffPower = 1.5,
            LoggingEnabled = true
        };
    });
	.....
}
```

### Hybrid Configuration (appsettings + programmatic override)
```csharp
public void ConfigureServices(IServiceCollection services)
{
	.....
    services.AddOptions<GotenbergSharpClientOptions>()
	        .Bind(Configuration.GetSection(nameof(GotenbergSharpClient)));

    // Override or add settings programmatically
    services.AddGotenbergSharpClient(options =>
    {
        options.TimeOut = TimeSpan.FromMinutes(10); // Override timeout
        options.BasicAuthUsername = Environment.GetEnvironmentVariable("GOTENBERG_USER");
        options.BasicAuthPassword = Environment.GetEnvironmentVariable("GOTENBERG_PASS");
    });
	.....
}
```


# Using GotenbergSharpClient
*See the [examples folder](examples/)* for complete working examples as console applications.

## Required Using Statements
```csharp
using Gotenberg.Sharp.API.Client;
using Gotenberg.Sharp.API.Client.Domain.Builders;
using Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;
using Gotenberg.Sharp.API.Client.Domain.Requests.Facets; // For Cookie, etc.
```

### Html To Pdf 
*With embedded assets:*

```csharp
 [HttpGet]
 public async Task<ActionResult> HtmlToPdf([FromServices] GotenbergSharpClient sharpClient)
 {
     var builder = new HtmlRequestBuilder()
         .AddDocument(doc => 
             doc.SetBody(GetBody()).SetFooter(GetFooter())
         ).WithPageProperties(pp =>
         {
             pp.SetPaperSize(PaperSizes.A3)
                 .SetMargins(Margins.None)
                 .SetScale(.99);
         }).WithAsyncAssets(async assets => assets.AddItem("some-image.jpg", await GetImageBytes()));

     var req = await builder.BuildAsync();

     var result = await sharpClient.HtmlToPdfAsync(req);

     return this.File(result, "application/pdf", "gotenbergFromHtml.pdf");
 }
```

### Url To Pdf
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
		)).WithPageProperties(pp =>
		{
			pp.SetPaperSize(PaperSizes.A4)
			 .SetMargins(Margins.None)
			 .SetScale(.90)
			 .SetLandscape();
		});

	var request = await builder.BuildAsync();
	return await _sharpClient.UrlToPdfAsync(request);
}
```
## Merge Office Docs
*Merges office documents:*

```csharp
public async Task<Stream> DoOfficeMerge(string sourceDirectory)
{
	var builder = new MergeOfficeBuilder()
		.WithAsyncAssets(async a => a.AddItems(await GetDocsAsync(sourceDirectory)))
		.SetPdfFormat(LibrePdfFormats.A2b);

	var request = await builder.BuildAsync();
	return await _sharpClient.MergeOfficeDocsAsync(request);
}
```
### Markdown to Pdf
*Markdown to Pdf conversion with embedded assets:*

```csharp
public async Task<Stream> CreateFromMarkdown()
{
	var builder = new HtmlRequestBuilder()
		.AddAsyncDocument(async
			doc => doc.SetHeader(await this.GetHeaderAsync())
				  .SetBody(await GetBodyAsync())
				  .SetContainsMarkdown()
				  .SetFooter(await GetFooterAsync())
		).WithPageProperties(pp =>
		{
			pp.UseChromeDefaults().SetLandscape().SetScale(.90);
		}).WithAsyncAssets(async
			a => a.AddItems(await GetMarkdownAssets())
		);

	var request = await builder.BuildAsync();
	return await _sharpClient.HtmlToPdfAsync(request);
}
```
### Working with Cookies
*Add cookies to the Chromium cookie jar for authenticated requests (v2.8.1+):*

```csharp
public async Task<Stream> CreatePdfWithCookies()
{
	var builder = new UrlRequestBuilder()
		.SetUrl("https://example.com/protected")
		.SetConversionBehaviors(b =>
		{
			b.AddCookie(new Cookie
			{
				Name = "session_token",
				Value = "abc123xyz",
				Domain = "example.com",
				Path = "/",
				Secure = true,
				HttpOnly = true,
				SameSite = "Lax"
			});
		})
		.WithPageProperties(pp => pp.UseChromeDefaults());

	var request = await builder.BuildAsync();
	return await _sharpClient.UrlToPdfAsync(request);
}
```

### Document Metadata
*Add custom metadata to your PDFs (v2.6+):*

```csharp
public async Task<Stream> CreatePdfWithMetadata()
{
	var builder = new HtmlRequestBuilder()
		.AddDocument(doc => doc.SetBody("<html><body><h1>Document with Metadata</h1></body></html>"))
		.SetConversionBehaviors(b =>
		{
			b.SetMetadata(new Dictionary<string, object>
			{
				{ "Author", "John Doe" },
				{ "Title", "My Document" },
				{ "Subject", "Important Report" },
				{ "Keywords", "report, pdf, gotenberg" }
			});
		})
		.WithPageProperties(pp => pp.UseChromeDefaults());

	var request = await builder.BuildAsync();
	return await _sharpClient.HtmlToPdfAsync(request);
}
```

### PDF Format Conversion
*Convert PDFs to PDF/A formats (v2.8+):*

```csharp
public async Task<Stream> ConvertToPdfA(string pdfPath)
{
	var builder = new PdfConversionBuilder()
		.WithPdfs(b => b.AddItem("document.pdf", File.ReadAllBytes(pdfPath)))
		.SetPdfFormat(LibrePdfFormats.A2b);

	var request = await builder.BuildAsync();
	return await _sharpClient.ConvertPdfDocumentsAsync(request);
}
```

### Single Page Output
*Generate a single-page PDF from multi-page content (v2.8.1+):*

```csharp
public async Task<Stream> CreateSinglePagePdf()
{
	var builder = new UrlRequestBuilder()
		.SetUrl("https://www.example.com")
		.WithPageProperties(pp =>
		{
			pp.UseChromeDefaults()
			  .SetSinglePage(true);
		});

	var request = await builder.BuildAsync();
	return await _sharpClient.UrlToPdfAsync(request);
}
```
### Webhook
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
         ).WithPageProperties(pp =>
         {
             pp.SetPaperSize(PaperSizes.A4)
                 .SetMargins(Margins.None)
                 .SetScale(.90)
                 .SetLandscape();
         });

     var request = await builder.BuildAsync();

     await _sharpClient.FireWebhookAndForgetAsync(request);
 }

```
### Merge 15 Urls to one pdf
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
            .WithPageProperties(pp =>
            {
                pp.UseChromeDefaults()
                    .SetScale(.90)
                    .SetLandscape()
                    .SetMarginLeft(.5)
                    .SetMarginRight(.5);
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

## Advanced Features

### PDF/UA Support for HTML Conversions
*Enable Universal Access for accessible PDFs from HTML (v2.4+):*

```csharp
public async Task<Stream> CreateAccessiblePdf()
{
	var builder = new HtmlRequestBuilder()
		.AddDocument(doc => doc.SetBody("<html><body><h1>Accessible Document</h1></body></html>"))
		.SetConversionBehaviors(b => b.SetPdfUa(true))
		.WithPageProperties(pp => pp.UseChromeDefaults());

	var request = await builder.BuildAsync();
	return await _sharpClient.HtmlToPdfAsync(request);
}
```

### PDF/UA for PDF Conversions
*Enable Universal Access when converting PDFs to PDF/A (v2.4+):*

```csharp
public async Task<Stream> ConvertToAccessiblePdfA(string pdfPath)
{
	var builder = new PdfConversionBuilder()
		.WithPdfs(b => b.AddItem("document.pdf", File.ReadAllBytes(pdfPath)))
		.SetPdfFormat(LibrePdfFormats.A2b)
		.EnablePdfUa(true);

	var request = await builder.BuildAsync();
	return await _sharpClient.ConvertPdfDocumentsAsync(request);
}
```

### Flatten PDFs
*Flatten PDF forms and annotations (v2.8+):*

```csharp
public async Task<Stream> FlattenPdf(string pdfPath)
{
	var builder = new PdfConversionBuilder()
		.WithPdfs(b => b.AddItem("form.pdf", File.ReadAllBytes(pdfPath)))
		.EnableFlatten(true);

	var request = await builder.BuildAsync();
	return await _sharpClient.ConvertPdfDocumentsAsync(request);
}
```

### Skip Network Idle Event
*Speed up conversions by skipping network idle wait (Gotenberg v8+ only):*

```csharp
public async Task<Stream> FastConversion()
{
	var builder = new UrlRequestBuilder()
		.SetUrl("https://example.com")
		.SetConversionBehaviors(b => b.SkipNetworkIdleEvent())
		.WithPageProperties(pp => pp.UseChromeDefaults());

	var request = await builder.BuildAsync();
	return await _sharpClient.UrlToPdfAsync(request);
}
```

### Custom Page Properties
*Fine-tune page dimensions and properties:*

```csharp
public async Task<Stream> CustomPageProperties()
{
	var builder = new HtmlRequestBuilder()
		.AddDocument(doc => doc.SetBody("<html><body><h1>Custom Page</h1></body></html>"))
		.WithPageProperties(pp =>
		{
			pp.SetPaperSize(PaperSizes.Letter)
			  .SetMargins(Margins.Normal)
			  .SetScale(0.95)
			  .SetLandscape()
			  .SetPrintBackground(true)
			  .SetGenerateDocumentOutline(true)
			  .SetOmitBackground(false);
		});

	var request = await builder.BuildAsync();
	return await _sharpClient.HtmlToPdfAsync(request);
}
```
