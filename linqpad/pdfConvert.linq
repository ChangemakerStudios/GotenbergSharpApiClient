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


static Random Rand = new Random(Math.Abs((int)DateTime.Now.Ticks));
const int NumberOfFilesToGet = 1; 
//If you get 1, the result is a pdf; get more 
//and the API retuns a zip containing the results
//Currently, Gotenberg supports these formats: A1a, A2b & A3b

async Task Main()
{
	var p = await DoConversion(@"D:\Gotenberg\Delivs");
	
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
		.Take(1);
		
	var toConvert = items.Select(item => KeyValuePair.Create(item.Info.Name, File.ReadAllBytes(item.Path)));
 
 	var builder = new PdfConversionBuilder()
		.WithPdfs(b => b.AddItems(toConvert) )
		.SetFormat(PdfFormats.A1a); 
		
	var request = builder.Build();

	var response = await sharpClient.ConvertPdfDocumentsAsync(request);

	//If you send one in -- the result is pdf.
	var extension = items.Count() >1 ? "zip" : "pdf";
	var outPath = @$"{destinationPath}\GotenbergConvertResult.{extension}";

	using (var destinationStream = File.Create(outPath))
	{
		await response.CopyToAsync(destinationStream, default(CancellationToken));
	}

	return outPath;

}