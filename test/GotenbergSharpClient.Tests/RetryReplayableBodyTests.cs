using Gotenberg.Sharp.API.Client.Application.Builders;
using Gotenberg.Sharp.API.Client.Domain.Settings;
using Gotenberg.Sharp.API.Client.Extensions;

using Microsoft.Extensions.DependencyInjection;

namespace GotenbergSharpClient.Tests;

[TestFixture]
public class RetryReplayableBodyTests
{
    private const int RetryCount = 2;

    [Test]
    public async Task NonSeekableStreamAsset_IsNotRetried_AndSurfacesTheOriginalFailure()
    {
        var handler = new ConnectionFailureHandler();
        var client = CreateClient(handler);

        var act = () => client.HtmlToPdfAsync(CreateRequest(new OneWayStream(new byte[] { 1, 2, 3, 4 })));

        // Without the fix the second attempt fails with "The stream was already read from",
        // an InvalidOperationException that hides the real connection failure.
        await act.Should().ThrowAsync<HttpRequestException>();
        handler.BodyLengths.Should().HaveCount(1);
    }

    [Test]
    public async Task SeekableStreamAsset_IsRetried_WithTheFullBodyEachTime()
    {
        var handler = new ConnectionFailureHandler();
        var client = CreateClient(handler);

        var act = () => client.HtmlToPdfAsync(CreateRequest(new MemoryStream(new byte[] { 1, 2, 3, 4 })));

        await act.Should().ThrowAsync<HttpRequestException>();
        handler.BodyLengths.Should().HaveCount(1 + RetryCount);
        handler.BodyLengths.Should().OnlyContain(length => length == handler.BodyLengths[0]);
    }

    [Test]
    public async Task ByteArrayAsset_IsRetried()
    {
        var handler = new ConnectionFailureHandler();
        var client = CreateClient(handler);

        var act = () => client.HtmlToPdfAsync(CreateRequest(new byte[] { 1, 2, 3, 4 }));

        await act.Should().ThrowAsync<HttpRequestException>();
        handler.BodyLengths.Should().HaveCount(1 + RetryCount);
    }

    private static HtmlRequestBuilder CreateRequest(Stream asset) => new HtmlRequestBuilder()
        .AddDocument(doc => doc.SetBody("<html><body>hi</body></html>"))
        .WithAssets(a => a.AddItem("logo.png", asset));

    private static HtmlRequestBuilder CreateRequest(byte[] asset) => new HtmlRequestBuilder()
        .AddDocument(doc => doc.SetBody("<html><body>hi</body></html>"))
        .WithAssets(a => a.AddItem("logo.png", asset));

    private static Gotenberg.Sharp.API.Client.GotenbergSharpClient CreateClient(HttpMessageHandler handler)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions<GotenbergSharpClientOptions>()
            .Configure(o =>
            {
                o.ServiceUrl = new Uri("http://gotenberg.invalid");
                o.RetryPolicy = new RetryOptions { Enabled = true, RetryCount = RetryCount, BackoffPower = 0 };
            });
        services.AddGotenbergSharpClient()
            .ConfigurePrimaryHttpMessageHandler(() => handler);

        return services.BuildServiceProvider()
            .GetRequiredService<Gotenberg.Sharp.API.Client.GotenbergSharpClient>();
    }

    /// <summary>Reads the request body, as a real transport does, then fails to get an answer.</summary>
    private sealed class ConnectionFailureHandler : HttpMessageHandler
    {
        public List<long> BodyLengths { get; } = new();

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var body = new MemoryStream();
            await request.Content!.CopyToAsync(body, cancellationToken);
            BodyLengths.Add(body.Length);

            throw new HttpRequestException("connection reset");
        }
    }

    /// <summary>A forward-only stream, like a network or compression stream.</summary>
    private sealed class OneWayStream(byte[] bytes) : Stream
    {
        private readonly MemoryStream _inner = new(bytes);

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
