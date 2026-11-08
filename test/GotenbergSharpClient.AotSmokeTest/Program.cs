using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

using Gotenberg.Sharp.API.Client;
using Gotenberg.Sharp.API.Client.Application.Builders;
using Gotenberg.Sharp.API.Client.Domain.Bookmarks;
using Cookie = Gotenberg.Sharp.API.Client.Domain.Cookies.Cookie;
using Gotenberg.Sharp.API.Client.Domain.Dimensions;
using Gotenberg.Sharp.API.Client.Domain.Settings;
using Gotenberg.Sharp.API.Client.Extensions;
using Gotenberg.Sharp.API.Client.Infrastructure;

using Microsoft.Extensions.DependencyInjection;

// Exercises the reflection- and JSON-driven paths of the client from a Native AOT binary.
// Runs against an in-process stub by default; set GOTENBERG_URL (and optionally
// GOTENBERG_USERNAME / GOTENBERG_PASSWORD) to also convert against a real Gotenberg.

var failures = 0;

await Check("Form fields are built from [MultiFormHeader] properties", async () =>
{
    var (client, stub) = CreateStubbedClient();

    var builder = new HtmlRequestBuilder()
        .AddDocument(doc => doc.SetBody("<html><body><p>AOT</p></body></html>"))
        .WithPageProperties(p => p.SetPaperWidth(Dimension.FromMillimeters(210)).SetScale(0.9))
        .SetConversionBehaviors(b => b
            .AddAdditionalHeaders("hello", "from-earth")
            .AddCookie(new Cookie { Name = "sid", Value = "abc", Domain = "example.com" })
            .AddEmulatedMediaFeature("prefers-color-scheme", "dark")
            .SetFailOnHttpStatusCodes(499))
        .SetPdfOutputOptions(o => o
            .SetMetadata(new Dictionary<string, object> { ["Author"] = "AotSmokeTest" }));

    await using var _ = await client.HtmlToPdfAsync(builder);

    var fields = stub.LastFormFields;
    Expect(fields.ContainsKey("index.html"), "index.html part is missing");
    ExpectField(fields, "paperWidth", "210mm");
    ExpectField(fields, "scale", "0.9");
    ExpectJsonField(fields, "extraHttpHeaders", "{\"hello\":\"from-earth\"}");
    ExpectField(fields, "cookies", "[{\"name\":\"sid\",\"value\":\"abc\",\"domain\":\"example.com\"}]");
    ExpectField(fields, "emulatedMediaFeatures", "{\"prefers-color-scheme\":\"dark\"}");
    ExpectField(fields, "failOnHttpStatusCodes", "[499]");
    ExpectJsonField(fields, "metadata", "{\"Author\":\"AotSmokeTest\"}");
});

await Check("Bookmarks are serialized into the request", async () =>
{
    var (client, stub) = CreateStubbedClient();

    var builder = PdfEngineBuilders
        .WriteBookmarks(new[] { new Bookmark("Chapter 1", 1, new Bookmark("Section 1.1", 2)) })
        .WithPdfs(pdfs => pdfs.AddItem("doc.pdf", StubHandler.FakePdf));

    await using var _ = await client.ExecutePdfEngineAsync(builder);

    ExpectField(
        stub.LastFormFields,
        "bookmarks",
        "[{\"title\":\"Chapter 1\",\"page\":1,\"children\":[{\"title\":\"Section 1.1\",\"page\":2,\"children\":[]}]}]");
});

await Check("Bookmarks are deserialized from the response", async () =>
{
    var (client, _) = CreateStubbedClient();

    var builder = PdfEngineBuilders.ReadBookmarks()
        .WithPdfs(pdfs => pdfs.AddItem("doc.pdf", StubHandler.FakePdf));

    var bookmarks = await client.ReadPdfBookmarksAsync(builder);

    Expect(bookmarks.ContainsKey("doc.pdf"), "doc.pdf key is missing");
    var root = bookmarks["doc.pdf"].Single();
    Expect(root.Title == "Chapter 1" && root.Page == 1, $"unexpected root '{root.Title}' p{root.Page}");
    Expect(root.Children.Single().Title == "Section 1.1", "child bookmark was not deserialized");
    Expect(root.Children.Single().Children.Count == 0, "null children should become an empty list");
});

await Check("Loosely typed JSON values are written without reflection", async () =>
{
    var (client, stub) = CreateStubbedClient();

    var metadata = new Dictionary<string, object>
    {
        ["Author"] = "AotSmokeTest",
        ["Pages"] = 3,
        ["Keywords"] = new[] { "aot", "trim" },
        ["Nested"] = new JsonObject { ["flag"] = true }
    };

    var builder = PdfEngineBuilders.WriteMetadata(metadata)
        .WithPdfs(pdfs => pdfs.AddItem("doc.pdf", StubHandler.FakePdf));

    await using var _ = await client.ExecutePdfEngineAsync(builder);

    ExpectField(
        stub.LastFormFields,
        "metadata",
        "{\"Author\":\"AotSmokeTest\",\"Pages\":3,\"Keywords\":[\"aot\",\"trim\"],\"Nested\":{\"flag\":true}}");
});

await Check("API errors can be dumped as verbose JSON", async () =>
{
    var (client, stub) = CreateStubbedClient();
    stub.FailWith = HttpStatusCode.BadRequest;

    var builder = new HtmlRequestBuilder()
        .AddDocument(doc => doc.SetBody("<html><body><p>AOT</p></body></html>"));

    try
    {
        await using var _ = await client.HtmlToPdfAsync(builder);
        Expect(false, "expected a GotenbergApiException");
    }
    catch (GotenbergApiException e)
    {
        using var dump = JsonDocument.Parse(e.ToVerboseJson());
        var root = dump.RootElement;
        Expect(root.GetProperty("GotenbergMessage").GetString() == "stub failure", "message is missing");
        Expect(
            root.GetProperty("GotenbergResponseReceived").GetProperty("StatusCode").GetInt32() == 400,
            "status code is missing");
        Expect(root.GetProperty("ClientRequestFormContent").GetArrayLength() > 0, "form content is missing");
    }
});

