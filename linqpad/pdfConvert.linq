<Query Kind="Program">
  <Reference Relative="..\lib\bin\Debug\netstandard2.1\Gotenberg.Sharp.API.Client.dll">C:\dev\Open\GotenbergSharpApiClient\lib\bin\Debug\netstandard2.1\Gotenberg.Sharp.API.Client.dll</Reference>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Domain.Requests.Facets</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Domain.Requests</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Domain.Builders.Faceted</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Extensions</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Domain.Builders</Namespace>
</Query>

async Task Main()
{
	var p = await DoConversion(@"D:\Gotenberg\Dumps");
	
	var info = new ProcessStartInfo { FileName = p, UseShellExecute = true };
	Process.Start(info);
	
	p.Dump("Done");
}

async Task<string> DoConversion(string destinationPath)
{
	var sharpClient = new GotenbergSharpClient("http://localhost:3000");

	var items = Directory.GetFiles(destinationPath, "*.pdf", SearchOption.TopDirectoryOnly)
		.Select(p => new { Info = new FileInfo(p), Path = p })
		.OrderBy(item => item.Info.CreationTime)
		.Take(2);
		
	var toConvert = items.Select(item => KeyValuePair.Create(item.Info.Name, File.ReadAllBytes(item.Path)));
 
 	var builder = new PdfConversionBuilder()
		.AddItems(b => b.AddItems(toConvert) )
		.SetFormat(PdfFormats.A1a); //A1a is the only one Gotenberg can handle?
		
	var request = builder.Build();
 
	var response = await sharpClient.ConvertPdfDocumentsAsync(request);

	//If you send one in -- the result is pdf.
	var outPath = @$"{destinationPath}\GotenbergConvertResult.zip";

	using (var destinationStream = File.Create(outPath))
	{
		await response.CopyToAsync(destinationStream, default(CancellationToken));
	}

	return outPath;

}