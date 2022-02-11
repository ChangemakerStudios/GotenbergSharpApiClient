<Query Kind="Program">
  <Reference Relative="..\lib\bin\Debug\netstandard2.1\Gotenberg.Sharp.API.Client.dll">C:\dev\Open\GotenbergSharpApiClient\lib\bin\Debug\netstandard2.1\Gotenberg.Sharp.API.Client.dll</Reference>
  <Namespace>Gotenberg.Sharp.API.Client</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Domain.Builders</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Domain.Builders.Faceted</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Domain.Requests</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Extensions</Namespace>
</Query>

static Random Rand = new Random(Math.Abs( (int) DateTime.Now.Ticks));

async Task Main()
{
	var destinationPath = @"D:\Gotenberg\Dumps";
 	var headerFooterPath = @$"{Path.GetDirectoryName(Util.CurrentQueryPath)}\Resources\Html";;

	var path = await CreateFromUrl(
		destinationPath, 
		@$"{headerFooterPath}\UrlHeader.html", 
		@$"{headerFooterPath}\UrlFooter.html");
	
	var info = new ProcessStartInfo { FileName = path, UseShellExecute = true };
	Process.Start(info);
	
	path.Dump("done");
}

public async Task<string> CreateFromUrl(string destinationPath, string headerPath, string footerPath)
{
	var sharpClient = new GotenbergSharpClient("http://localhost:3000");

	var builder = new UrlRequestBuilder()
		.SetUrl("https://www.cnn.com")
		.SetConversionBehaviors(b =>
		   b.EmulateAsScreen()
		    .SetBrowserWaitDelay(1)
			.SetUserAgent(nameof(GotenbergSharpClient))
		).ConfigureRequest(b => b.SetPageRanges("1-2"))
		.AddAsyncHeaderFooter(async
			b => b.SetHeader(await File.ReadAllBytesAsync(headerPath))
				  .SetFooter(await File.ReadAllBytesAsync(footerPath)
		)).WithDimensions(b =>
			b.SetPaperSize(PaperSizes.A4)
			 .UseChromeDefaults()
			 .MarginLeft(0)
			 .MarginRight(0)
		);

	var request = await builder.BuildAsync();

	var response = await sharpClient.UrlToPdfAsync(request);

	var resultPath = @$"{destinationPath}\GotenbergFromUrl-{Rand.Next()}.pdf";

	using (var destinationStream = File.Create(resultPath))
	{
		await response.CopyToAsync(destinationStream);
	}

	return resultPath;
}