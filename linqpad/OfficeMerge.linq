<Query Kind="Program">
  <Reference Relative="..\lib\bin\Debug\netstandard2.1\Gotenberg.Sharp.API.Client.dll">lib\bin\Debug\netstandard2.1\Gotenberg.Sharp.API.Client.dll</Reference>
  <Namespace>Gotenberg.Sharp.API.Client</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Domain.Builders</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Domain.Requests</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Infrastructure</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

async Task Main()
{
	var source = @"D:\Gotenberg\Resources\OfficeDocs";
	var destination = @"D:\Gotenberg\Dumps";
	var p = await DoOfficeMerge(source, destination);
	p.Dump("The path");
}

public async Task<string> DoOfficeMerge(string sourceDirectory, string destinationDirectory)
{
	var client = new GotenbergSharpClient("http://localhost:3000");

	var builder = new MergeOfficeBuilder()
			.WithAsyncAssets(async b => b.AddItems(await GetDocsAsync(sourceDirectory)))
			.ConfigureRequest(b =>
			{
				b.TimeOut(100);
			});

	var request = await builder.BuildAsync();
	var response = await client.MergeOfficeDocsAsync(request).ConfigureAwait(false);

	var mergeResultPath = @$"{destinationDirectory}\GotenbergOfficeMerge.pdf";
	using (var destinationStream = File.Create(mergeResultPath))
	{
		await response.CopyToAsync(destinationStream).ConfigureAwait(false);
	}

	return mergeResultPath;
}

async Task<IEnumerable<KeyValuePair<string, byte[]>>> GetDocsAsync(string sourceDirectory)
{
	var paths = Directory.GetFiles(sourceDirectory, "*.*", SearchOption.TopDirectoryOnly);
	var names = paths.Select(p => new FileInfo(p).Name);
	var tasks = paths.Select(async f => await File.ReadAllBytesAsync(f));
	var docs = await Task.WhenAll(tasks);

	return names.Select((name, index) => new KeyValuePair<string,byte[]>(name, docs[index]));
}