await Check("Dimension units round-trip through their descriptions", () =>
{
    foreach (var text in new[] { "210mm", "21cm", "8.5in", "72pt", "96px", "6pc" })
    {
        var parsed = Dimension.Parse(text);
        var expected = text.EndsWith("in", StringComparison.Ordinal) ? text[..^2] : text;
        Expect(parsed.ToString() == expected, $"'{text}' round-tripped to '{parsed}'");
    }

    return Task.CompletedTask;
});

var gotenbergUrl = Environment.GetEnvironmentVariable("GOTENBERG_URL");

if (string.IsNullOrWhiteSpace(gotenbergUrl))
{
    Console.WriteLine("SKIP  Live conversion (GOTENBERG_URL is not set)");
}
else
{
    await Check($"Live conversion against {gotenbergUrl}", async () =>
    {
        var services = new ServiceCollection();
        services.AddOptions<GotenbergSharpClientOptions>()
            .Configure(options =>
            {
                options.ServiceUrl = new Uri(gotenbergUrl);
                options.BasicAuthUsername = Environment.GetEnvironmentVariable("GOTENBERG_USERNAME");
                options.BasicAuthPassword = Environment.GetEnvironmentVariable("GOTENBERG_PASSWORD");
            });
        services.AddGotenbergSharpClient();

        await using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<GotenbergSharpClient>();

        var builder = new HtmlRequestBuilder()
            .AddDocument(doc => doc.SetBody("<html><body><h1>Native AOT</h1></body></html>"))
            .WithPageProperties(p => p.UseChromeDefaults())
            .SetConversionBehaviors(b => b.AddAdditionalHeaders("hello", "from-earth"));

        await using var pdf = await client.HtmlToPdfAsync(builder);

        var header = new byte[4];
        var read = await pdf.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false);
        Expect(read == 4 && Encoding.ASCII.GetString(header) == "%PDF", "response is not a PDF");
    });
}

Console.WriteLine(failures == 0 ? "All checks passed." : $"{failures} check(s) failed.");
return failures == 0 ? 0 : 1;

async Task Check(string name, Func<Task> check)
{
    try
    {
        await check();
        Console.WriteLine($"PASS  {name}");
    }
    catch (Exception e)
    {
        failures++;
        Console.WriteLine($"FAIL  {name}");
        Console.WriteLine($"      {e.GetType().Name}: {e.Message}");
    }
}

static void Expect(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

static void ExpectField(IReadOnlyDictionary<string, string> fields, string name, string expected)
{
    Expect(fields.TryGetValue(name, out var actual), $"form field '{name}' is missing");
    Expect(actual == expected, $"form field '{name}' was '{actual}', expected '{expected}'");
}

// Some fields are written indented; compare them with the whitespace removed.
static void ExpectJsonField(IReadOnlyDictionary<string, string> fields, string name, string expected)
{
    Expect(fields.TryGetValue(name, out var actual), $"form field '{name}' is missing");
    var compact = string.Concat(actual!.Where(c => !char.IsWhiteSpace(c)));
    Expect(compact == expected, $"form field '{name}' was '{compact}', expected '{expected}'");
}

// Resolved through DI so the typed-client registration and the Polly pipeline are exercised too.
static (GotenbergSharpClient Client, StubHandler Stub) CreateStubbedClient()
{
    var stub = new StubHandler();

    var services = new ServiceCollection();
    services.AddOptions<GotenbergSharpClientOptions>()
        .Configure(options => options.ServiceUrl = new Uri("http://gotenberg.invalid"));
    services.AddGotenbergSharpClient().ConfigurePrimaryHttpMessageHandler(() => stub);

    var client = services.BuildServiceProvider().GetRequiredService<GotenbergSharpClient>();

    return (client, stub);
}

/// <summary>
/// Stands in for Gotenberg: records the multipart form fields it is sent and answers each route
/// with a canned response.
/// </summary>
internal sealed class StubHandler : HttpMessageHandler
{
    internal static readonly byte[] FakePdf = Encoding.ASCII.GetBytes("%PDF-1.7\n%%EOF");

    internal Dictionary<string, string> LastFormFields { get; private set; } = new();

    internal HttpStatusCode? FailWith { get; set; }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var path = request.RequestUri!.AbsolutePath;

        if (path.EndsWith("/version", StringComparison.Ordinal))
        {
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("8.30.0") };
        }

        if (request.Content is MultipartFormDataContent form)
        {
            var fields = new Dictionary<string, string>();

            foreach (var part in form)
            {
                var disposition = part.Headers.ContentDisposition;
                var name = (disposition?.FileName ?? disposition?.Name ?? string.Empty).Trim('"');
                fields[name] = await part.ReadAsStringAsync(cancellationToken);
            }

            this.LastFormFields = fields;
        }

        if (this.FailWith is { } status)
        {
            return new HttpResponseMessage(status) { Content = new StringContent("stub failure") };
        }

        if (path.EndsWith("/bookmarks/read", StringComparison.Ordinal))
        {
            const string json =
                "{\"doc.pdf\":[{\"title\":\"Chapter 1\",\"page\":1,\"children\":[{\"title\":\"Section 1.1\",\"page\":2,\"children\":null}]}]}";

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        }

        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(FakePdf) };
    }
}
