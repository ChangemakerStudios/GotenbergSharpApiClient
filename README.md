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

    var builder = new PdfRequestBuilder().Document
		.AddBody(GetBody())
        .AddFooter(GetFooter())
        .ConfigureRequest.ChromeRpccBufferSize(1048555).Parent
        .Dimensions.UseChromeDefaults()
        .SetScale(.75)
        .LandScape().Parent
        .Assets.AddItem("ear-on-beach.jpg", await GetImageBytes()).Parent;
	 
	var response = await sharpClient.ToPdfAsync(builder.Build());

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
					h1, h3{ text-align: center; } 
					figure { width:548px; height:600px; } 
					figure img { border: 10px solid #000; } 
					figcaption { text-align: right; font-size: 10pt; } 
					figcaption > a:link, a:visited { color: black; }
				</style>
				<head><meta charset=""utf-8""><title>Thanks to TheCodingMachine</title></head>  
				<body>
					<h1>Hello world</h1>
					<div>
						<figure>
						    <img src=""ear-on-beach.jpg"">
        	                <figcaption>Photo by <a href=""https://www.moma.org/artists/740"">Bill Brandt</a>.</figcaption>
						 </figure>   
					</div>
					<h3>Powered by Gotenberg</h3>
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
        .ConfigureRequest.PageRanges("1-1").Parent
        .Document.AddHeader("<html><head> <style> body { font-size: 8rem; } h1 { margin-left: auto; margin-right: auto; } </style></head><body><h1>Header</h1> </body></html>")
        .AddFooter("<html><head> <style> body { font-size: 8rem; } h1 { margin-left: auto; margin-right: auto; } </style></head><body><h1>Footer</h1></body></html>")
        .Parent.Dimensions
        .UseChromeDefaults()
        .SetScale(.90)
        .LandScape().Parent;

	var response = await sharpClient.UrlToPdf(builder.Build());

	var platformAwareSlash = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @"\" : "/";
	var outPath = @$"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}{platformAwareSlash}GotenbergFromUrl.pdf";

	using (var destinationStream = File.Create(outPath))
	{
		await response.CopyToAsync(destinationStream);
	}

	return outPath;
}
```

## Scenario 3 Markdown
*Markdown to Pdf conversion with embedded assets:*

```csharp
async Task<string> MarkdownPdf()
{
	var sharpClient = new GotenbergSharpClient("http://localhost:3000");

    var builder = new PdfRequestBuilder(hasMarkdown:true)
        .Document
        .AddBody(await GetBody())
        .AddHeader(await GetHeader())
        .AddFooter(await GetFooter())
        .Assets.AddItems(await GetMarkdownAssets()).Parent
        .Dimensions.UseChromeDefaults()
        .SetScale(.85)
        .LandScape()
        .Parent.ConfigureRequest.ChromeRpccBufferSize(558576);
				
	var response = await sharpClient.ToPdfAsync(builder.Parent.Build());

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
