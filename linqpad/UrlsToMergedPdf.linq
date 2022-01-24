<Query Kind="Program">
  <Reference Relative="..\lib\bin\Debug\netstandard2.1\Gotenberg.Sharp.API.Client.dll">C:\dev\Open\GotenbergSharpApiClient\lib\bin\Debug\netstandard2.1\Gotenberg.Sharp.API.Client.dll</Reference>
  <Namespace>Gotenberg.Sharp.API.Client</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Domain.Builders</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Domain.Builders.Faceted</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Domain.Requests</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

async Task Main()
{
	var path = await CreateWorldNewsSummary($@"D:\NewsArchive");
	var info = new ProcessStartInfo{ FileName = path, UseShellExecute = true};
	Process.Start(info);
	path.Dump();
}

public async Task<string> CreateWorldNewsSummary(string destinationDirectory)
{
	var sites = new[] {"https://www.nytimes.com","https://www.axios.com/","https://www.cnn.com",  "https://www.csmonitor.com",
		"https://www.wsj.com", "https://www.usatoday.com",  "https://www.irishtimes.com",
		"https://www.lemonde.fr", "https://calgaryherald.com", "https://www.bbc.com/news/uk",
		"https://english.elpais.com/", 	"https://www.thehindu.com", "https://www.theaustralian.com.au",
		"https://www.welt.de", "https://www.cankaoxiaoxi.com", "https://www.novinky.cz","https://www.elobservador.com.uy"}
		.Select(u => new Uri(u))
		.Take(5);
	
	//when running with .net framework, you'll need to add this line:
	// ServicePointManager.DefaultConnectionLimit = sites.Count();
	
	var builders = CreateRequestBuilders(sites);
	var requests = builders.Select(b => b.Build());

	return await ExecuteRequestsAndMerge(requests, destinationDirectory);
}

IEnumerable<UrlRequestBuilder> CreateRequestBuilders(IEnumerable<Uri> uris)
{
	foreach (var uri in uris)
	{
		yield return new UrlRequestBuilder()
			.SetUrl(uri)
			.SetRemoteUrlHeader("gotenberg-sharp-news-summary", $"{DateTime.Now.ToShortDateString()}")
			.ConfigureRequest(b =>
			{
				 b.PageRanges("1-3");
			}).AddHeaderFooter(b =>
			{
				 b.SetFooter(GetHeadFoot(uri.ToString()));
			})
			.WithDimensions(b =>
			{
				b.SetPaperSize(PaperSizes.A4)
				 .SetMargins(Margins.None)
				 .MarginBottom(1)
				 .LandScape();
			});
	}

	static string GetHeadFoot(string heading)
		=> "<html><head> <style> body { font-size: 8rem; } h1 { margin-left: auto; margin-right: auto; } </style></head><body><h1>" + heading + "</h1></body></html>";
}

async Task<string> ExecuteRequestsAndMerge(IEnumerable<UrlRequest> requests, string destinationDirectory)
{
	var sharpClient = new GotenbergSharpClient("http://localhost:3000");

	var tasks = requests.Select(r => sharpClient.UrlToPdfAsync(r, CancellationToken.None));
	var results = await Task.WhenAll(tasks);

	var mergeBuilder = new MergeBuilder()
		.WithAssets(b =>
		{
			b.AddItems(results.Select((r, i) => KeyValuePair.Create($"{i}.pdf", r)));
		})
		.ConfigureRequest(b => b.TimeOut(1799));

	var response = await sharpClient.MergePdfsAsync(mergeBuilder.Build());

	return await WriteFileAndGetPath(response, destinationDirectory);
}

async Task<string> WriteFileAndGetPath(Stream responseStream, string desinationDirectory)
{
	var date = DateTime.Now;
	var fullPath = @$"{desinationDirectory}\{date.ToString("yyyy-MM-MMMM-dd")}-{date.Ticks}-a4.pdf";
	using (var destinationStream = File.Create(fullPath))
	{
		await responseStream.CopyToAsync(destinationStream);
	}
	return fullPath;
}