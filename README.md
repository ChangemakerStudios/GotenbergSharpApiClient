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

## Scenario 1 
*Html to PDF conversion with embedded assets:*

```csharp
public async Task<string> HtmlToPdf()
{
    var sharpClient = new GotenbergSharpClient("http://localhost:3000");

	var builder = new HtmlRequestBuilder()
		.AddDocument(b => 
			b.SetBody(GetBody())
			 .SetFooter(GetFooter())
		).WithDimensions(b =>
		{
			b.UseChromeDefaults().LandScape().SetScale(.75);
		}).WithAsyncAssets(async
			b => b.AddItem("ear-on-beach.jpg", await GetImageBytes())
		).ConfigureRequest(b => {		
			b.ChromeRpccBufferSize(1024);
		});

	//You can also do this for async: 
	//	.AddAsyncDocument(async b => b.SetBody(await GetBodyAsync()).SetFooter(GetFooter()))

	var response = await sharpClient.ToPdfAsync(await builder.BuildAsync());

	var platformAwareSlash = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @"\" : "/";
	var outPath = @$"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}{platformAwareSlash}GotenbergFromHtml.pdf";

	using (var destinationStream = File.Create(outPath))
	{
		await response.CopyToAsync(destinationStream);
	}

	return outPath;
}

async Task<byte[]> GetImageBytes()
{
	return await new HttpClient().GetByteArrayAsync(
		"http://4.bp.blogspot.com/-jdRdVRheb74/UlLHPkWs99I/AAAAAAAAAJc/lbJEG0KwfgI/s1600/bill-brandt-31.jpg");
}

string GetBody()
{
	return @"<!doctype html>
			<html lang=""en"">
    			<style>
					body { max-width: 700px;  margin: auto;}
					h1 { font-size: 55px; }
					h1, h3{ text-align: center; margin-top: 5px; } 
					.photo-container { padding-bottom: 20px;  }
					figure { width:548px; height:600px; } 
					figure img { border: 10px solid #000; } 
					figcaption { text-align: right; font-size: 10pt; } 
					a:link, a:visited { color: black !important; }
				</style>
				<head><meta charset=""utf-8""><title>Thanks to TheCodingMachine</title></head>  
				<body>
					<h1>Hello world</h1>
					<div class=""photo-container"">
						<figure>
						    <img src=""ear-on-beach.jpg"">
        	                <figcaption>Photo by <a href=""https://www.moma.org/artists/740"">Bill Brandt</a>.</figcaption>
						 </figure>   
					</div>
					<h3>Powered by <a href=""https://hub.docker.com/r/thecodingmachine/gotenberg"">Gotenberg</a></h3>
				</body>
			</html>";
}

string GetFooter()
{
	return
		@"<html><head><style>body { font-size: 8rem; font-family: Roboto,""Helvetica Neue"",Arial,sans-serif; margin: 4rem auto; }  </style></head><body><p><span class=""pageNumber""></span> of <span class=""totalPages""> pages</span> PDF Created on <span class=""date""></span> <span class=""title""></span></p></body></html>";
}
```

## Scenario 2 
*Url to PDF conversion with custom header & footer:*

```csharp
public async Task<string> UrlToPdf()
{
    var sharpClient = new GotenbergSharpClient("http://localhost:3000");

	var builder = new UrlRequestBuilder()
		.SetUrl("https://www.nytimes.com")
		.ConfigureRequest(b =>
		{
			b.PageRanges("1-1").ChromeRpccBufferSize(3145728);
		})
		.AddAsyncHeaderFooter(async
			b => b.SetHeader(await File.ReadAllTextAsync(@"E:\GotenBerg\ProjectResources\UrlHeader.html"))
				  .SetFooter(File.ReadAllText(@"E:\GotenBergNotes\ProjectResources\UrlFooter.html")
		)).WithDimensions(b =>
		{
			b.UseChromeDefaults()
			 .SetScale(.90)
		  	 .LandScape();
		});

	var request = await builder.Build();
	var response = await sharpClient.UrlToPdf(request);

	var platformAwareSlash = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @"\" : "/";
	var outPath = @$"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}{platformAwareSlash}GotenbergFromUrl.pdf";

	using (var destinationStream = File.Create(outPath))
	{
		await response.CopyToAsync(destinationStream);
	}

	return outPath;
}
```

## Scenario 3 Merge multiple Url request results into one pdf
*Builds a 28 page pdf by merging the front two pages of several news sites. Takes about a minute to complete*

