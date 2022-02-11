<Query Kind="Program">
  <Reference Relative="..\lib\bin\Debug\netstandard2.1\Gotenberg.Sharp.API.Client.dll">C:\dev\Open\GotenbergSharpApiClient\lib\bin\Debug\netstandard2.1\Gotenberg.Sharp.API.Client.dll</Reference>
  <Namespace>Gotenberg.Sharp.API.Client</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Domain.Builders</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Domain.Builders.Faceted</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Domain.Requests</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Net.Http</Namespace>
</Query>

//NOTE: You need to increase gotenberg api's timeout for this to work 
//by passing --api-timeout=1800s when running the container.
//FYI, latest goten, 7.5.0 errors when you run all them. e.g. 
// "unix process error: wait for unix process: exit status 5"
static Random Rand = new Random(Math.Abs( (int) DateTime.Now.Ticks));

async Task Main()
{
	var path = await CreateWorldNewsSummary($@"D:\NewsArchive");
	
	var info = new ProcessStartInfo{ FileName = path, UseShellExecute = true};
	Process.Start(info);
	
	path.Dump("Done");
}

public async Task<string> CreateWorldNewsSummary(string destinationDirectory)
{
	var sites = new[] {
		"https://www.nytimes.com","https://www.axios.com/",
		"https://www.cnn.com",  "https://www.csmonitor.com",
		"https://www.wsj.com", "https://www.usatoday.com",  
		"https://www.irishtimes.com", "https://www.lemonde.fr", 
		"https://calgaryherald.com", "https://www.bbc.com/news/uk",
		"https://english.elpais.com/", 	"https://www.thehindu.com", 
		"https://www.theaustralian.com.au",	"https://www.welt.de", 
		"https://www.cankaoxiaoxi.com", "https://www.novinky.cz",
		"https://www.elobservador.com.uy"}
		.Select(u => new Uri(u))
		.Take(6);
		
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
			.SetConversionBehaviors(b => 
			   // b.SetPdfFormat(PdfFormats.A1a)
				b.EmulateAsScreen()
			    .SetUserAgent(nameof(GotenbergSharpClient)))
			.ConfigureRequest(b =>
			{
				 b.SetPageRanges("1-2");
			})
			.WithDimensions(b =>
			{
				b.SetMargins(Margins.None)
				 .MarginLeft(.3)
				 .MarginRight(.3)
				.SetPaperSize(PaperSizes.A4);
			});
	}

}

async Task<string> ExecuteRequestsAndMerge(IEnumerable<UrlRequest> requests, string destinationDirectory)
{
	var innerClient = new HttpClient {
		 BaseAddress = new Uri("http://localhost:3000"),
		 Timeout = TimeSpan.FromMinutes(7)
	};
	
	var sharpClient = new GotenbergSharpClient(innerClient);

	var tasks = requests.Select(r => sharpClient.UrlToPdfAsync(r, CancellationToken.None));
	var results = await Task.WhenAll(tasks);

	var mergeBuilder = new MergeBuilder()
		.WithAssets(b =>
		{
			b.AddItems(results.Select((r, i) => KeyValuePair.Create($"{i}.pdf", r)));
		});

	var response = await sharpClient.MergePdfsAsync(mergeBuilder.Build());

	return await WriteFileAndGetPath(response, destinationDirectory);
}

async Task<string> WriteFileAndGetPath(Stream responseStream, string desinationDirectory)
{
	var fullPath = @$"{desinationDirectory}\{DateTime.Now.ToString("yyyy-MM-MMMM-dd")}-{Rand.Next()}.pdf";
	
	using (var destinationStream = File.Create(fullPath))
	{
		await responseStream.CopyToAsync(destinationStream);
	}
	return fullPath;
}