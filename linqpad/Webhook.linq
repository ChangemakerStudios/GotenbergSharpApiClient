<Query Kind="Program">
  <Reference Relative="..\lib\bin\Debug\net5.0\Gotenberg.Sharp.API.Client.dll">C:\Projects\GotenbergSharpApiClient\lib\bin\Debug\net5.0\Gotenberg.Sharp.API.Client.dll</Reference>
  <Namespace>Gotenberg.Sharp.API.Client</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Domain.Builders</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Domain.Builders.Faceted</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Extensions</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>


async Task Main()
{
	//For this to work you need an api running on localhost:5000 w/ an endpoint to receive the webhook
	
	var resourcePath = @$"{Path.GetDirectoryName(Util.CurrentQueryPath)}\Resources\Html";
	
	var destinationPath = @"C:\Temp\Gotenberg\Dumps\FromWebhook";
	var footerPath = @$"{resourcePath}\UrlHeader.html";
	var headerPath =@$"{resourcePath}\UrlFooter.html";
	
	headerPath.Dump();
	
	await CreateFromUrl(destinationPath,headerPath, footerPath);
	
	"Request sent...".Dump();
}
public async Task CreateFromUrl(string destinationPath, string headerPath, string footerPath)
{
	var sharpClient = new GotenbergSharpClient("http://localhost:3000");

	var builder = new UrlRequestBuilder()
		.SetUrl("https://www.newyorker.com")
		.ConfigureRequest(b =>
		{
			b.AddWebhook(hook =>
			{
				hook.SetUrl("http://host.docker.internal:5000/api/WebhookReceiver")
					.SetErrorUrl("http://host.docker.internal:5000/api/WebhookReceiver")
					.AddExtraHeader("custom-header", "value");
			}).SetPageRanges("1-2");
		})
		.AddAsyncHeaderFooter(async
			b => b.SetHeader(await File.ReadAllTextAsync(headerPath))
				  .SetFooter(await File.ReadAllBytesAsync(footerPath)
		)).WithDimensions(b =>
		{
			b.SetPaperSize(PaperSizes.A4)
			.SetMargins(Margins.None);
			 
		});

	var request = await builder.BuildAsync();
	
	await sharpClient.FireWebhookAndForgetAsync(request);
}