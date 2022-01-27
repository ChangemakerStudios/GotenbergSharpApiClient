<Query Kind="Program">
  <Reference Relative="..\lib\bin\Debug\netstandard2.1\Gotenberg.Sharp.API.Client.dll">..\GotenbergSharpApiClient\lib\bin\Debug\netstandard2.1\Gotenberg.Sharp.API.Client.dll</Reference>
  <Namespace>Gotenberg.Sharp.API.Client</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Domain.Builders</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Domain.Builders.Faceted</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Domain.Requests</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

static Random Rando = new Random(Math.Abs( (int) DateTime.Now.Ticks));

async Task Main()
{
	var destinationPath = @"D:\Gotenberg\Dumps";
 	var headerFooterPath = @"D:\Gotenberg\Resources\Html";
	
	//Looks like V7 doesn't support head/footers for Url conversions.
 	var file = await CreateFromUrl(destinationPath, @$"{headerFooterPath}\UrlHeader.html", @$"{headerFooterPath}\UrlFooter.html" );
	file.Dump();
}

public async Task<string> CreateFromUrl(string destinationPath, string headerPath, string footerPath)
{
	var sharpClient = new GotenbergSharpClient("http://localhost:3000");

	var builder = new UrlRequestBuilder()
		.SetUrl("https://www.cnn.com")
		.ConfigureRequest(b =>
		{
			b.PageRanges("1-2");
		})
		.AddAsyncHeaderFooter(async
			b => b.SetHeader(await File.ReadAllBytesAsync(headerPath))
				  .SetFooter(await File.ReadAllBytesAsync(footerPath)
		)).WithDimensions(b =>
		{
			b.SetPaperSize(PaperSizes.A4)
			.SetMargins(Margins.None)
			 .LandScape()
			 .SetScale(.90);
		});

	var request = await builder.BuildAsync();
	var response = await sharpClient.UrlToPdfAsync(request);

	var resultPath = @$"{destinationPath}\GotenbergFromUrl-{Rando.Next()}.pdf";

	using (var destinationStream = File.Create(resultPath))
	{
		await response.CopyToAsync(destinationStream);
	}

	return resultPath;
}