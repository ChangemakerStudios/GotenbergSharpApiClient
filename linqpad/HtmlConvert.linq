<Query Kind="Program">
  <Reference Relative="..\lib\bin\Debug\netstandard2.1\Gotenberg.Sharp.API.Client.dll">C:\dev\Open\GotenbergSharpApiClient\lib\bin\Debug\netstandard2.1\Gotenberg.Sharp.API.Client.dll</Reference>
  <Namespace>Gotenberg.Sharp.API.Client</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Domain.Builders</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Extensions</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Domain.Builders.Faceted</Namespace>
</Query>


static string ResourcePath = @$"{Path.GetDirectoryName(Util.CurrentQueryPath)}\Resources\Html\ConvertExample\";
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
	   .AddAsyncDocument(async doc => 
			doc.SetBody(await GetHtmlFile("body.html"))
			   .SetFooter(await GetHtmlFile("footer.html"))
		).WithDimensions(dims => dims.UseChromeDefaults())
		.WithAsyncAssets(async
			assets => assets.AddItem("ear-on-beach.jpg", await GetImageBytes())
		).SetConversionBehaviors(b => 
			b.AddAdditionalHeaders("hello", "from-earth")
		).ConfigureRequest(b=> b.SetPageRanges("1"));

	var request = await builder.BuildAsync();

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
	return File.ReadAllBytesAsync($@"{ResourcePath}\ear-on-beach.jpg");
}

static Task<byte[]> GetHtmlFile(string fileName)
{
	return File.ReadAllBytesAsync($@"{ResourcePath}\{fileName}");
}