```csharp
public async Task<string> CreateWorldNewsSummary(string destinationDirectory)
{
	var sites = new[] {"https://www.nytimes.com","https://www.axios.com/", "https://www.csmonitor.com",
		"https://www.wsj.com", "https://www.usatoday.com",  "https://www.irishtimes.com", 
		"https://www.lemonde.fr", "https://calgaryherald.com", "https://www.bbc.com/news/uk", 
		"https://www.thehindu.com", "https://www.theaustralian.com.au", 
		"https://www.welt.de", "https://www.cankaoxiaoxi.com", 
		"https://www.novinky.cz","https://www.elobservador.com.uy"}
		.Select(u => new Uri(u));

	var builders = CreateRequestBuilders(sites);
	var requests = builders.Select(b => b.Build() );

	return await ExecuteRequestsAndMerge(requests, destinationDirectory);
}

IEnumerable<UrlRequestBuilder> CreateRequestBuilders(IEnumerable<Uri> uris)
{
	foreach (var uri in uris)
	{
		yield return new UrlRequestBuilder()
			.SetUrl(uri)
			.SetRemoteUrlHeader("NewSummary", $"{DateTime.Now.ToShortDateString()}")
			.ConfigureRequest(b =>
			{
			    b.PageRanges("1-2").ResultFileName($"{uri.Host}.pdf");
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

async Task<string> ExecuteRequestsAndMerge(IEnumerable<UrlRequest> requests, string destinationDirectory)
{
	var sharpClient = new GotenbergSharpClient("http://localhost:3000");
	
	var tasks = requests.Select(r => sharpClient.UrlToPdf(r));
	var results = await Task.WhenAll(tasks);

	var mergeBuilder = new MergeBuilder()
		.WithAssets(b => {
		    b.AddItems(results.Select((r, i) => KeyValuePair.Create($"{i}.pdf", r)));
		})
		.ConfigureRequest(b=> b.TimeOut(1799));
	
	var response = await sharpClient.MergePdfsAsync(mergeBuilder.Build());
	
	return await WriteFileAndGetPath(response, destinationDirectory);
}

async Task<string> WriteFileAndGetPath(Stream responseStream, string desinationDirectory)
{
	var fullPath = @$"{desinationDirectory}\{DateTime.Now.ToString("yyyy-MM-MMMM-dd")}.pdf";
	using (var destinationStream = File.Create(fullPath))
	{
		await responseStream.CopyToAsync(destinationStream);
	}
	return fullPath; 
}
```
## Scenario 4 Merge Office Docs
*Markdown to Pdf conversion with embedded assets:*

```csharp
async Task<string> DoOfficeMerge(string sourceDirectory, string destinationDirectory)
{
	var client = new GotenbergSharpClient("http://localhost:3000");

	var items = Directory.GetFiles(sourceDirectory, "*.*", SearchOption.TopDirectoryOnly)
				.Select(item => KeyValuePair.Create(new FileInfo(item).Name, File.ReadAllBytes(item)));

	var builder = new MergeOfficeBuilder()
		.WithAssets(b => 
		{
	   		b.AddItems(items);
	    }).ConfigureRequest(b =>
		{
			b.TimeOut(100);
	   });
	   
	var request = builder.Build();
	var response = await client.MergeOfficeDocsAsync(request).ConfigureAwait(false);

	var filePathAndName =@$"{destinationDirectory}\GotenbergOfficeMerge.pdf";
	using (var destinationStream = File.Create(filePathAndName))
	{
		await response.CopyToAsync(destinationStream).ConfigureAwait(false);
	}
	 
	return filePathAndName;
}

```
## Scenario 5 Markdown
*Markdown to Pdf conversion with embedded assets:*

```csharp
async Task<string> MarkdownPdf()
{
	var sharpClient = new GotenbergSharpClient("http://localhost:3000");

	var builder = new HtmlRequestBuilder(containsMarkdown: true)
		.AddAsyncDocument(async
			b => b.SetHeader(await this.GetHeaderAsync())
				  .SetBody(await GetBodyAsync())
				  .SetFooter(GetFooter())
		).WithDimensions(b =>
		{
			b.UseChromeDefaults().LandScape().SetScale(.90);
		}).WithAsyncAssets(async
			b => b.AddItems(await GetMarkdownAssets())
		).ConfigureRequest(b =>
		{
			b.ChromeRpccBufferSize(1048555)
			 .ResultFileName("hello.pdf");
		});

	var request = await builder.BuildAsync();
	var response = await sharpClient.ToPdfAsync(request);

	var platformAwareSlash = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @"\" : "/";
	var destination = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
	var outPath = @$"{destination}{platformAwareSlash}GotenbergFromMarkDown.pdf";

	using (var destinationStream = File.Create(outPath))
	{
		await response.CopyToAsync(destinationStream);
	}

	return outPath;
}

async Task<string> GetBody()
{
	var body = await new HttpClient().GetStringAsync("https://raw.githubusercontent.com/thecodingmachine/gotenberg-php-client/master/tests/assets/markdown/index.html");
	return body.Replace(@"<img src=""img.gif"">", string.Empty);
}

async Task<string> GetHeader()
{
	return await new HttpClient().GetStringAsync("https://raw.githubusercontent.com/thecodingmachine/gotenberg-php-client/master/tests/assets/markdown/header.html");
}

async Task<string> GetFooter()
{
	return await new HttpClient().GetStringAsync("https://raw.githubusercontent.com/thecodingmachine/gotenberg-php-client/master/tests/assets/markdown/footer.html");
}

async Task<IEnumerable<KeyValuePair<string, string>>> GetMarkdownAssets()
{
	var bodyAssetNames = new[] { "font.woff", "style.css", "img.gif" };
	var markdownFiles = new[] { "paragraph1.md", "paragraph2.md", "paragraph3.md" };

	var client = new HttpClient() { BaseAddress = new Uri("https://raw.githubusercontent.com/thecodingmachine/gotenberg-php-client/master/tests/assets/markdown/") };

	var bodyAssetTasks = bodyAssetNames.Select(ba => client.GetStringAsync(ba));
	var mdTasks = markdownFiles.Select(md => client.GetStringAsync(md));

	var bodyAssets = await Task.WhenAll(bodyAssetTasks);
	var mdParagraphs = await Task.WhenAll(mdTasks);

	return bodyAssetNames.Select((name, index) => KeyValuePair.Create(name, bodyAssets[index]))
			   .Concat(markdownFiles.Select((name, index) => KeyValuePair.Create(name, mdParagraphs[index])));
}
```
