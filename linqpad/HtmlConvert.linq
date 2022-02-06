<Query Kind="Program">
  <Reference Relative="..\lib\bin\Debug\netstandard2.1\Gotenberg.Sharp.API.Client.dll">C:\dev\Open\GotenbergSharpApiClient\lib\bin\Debug\netstandard2.1\Gotenberg.Sharp.API.Client.dll</Reference>
  <Namespace>Gotenberg.Sharp.API.Client</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Domain.Builders</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Extensions</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Domain.Builders.Faceted</Namespace>
</Query>

static bool DumpHttpContent = false;

static Random Rand = new Random(Math.Abs( (int) DateTime.Now.Ticks));

async Task Main()
{
	var path = await CreateFromHtml(@"D:\Gotenberg\Dumps");
	
	var info = new ProcessStartInfo { FileName = path, UseShellExecute = true };
	Process.Start(info);
	
	path.Dump("Done");
}

public async Task<string> CreateFromHtml(string destinationDirectory)
{
	var sharpClient = new GotenbergSharpClient("http://localhost:3000");

	var builder = new HtmlRequestBuilder()
		.AddDocument(doc => 
			doc.SetBody(GetBody())
			   .SetFooter(GetFooter())
		).WithDimensions(dims => dims.UseChromeDefaults())
		.WithAsyncAssets(async
			assets => assets.AddItem("ear-on-beach.jpg", await GetImageBytes())
		).SetConversionBehaviors(b => 
			b.SetBrowserWaitDelay(1)
		).ConfigureRequest(b=> b.PageRanges("1"));

	var request = await builder.BuildAsync();

	if (DumpHttpContent) request.ToHttpContent().ToDumpFriendlyFormat().Dump("HttpConent X-Ray");

	var resultPath = @$"{destinationDirectory}\GotenbergFromHtml-{Rand.Next()}.pdf";
	var response = await sharpClient.HtmlToPdfAsync(await builder.BuildAsync());

	using (var destinationStream = File.Create(resultPath))
	{
		await response.CopyToAsync(destinationStream);
	}
	
	return resultPath;
}

static Task<byte[]> GetImageBytes()
{
	return new HttpClient().GetByteArrayAsync("http://4.bp.blogspot.com/-jdRdVRheb74/UlLHPkWs99I/AAAAAAAAAJc/lbJEG0KwfgI/s1600/bill-brandt-31.jpg");
}

static string GetBody()
{
	return @"<!doctype html>
			<html lang=""en"">
    			<style>
					body { max-width: 700px;  margin: auto;} h1 { font-size: 55px; }
					h1, h3{ text-align: center; margin-top: 5px; } .photo-container { padding-bottom: 20px;  }
					figure { width:548px; height:600px; } figure img { border: 10px solid #000; } 
					figcaption { text-align: right; font-size: 10pt; } 	a:link, a:visited { color: black !important; }
				</style>
				<head><meta charset=""utf-8""><title>Thanks to TheCodingMachine</title></head>  
				<body>
					<h1>Hello World</h1>
					<div class=""photo-container"">
						<figure>
						    <img src=""ear-on-beach.jpg"">
        	                <figcaption>Photo by <a href=""https://www.moma.org/artists/740"">Bill Brandt</a>.</figcaption>
						 </figure>   
					</div>
					<h3>Powered by <a href=""https://hub.docker.com/r/thecodingmachine/gotenberg"">The Coding Machine</a></h3>
				</body>
			</html>";
}

static string GetFooter()
{
	return @"<html><head><style>body { font-size: 8rem; font-family: Roboto,""Helvetica Neue"",Arial,sans-serif; margin: 4rem auto; }  </style></head>
	<body><p><span class=""pageNumber""></span> of <span class=""totalPages""> pages</span> PDF Created on <span class=""date""></span> <span class=""title""></span></p></body></html>";
}