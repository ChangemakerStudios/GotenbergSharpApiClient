# <img src="https://github.com/ChangemakerStudios/GotenbergSharpApiClient/raw/master/lib/Resources/gotenbergSharpClient.PNG" width="24" height="24" /> Gotenberg.Sharp.Api.Client

[![NuGet version](https://badge.fury.io/nu/Gotenberg.Sharp.Api.Client.svg)](https://badge.fury.io/nu/Gotenberg.Sharp.Api.Client) [![Build status](https://ci.appveyor.com/api/projects/status/s8lvj93xewlsylxh/branch/master?svg=true)](https://ci.appveyor.com/project/Jaben/gotenbergsharpapiclient/branch/master)

.NET C# Client for interacting with the [Gotenberg](https://thecodingmachine.github.io/gotenberg) micro-service's API, version 6.x.
[Gotenberg](https://thecodingmachine.github.io/gotenberg) is a Docker-powered stateless API for converting HTML, Markdown and Office documents to PDF.

## Getting started
*Install the Gotenberg.Sharp.Api.Client package from Visual Studio's NuGet console:*

```powershell
PM> Install-Package Gotenberg.Sharp.Api.Client
```

*Start up Gotenberg Docker Instance:*

```powershell
docker run --name gotenbee -e DEFAULT_WAIT_TIMEOUT=1800 -e MAXIMUM_WAIT_TIMEOUT=1800 -e LOG_LEVEL=DEBUG -p:3000:3000 "thecodingmachine/gotenberg:latest"
```

## Usage
*Html to PDF conversion with embedded assets:*

```csharp
public async Task<string> BuildPdf()
{
	var sharpClient = new GotenbergSharpClient("http://localhost:3000");

	var builder = new PdfRequestBuilder().Document
			    .AddBody(GetBody())
			    .AddFooter(GetFooter())
			    .SetChromeDimensionDefaults()
			    .Dimensions.MarginLeft(.5)
			    .Dimensions.MarginRight(.5)
			    .Document.AddAsset("mandala.png", await GetImageBytes());
	//Dims: Sets chrome's default dims and then over-writes margin left/right

	var response = await sharpClient.HtmlToPdfAsync(builder.Build());

	var outPath = @$"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\Gotenberg.pdf";

	using (var destinationStream = File.Create(outPath))
	{
		await response.CopyToAsync(destinationStream);
	}

	return outPath;
}

private async Task<byte[]> GetImageBytes()
{
	return await new HttpClient().GetByteArrayAsync(
		"https://bjc.berkeley.edu/~bh/bjc/bjc-r/img/2-complexity/Mandala_img/ColorMandala1.png");
}

private string GetBody()
{
	return @"<!doctype html>
			<html lang=""en"">
				<style> h1, h3{ text-align: center; } img { display: block; margin-left: auto;margin-right: auto; width: 88%;}  </style>
				<head><meta charset=""utf-8""><title>Thanks to TheCodingMachine</title></head>  
				<body>
					<h1>Hello world</h1>
					<img src=""mandala.png"">
					<h3>Powered by Gotenberg</h3>
				</body>
			</html>";
}

private string GetFooter()
{
	return
		@"<html><head><style>body { font-size: 8rem;margin: 4rem auto; }  </style></head><body><p><span class=""pageNumber""></span> of <span class=""totalPages""> pages</span> PDF Created on <span class=""date""></span> <span class=""title""></span></p></body></html>";
}
```

## Usage in .NET Core App
*Add an IServiceCollection extension:*

```csharp
public static IHttpClientBuilder AddTypedApiClient<TClient>(this IServiceCollection services, InnerClientSettings settings) where TClient: class 
 {
     if(settings == null) throw new ArgumentNullException(nameof(settings));
     if(settings.GetApiBaseUriFunc == null) throw new ArgumentException(nameof(settings.GetApiBaseUriFunc));

     return services.AddHttpClient(settings.ClientName)
                    .ConfigureHttpClient((sp, client) =>
                                         {
                                             client.Timeout = settings.Timeout;
                                             client.BaseAddress = settings.GetApiBaseUriFunc(sp);
                                         })
                    .AddTypedClient<TClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => new TimeoutHandler(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
                    .SetHandlerLifetime(settings.HandlerLifeTime);
                    //Also recommended: add a Polly Retry policy using https://www.nuget.org/packages/Polly
                    // https://github.com/App-vNext/Polly/wiki/Polly-and-HttpClientFactory
                    // https://www.stevejgordon.co.uk/httpclientfactory-using-polly-for-transient-fault-handling
}
```

*Add the client to the ServiceCollection in your Startup.cs:*

```csharp
public IServiceProvider ConfigureServices(IServiceCollection services)
{
	//add core services then
	services.AddTypedApiClient<GotenbergApiSharpClient>(CreateGotenbergClientSettings());

	return builder.Build();
}
```

*Inject it where you need it: (Code needs to be updated for breaking changes)*

```csharp
public class GenerateSomePdfService
{
	readonly GotenbergApiSharpClient _gotenbergClient;
	
	public GenerateSomePdfService(GotenbergApiSharpClient gotenbergClient)
	{
		_gotenbergClient = gotenbergClient;
	}

	public async Task<Stream> MakePdf(string body, string footer, CancellationToken cancelToken = default)
	{
		//Without using the builder:
		var dims = new DocumentDimensions { PaperWidth = 8.26, PaperHeight = 11.69, Landscape = false, MarginBottom = .38 };

		var content = new DocumentRequest
		{
			Body = new ContentItem(body),
			Footer = new ContentItem(footer)
		};

		var request = new PdfRequest { Content = content, Dimensions = dims };

		return await this._gotenbergClient.HtmlToPdfAsync(request, cancelToken).ConfigureAwait(false);
	}

}
```
