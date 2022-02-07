<Query Kind="Program">
  <Reference Relative="..\lib\bin\Debug\netstandard2.1\Gotenberg.Sharp.API.Client.dll">C:\dev\Open\GotenbergSharpApiClient\lib\bin\Debug\netstandard2.1\Gotenberg.Sharp.API.Client.dll</Reference>
  <Namespace>Gotenberg.Sharp.API.Client</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Domain.Builders</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Extensions</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Infrastructure</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>


static Random Rand = new Random(Math.Abs( (int) DateTime.Now.Ticks));
static string ResourcePath = @$"{Path.GetDirectoryName(Util.CurrentQueryPath)}\Resources\Markdown";

async Task Main()
{
	var path = await CreateFromMarkdown(@"D:\Gotenberg\Dumps");
	
	var info = new ProcessStartInfo { FileName = path, UseShellExecute = true };
	Process.Start(info);
	
	path.Dump("done");
}

public async Task<string> CreateFromMarkdown(string destinationDirectory)
{
	var sharpClient = new GotenbergSharpClient("http://localhost:3000");

	var builder = new HtmlRequestBuilder()
		.AddAsyncDocument(async
			b => b.SetHeader(await GetFile("header.html"))
				  .SetBody(await GetFile("index.html"))
				  .ContainsMarkdown()
				  .SetFooter(await GetFile("footer.html"))
		).WithDimensions(b =>
		{
			b.UseChromeDefaults()
			 .LandScape()
			 .SetScale(.90);
		}).WithAsyncAssets(async
			b => b.AddItems(await GetMarkdownAssets())
		).ConfigureRequest(b => b.ResultFileName("hello.pdf")
		).SetConversionBehaviors(b => b.SetBrowserWaitDelay(2));

	var request = await builder.BuildAsync();

	var response = await sharpClient.HtmlToPdfAsync(request);

	var outPath = @$"{destinationDirectory}\GotenbergFromMarkDown-{Rand.Next()}.pdf";

	using (var destinationStream = File.Create(outPath))
	{
		await response.CopyToAsync(destinationStream);
	}

	return outPath;
}

async Task<string> GetFile(string fileName)
	=> await File.ReadAllTextAsync(@$"{ResourcePath}\{fileName}");

async Task<IEnumerable<KeyValuePair<string, string>>> GetMarkdownAssets()
{
	var bodyAssetNames = new[] {"img.gif" , "font.woff", "style.css" };
	var markdownFiles = new[] { "paragraph1.md", "paragraph2.md", "paragraph3.md" };

	var bodyAssetTasks = bodyAssetNames.Select(ba => GetFile(ba));
	var mdTasks = markdownFiles.Select(md => GetFile(md));

	var bodyAssets = await Task.WhenAll(bodyAssetTasks);
	var mdParagraphs = await Task.WhenAll(mdTasks);

	return bodyAssetNames.Select((name, index) => KeyValuePair.Create(name, bodyAssets[index]))
			   .Concat(markdownFiles.Select((name, index) => KeyValuePair.Create(name, mdParagraphs[index])));
}