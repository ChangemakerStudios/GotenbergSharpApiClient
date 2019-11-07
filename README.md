# GotenbergSharpApiClient
C# Client for interacting with the Gotenberg API >= v6.0 https://thecodingmachine.github.io/gotenberg
Gotenberg is a Docker-powered stateless API for converting HTML, Markdown and Office documents to PDF.

## Recommended usage for .NetCore app: make an IServiceCollection extension.
```csharp

public static IHttpClientBuilder AddTypedApiClient<TClient>(this IServiceCollection services, InnerClientSettings settings) where TClient: class 
 {
     if(settings == null) throw new ArgumentNullException(nameof(settings));
     if(settings.GetApiBaseUriFunc == null) throw new ArgumentException(nameof(settings.GetApiBaseUriFunc));

     return services.AddHttpClient(settings.ClientName) 
                    .ConfigureHttpClient((sp, client) =>
                                         {
                                             client.Timeout = settings.Timeout;
                                             client.BaseAddress = settings.GetApiBaseUriFunc(sp);
                                         })
                    .AddTypedClient<TClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => new TimeoutHandler(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
                    .SetHandlerLifetime(settings.HandlerLifeTime);
                    
                    //Also recommended: add a Polly Retry policy using https://www.nuget.org/packages/Polly
                    //See: 
                    // https://github.com/App-vNext/Polly/wiki/Polly-and-HttpClientFactory
                    // https://www.stevejgordon.co.uk/httpclientfactory-using-polly-for-transient-fault-handling
}

 ## Add the client to the ServiceCollection in startup.cs
public IServiceProvider ConfigureServices(IServiceCollection services) 
{
	//add core services then
	services.AddTypedApiClient<GotenbergApiSharpClient>(CreateGotenbergClientSettings());

	//more setup then build the container and add it to your DI Container
	var builder = new ContainerBuilder();
        builder.Populate(services);
			
	return new AutofacServiceProvider(builder.Build());
}

 ## inject it where you need it and use
public class GenerateSomePdfService
{
	readonly GotenbergApiSharpClient _gotenbergClient;
	
	public GenerateSomePdfService(GotenbergApiSharpClient gotenbergClient)
	{
		_gotenbergClient = gotenbergClient;
	}

	public async Task<Stream> MakePdf(string htmlContent, string footer, CancellationToken cancelToken = default)
	{
		var dims = new DocumentDimensions { PaperWidth = 8.26, PaperHeight = 11.69, Landscape = false, MarginBottom = .38 };

		var content = new DocumentContent(htmlContent, footer);
		var request = new PdfRequest(content, dims);

		return await this._gotenbergClient.HtmlToPdfAsync(request, cancelToken).ConfigureAwait(false);
	}

	public async Task<Stream> MergePdfs(IEnumerable<byte[]> toMerge, CancellationToken cancelToken = default)
	{
		var request = new MergeRequest();
		foreach (var item in toMerge.SelectWithIndex())
		{
			request.Items.Add(KeyValuePair.Create($"{item.Index.ToAlphabeticallySortableName()}.pdf", item.Value));
		}

		return await this._gotenbergClient.MergePdfsAsync(request, cancelToken).ConfigureAwait(false);
	}

}
