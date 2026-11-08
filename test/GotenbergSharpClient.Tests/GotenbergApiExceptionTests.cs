using System.Net;
using System.Text.Json.Nodes;

using Gotenberg.Sharp.API.Client.Domain.Requests.ApiRequests;
using Gotenberg.Sharp.API.Client.Domain.Settings;
using Gotenberg.Sharp.API.Client.Extensions;
using Gotenberg.Sharp.API.Client.Infrastructure;

using Microsoft.Extensions.DependencyInjection;

namespace GotenbergSharpClient.Tests;

[TestFixture]
public class GotenbergApiExceptionTests
{
    private const string TraceHeader = "Gotenberg-Trace";

    [Test]
    public async Task CreateAsync_CapturesStatusMessageAndTrace_AndDisposesTheResponse()
    {
        var content = new TrackingContent("Invalid form data");
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = content };
        response.Headers.Add(TraceHeader, "trace-1234");

        var ex = await GotenbergApiException.CreateAsync(new GetApiRequestImpl("forms/chromium/convert/html"), response);

        ex.Message.Should().Be("Invalid form data");
        ex.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        ex.Trace.Should().Be("trace-1234");
        content.Disposed.Should().BeTrue("the exception copies what it reports and does not keep the response");
    }

    [Test]
    public async Task CreateAsync_WithoutTraceHeader_HasNullTrace()
    {
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = new StringContent("boom") };

        var ex = await GotenbergApiException.CreateAsync(new GetApiRequestImpl("version"), response);

        ex.Trace.Should().BeNull();
    }

    [Test]
    public async Task CreateAsync_HonoursCancellation_AndStillDisposesTheResponse()
    {
        var content = new TrackingContent(neverCompletes: true);
        var response = new HttpResponseMessage(HttpStatusCode.BadGateway) { Content = content };
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

        var act = () => GotenbergApiException.CreateAsync(new GetApiRequestImpl("version"), response, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        content.Disposed.Should().BeTrue();
    }

    [Test]
    public async Task ToVerboseJson_UsesTheSnapshot_AfterTheResponseWasDisposed()
    {
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("Invalid form data"),
            ReasonPhrase = "Bad Request",
            RequestMessage = new HttpRequestMessage(HttpMethod.Post, "http://gotenberg.invalid/forms/chromium/convert/html")
        };
        response.Headers.Add(TraceHeader, "trace-1234");

        var ex = await GotenbergApiException.CreateAsync(new GetApiRequestImpl("version"), response);

        var json = JsonNode.Parse(ex.ToVerboseJson())!;
        var received = json["GotenbergResponseReceived"]!;

        json["GotenbergMessage"]!.GetValue<string>().Should().Be("Invalid form data");
        received["StatusCode"]!.GetValue<int>().Should().Be(400);
        received["ReasonPhrase"]!.GetValue<string>().Should().Be("Bad Request");
        received["RequestUri"]!.GetValue<string>().Should().Be("http://gotenberg.invalid/forms/chromium/convert/html");
        received["RequestMethod"]!.GetValue<string>().Should().Be("POST");
        received["Headers"]![TraceHeader]!.GetValue<string>().Should().Be("trace-1234");
        received["Trace"].Should().BeNull("Trace is exposed on the exception, not duplicated in the dump");

        ex.ToVerboseJson(includeGotenbergResponse: false).Should().NotContain("GotenbergResponseReceived");
        ex.ToVerboseJson().Should().Be(ex.ToVerboseJson(), "the exception can be dumped more than once");
    }

    [Test]
    public async Task Client_ErrorResponse_ExposesTraceAndReleasesTheResponse()
    {
        var content = new TrackingContent("Invalid form data");
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions<GotenbergSharpClientOptions>()
            .Configure(o => o.ServiceUrl = new Uri("http://gotenberg.invalid"));
        services.AddGotenbergSharpClient()
            .ConfigurePrimaryHttpMessageHandler(() => new StubHandler(() =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = content };
                response.Headers.Add(TraceHeader, "trace-5678");
                return response;
            }));

        var client = services.BuildServiceProvider()
            .GetRequiredService<Gotenberg.Sharp.API.Client.GotenbergSharpClient>();

        var act = () => client.GetVersion();

        var ex = (await act.Should().ThrowAsync<GotenbergApiException>()).Which;
        ex.Trace.Should().Be("trace-5678");
        ex.Message.Should().Be("Invalid form data");
        content.Disposed.Should().BeTrue();
    }

    private sealed class StubHandler(Func<HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) => Task.FromResult(respond());
    }

    /// <summary>Records disposal and can stall, to model a body that is still in transit.</summary>
    private sealed class TrackingContent : HttpContent
    {
        private readonly string _text;
        private readonly bool _neverCompletes;

        public TrackingContent(string text = "", bool neverCompletes = false)
        {
            _text = text;
            _neverCompletes = neverCompletes;
        }

        public bool Disposed { get; private set; }

        protected override async Task SerializeToStreamAsync(
            Stream stream,
            TransportContext? context,
            CancellationToken cancellationToken)
        {
            if (_neverCompletes) await Task.Delay(Timeout.Infinite, cancellationToken);

            var bytes = System.Text.Encoding.UTF8.GetBytes(_text);
            await stream.WriteAsync(bytes, cancellationToken);
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) =>
            SerializeToStreamAsync(stream, context, CancellationToken.None);

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }

        protected override void Dispose(bool disposing)
        {
            Disposed = true;
            base.Dispose(disposing);
        }
    }
}
