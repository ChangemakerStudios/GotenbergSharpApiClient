<Query Kind="Program">
  <NuGetReference Version="1.2.0">Gotenberg.Sharp.API.Client</NuGetReference>
  <Namespace>Gotenberg.Sharp.API.Client</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Domain.Builders</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Domain.Builders.Faceted</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

async Task Main()
{
	//For this to work you need an api running on localhost:5000 w/ an endpoint to receive the webhook
	
	var destinationPath = @"D:\Gotenberg\Dumps\FromWebhook";
	var footerPath = @"D:\Gotenberg\Resources\Html\UrlHeader.html";
	var headerPath =@"D:\Gotenberg\Resources\Html\UrlFooter.html";
	
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
					.SetTimeout(20)
					.AddRequestHeader("custom-header", "value");
			}).ChromeRpccBufferSize(1024);
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