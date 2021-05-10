<Query Kind="Program">
  <NuGetReference Version="1.0.0">Gotenberg.Sharp.API.Client</NuGetReference>
  <Namespace>Gotenberg.Sharp.API.Client</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Domain.Builders</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Extensions</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Infrastructure</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

async Task Main()
{
	var path = await CreateFromMarkdown(@"D:\Gotenberg\Dumps");
	path.Dump("Done");
}
public async Task<string> CreateFromMarkdown(string destinationDirectory)
{
	var sharpClient = new GotenbergSharpClient("http://localhost:3000");

	var builder = new HtmlRequestBuilder()
		.AddAsyncDocument(async
			b => b.SetHeader(await this.GetHeaderAsync())
				  .SetBody(await GetBodyAsync())
				  .ContainsMarkdown()
				  .SetFooter(await GetFooterAsync())
		).WithDimensions(b =>
		{
			b.UseChromeDefaults()
			 .LandScape()
			 .SetScale(.90);
		}).WithAsyncAssets(async
			b => b.AddItems(await GetMarkdownAssets())
		).ConfigureRequest(b =>
		{
			b.ChromeRpccBufferSize(1048555)
			 .ResultFileName("hello.pdf");
		});

	var request = await builder.BuildAsync();
	var response = await sharpClient.HtmlToPdfAsync(request);

	var outPath = @$"{destinationDirectory}\GotenbergFromMarkDown.pdf";

	using (var destinationStream = File.Create(outPath))
	{
		await response.CopyToAsync(destinationStream);
	}

	return outPath;
}

async Task<string> GetBodyAsync()
{
	var body = await new HttpClient().GetStringAsync("https://raw.githubusercontent.com/thecodingmachine/gotenberg-php-client/master/tests/assets/markdown/index.html");
	return body;
}

async Task<string> GetHeaderAsync()
{
	return await new HttpClient().GetStringAsync("https://raw.githubusercontent.com/thecodingmachine/gotenberg-php-client/master/tests/assets/markdown/header.html");
}

async Task<string> GetFooterAsync()
{
	return await new HttpClient().GetStringAsync("https://raw.githubusercontent.com/thecodingmachine/gotenberg-php-client/master/tests/assets/markdown/footer.html");
}

async Task<IEnumerable<KeyValuePair<string, string>>> GetMarkdownAssets()
{
	var bodyAssetNames = new[] {"img.gif" , "font.woff", "style.css" };
	var markdownFiles = new[] { "paragraph1.md", "paragraph2.md", "paragraph3.md" };

	var client = new HttpClient() { BaseAddress = new Uri("https://raw.githubusercontent.com/thecodingmachine/gotenberg-php-client/master/tests/assets/markdown/") };

	var bodyAssetTasks = bodyAssetNames.Select(ba => client.GetStringAsync(ba));
	var mdTasks = markdownFiles.Select(md => client.GetStringAsync(md));

	var bodyAssets = await Task.WhenAll(bodyAssetTasks);
	var mdParagraphs = await Task.WhenAll(mdTasks);

	return bodyAssetNames.Select((name, index) => new KeyValuePair<string, string>(name, bodyAssets[index]))
			   .Concat(markdownFiles.Select((name, index) => new KeyValuePair<string,string>(name, mdParagraphs[index])));
}