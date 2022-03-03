<Query Kind="Program">
  <NuGetReference>Autofac</NuGetReference>
  <NuGetReference>Autofac.Extensions.DependencyInjection</NuGetReference>
  <NuGetReference Version="2.0.0-alpha0002" Prerelease="true">Gotenberg.Sharp.API.Client</NuGetReference>
  <NuGetReference>Microsoft.Extensions.Configuration</NuGetReference>
  <NuGetReference>Microsoft.Extensions.Configuration.Json</NuGetReference>
  <NuGetReference>Microsoft.Extensions.DependencyInjection</NuGetReference>
  <NuGetReference>Microsoft.Extensions.Logging</NuGetReference>
  <NuGetReference>Microsoft.Extensions.Logging.Console</NuGetReference>
  <NuGetReference>Microsoft.Extensions.Options</NuGetReference>
  <NuGetReference>Microsoft.Extensions.Options.ConfigurationExtensions</NuGetReference>
  <Namespace>Autofac</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Domain.Builders</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Domain.Builders.Faceted</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Domain.Requests</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Domain.Settings</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Extensions</Namespace>
  <Namespace>Microsoft.Extensions.Configuration</Namespace>
  <Namespace>Microsoft.Extensions.DependencyInjection</Namespace>
  <Namespace>Microsoft.Extensions.DependencyInjection.Extensions</Namespace>
  <Namespace>Microsoft.Extensions.Logging</Namespace>
  <Namespace>Microsoft.Extensions.Logging.Console</Namespace>
  <Namespace>Microsoft.Extensions.Options</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

//Builds a simple DI container with logging enabled.
//Client retreived through the SP is configured with options defined in local appsettings.json
//Watch the polly-retry policy in action: 
//	Turn off gotenberg, run this script and let it fail/retry two or three times.
//	Turn gotenberg back on & the request will successfully complete. 
//Example builds a 1 page PDF from the specified TargetUrl

const string TargetUrl = "https://www.cnn.com";
const string SaveToPath = @"D:\Gotenberg\Dumps";

static Random Rand = new Random(Math.Abs( (int) DateTime.Now.Ticks));

async Task Main()
{
	using (var scope =  new ContainerBuilder().Build().BeginLifetimeScope())
	{
	    var services = BuildServiceCollection();

		var sp = services.BuildServiceProvider();

		var sharpClient = sp.GetRequiredService<GotenbergSharpClient>();
 
 		var request = await CreateUrlRequest();

		var response = await sharpClient.UrlToPdfAsync(request);

		var resultPath = @$"{SaveToPath}\GotenbergFromUrl-{Rand.Next()}.pdf";

		using (var destinationStream = File.Create(resultPath))
		{
			await response.CopyToAsync(destinationStream);
		}

		var info = new ProcessStartInfo { FileName = resultPath, UseShellExecute = true };
		Process.Start(info);

		resultPath.Dump("Done");

		//var ops = sp.GetRequiredService<IOptions<GotenbergSharpClientOptions>>();
		//ops.Dump();
	}
}

#region configuration & service collection Setup

IServiceCollection BuildServiceCollection()
{
	var config = new ConfigurationBuilder()
		.SetBasePath(@$"{Path.GetDirectoryName(Util.CurrentQueryPath)}\Resources\Settings")
		.AddJsonFile("appsettings.json")
		.Build();

	return new ServiceCollection()
		.AddOptions<GotenbergSharpClientOptions>()
		.Bind(config.GetSection(nameof(GotenbergSharpClient))).Services
		.AddGotenbergSharpClient()
		.Services.AddLogging(s => s.AddSimpleConsole(ops => {
			ops.IncludeScopes = true;
			ops.SingleLine = false;
			ops.TimestampFormat = "hh:mm:ss ";
		}));
}

#endregion

#region gotenberg request creation

Task<UrlRequest> CreateUrlRequest()
{
	var builder = new UrlRequestBuilder()
		.SetUrl(TargetUrl)
		.SetConversionBehaviors(b =>
		{
			  b.SetUserAgent(nameof(GotenbergSharpClient));
		})
		.ConfigureRequest(b => b.SetPageRanges("1-2"))
		.WithDimensions(b =>
		{
			b.SetPaperSize(PaperSizes.A4)
			 .SetMargins(Margins.None);
		});

	return builder.BuildAsync();
}

#endregion