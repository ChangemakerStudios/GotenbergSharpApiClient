<Query Kind="Program">
  <NuGetReference Version="2.0.0-alpha0002" Prerelease="true">Gotenberg.Sharp.API.Client</NuGetReference>
  <Namespace>Gotenberg.Sharp.API.Client</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Domain.Builders</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Domain.Builders.Faceted</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Domain.Requests</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Domain.Requests.Facets</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Extensions</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

async Task Main()
{
	var p = await DoMerge(@"D:\Gotenberg\Dumps");
	
	var info = new ProcessStartInfo { FileName = p, UseShellExecute = true };
	Process.Start(info);
	
	p.Dump("Done");
}

async Task<string> DoMerge(string destinationPath)
{
	var sharpClient = new GotenbergSharpClient("http://localhost:3000");

	var items = Directory.GetFiles(destinationPath, "*.pdf", SearchOption.TopDirectoryOnly)
		.Select(p => new { Info = new FileInfo(p), Path = p })
		.Where(item => !item.Info.Name.Contains("GotenbergMergeResult.pdf"))
		.OrderBy(item => item.Info.CreationTime)
		.Take(2);

	 items.Dump("Items", 0);

	var toMerge = items.Select(item => KeyValuePair.Create(item.Info.Name, File.ReadAllBytes(item.Path)));
 
 	var builder = new MergeBuilder()
		 .SetPdfFormat(PdfFormats.A2b)
		.WithAssets(b => { b.AddItems(toMerge) ;});

	var request = builder.Build();
	request.ToHttpContent()
		   .ToDumpFriendlyFormat()
		   .Dump();
	var response = await sharpClient.MergePdfsAsync(request);

	var outPath = @$"{destinationPath}\GotenbergMergeResult.pdf";

	using (var destinationStream = File.Create(outPath))
	{
		await response.CopyToAsync(destinationStream, default(CancellationToken));
	}

	return outPath;

}