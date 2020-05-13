<Query Kind="Program">
  <Reference Relative="..\lib\bin\Debug\netstandard2.1\Gotenberg.Sharp.API.Client.dll">lib\bin\Debug\netstandard2.1\Gotenberg.Sharp.API.Client.dll</Reference>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Domain.Builders</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Extensions</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Domain.Requests</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Domain.Requests.Facets</Namespace>
</Query>

async Task Main()
{
	var p = await DoMerge(@"D:\Gotenberg\Dumps");
	p.Dump();
}

async Task<string> DoMerge(string destinationPath)
{
	var sharpClient = new GotenbergSharpClient("http://localhost:3000");

	var items = Directory.GetFiles(destinationPath, "*.pdf", SearchOption.TopDirectoryOnly)
		.Select(p => new { Info = new FileInfo(p), Path = p })
		.Where(item => !item.Info.Name.Contains("GotenbergMergeResult.pdf"))
		.OrderBy(item => item.Info.CreationTime)
		.Take(4);

	 items.Dump("Items", 0);

	var toMerge = items.Select(item => KeyValuePair.Create(item.Info.Name, File.ReadAllBytes(item.Path)));
 
 	var builder = new MergeBuilder()
		.WithAssets(b => {
			b.AddItems(toMerge);
		}).ConfigureRequest(b =>
		{
			b.ChromeRpccBufferSize(RequestConfig.DefaultChromeRpccBufferSize * 3)
			 .ResultFileName("WTF.pdf")
			 .TimeOut(55);
		});
		
	var request = builder.Build();	
 
	 var response = await sharpClient.MergePdfsAsync(request);

	var outPath = @$"{destinationPath}\GotenbergMergeResult.pdf";

	using (var destinationStream = File.Create(outPath))
	{
		await response.CopyToAsync(destinationStream, default(CancellationToken));
	}

	return outPath;

}