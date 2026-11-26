using System.Net;
using System.Net.Http.Headers;

using Gotenberg.Sharp.API.Client.Application.Builders;
using Gotenberg.Sharp.API.Client.Domain.Shared;

namespace GotenbergSharpClient.Tests;

/// <summary>
/// <c>TimeOut</c>, the <see cref="HttpClient.Timeout" />, limits a call from its start to the end of the response:
/// the version lookup, the request and the reading of the body. A stub transport answers with headers and then a body
/// whose reads stall.
/// </summary>
[TestFixture]
public class TimeOutTests
{
    private static readonly TimeSpan Short = TimeSpan.FromMilliseconds(300);

    /// <summary>How long a test waits for a call that should end, before it fails instead of hanging.</summary>
    private static readonly TimeSpan Guard = TimeSpan.FromSeconds(10);

    [TestCase(false, TestName = "TimeOut ends a document whose body stalls after the headers, with a timeout")]
    [TestCase(true, TestName = "TimeOut ends a document whose headers never arrive, with a timeout")]
    public async Task TimeOut_EndsADocumentCall_WithATimeout(bool beforeHeaders)
    {
        var transport = new Transport(async cancellationToken =>
        {
            if (beforeHeaders) await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);

            return Pdf(new SlowBody(chunks: 2, pause: Timeout.InfiniteTimeSpan));
        });
        var client = CreateClient(transport, Short);

        var failure = await FailureOf(Convert(client));

        failure.Should().BeOfType<TaskCanceledException>()
            .Which.InnerException.Should().BeOfType<TimeoutException>();
    }

    [Test]
    public async Task CallersToken_EndsADocumentWhoseBodyStalls_WithACancellationThatIsNotATimeout()
    {
        var client = CreateClient(Stalling(), TimeSpan.FromMinutes(3));
        using var caller = new CancellationTokenSource(Short);

        var failure = await FailureOf(Convert(client, caller.Token));

        failure.Should().BeAssignableTo<OperationCanceledException>()
            .Which.CancellationToken.Should().Be(caller.Token);
        (failure.InnerException is TimeoutException).Should().BeFalse("the caller cancelled the call, it did not time out");
    }

    [Test]
    public async Task ADocumentThatKeepsArriving_AndEndsWithinTimeOut_IsRead()
    {
        var transport = new Transport(_ => Task.FromResult(Pdf(new SlowBody(chunks: 10, pause: TimeSpan.FromMilliseconds(50)))));
        var client = CreateClient(transport, TimeSpan.FromSeconds(5));

        using var pdf = await Convert(client);

        pdf.Length.Should().Be(10);
    }

    [Test]
    public async Task InfiniteTimeOut_LeavesTheDownloadToTheCallersToken()
    {
        var client = CreateClient(Stalling(), Timeout.InfiniteTimeSpan);
        using var caller = new CancellationTokenSource();

        var call = Convert(client, caller.Token);
        await Task.Delay(TimeSpan.FromSeconds(1));

        call.IsCompleted.Should().BeFalse("nothing but the caller's token limits the download");

        await caller.CancelAsync();

        (await FailureOf(call)).Should().BeAssignableTo<OperationCanceledException>()
            .Which.CancellationToken.Should().Be(caller.Token);
    }

    [Test]
    public async Task TimeOut_CountsTheVersionLookup()
    {
        // Each request alone takes less than TimeOut, both together more.
        var transport = new Transport(
            async cancellationToken =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(600), cancellationToken);
                return Pdf(new MemoryStream([1, 2, 3]));
            },
            async cancellationToken =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(600), cancellationToken);
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("8.40.0") };
            });
        var client = CreateClient(transport, TimeSpan.FromSeconds(1));
        client.ApiVersion = GotenbergApiVersion.Auto;

        var failure = await FailureOf(Convert(client));

        failure.Should().BeOfType<TaskCanceledException>()
            .Which.InnerException.Should().BeOfType<TimeoutException>();
        transport.Calls.Should().Be(2);
    }

    [TestCase("version", TestName = "TimeOut ends a version lookup whose body stalls, with a timeout")]
    [TestCase("metadata", TestName = "TimeOut ends a metadata read whose body stalls, with a timeout")]
    public async Task TimeOut_EndsATextCallWhoseBodyStalls_WithATimeout(string call)
    {
        var transport = new Transport(
            _ => Task.FromResult(Pdf(new SlowBody(chunks: 2, pause: Timeout.InfiniteTimeSpan))),
            _ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(new SlowBody(chunks: 2, pause: Timeout.InfiniteTimeSpan))
            }));
        var client = CreateClient(transport, Short);
        client.EnforceMinimumVersion = false;

        var failure = await FailureOf(call == "version"
            ? (Task)client.GetVersion()
            : client.ReadPdfMetadataAsync(PdfEngineBuilders.ReadMetadata().WithPdfs(a => a.AddItem("in.pdf", new byte[] { 1 }))));

        failure.Should().BeOfType<TaskCanceledException>()
            .Which.InnerException.Should().BeOfType<TimeoutException>();
    }

    private static Transport Stalling() =>
        new(_ => Task.FromResult(Pdf(new SlowBody(chunks: 2, pause: Timeout.InfiniteTimeSpan))));

    private static Task<Stream> Convert(
        Gotenberg.Sharp.API.Client.GotenbergSharpClient client,
        CancellationToken cancelToken = default) =>
        client.HtmlToPdfAsync(
            new HtmlRequestBuilder().AddDocument(doc => doc.SetBody("<html><body>hi</body></html>")),
            cancelToken);

    /// <summary>A client of <paramref name="transport" /> with an explicit API, so that a conversion sends no version lookup.</summary>
    private static Gotenberg.Sharp.API.Client.GotenbergSharpClient CreateClient(HttpMessageHandler transport, TimeSpan timeOut) =>
        new(new HttpClient(transport) { BaseAddress = new Uri("http://gotenberg.invalid"), Timeout = timeOut })
        {
            ApiVersion = GotenbergApiVersion.V8
        };

    private static HttpResponseMessage Pdf(Stream body) => new(HttpStatusCode.OK)
    {
        Content = new StreamContent(body) { Headers = { ContentType = new MediaTypeHeaderValue("application/pdf") } }
    };

    /// <summary>The exception <paramref name="call" /> ends with; the test fails when the call is still running after <see cref="Guard" />.</summary>
    private static async Task<Exception> FailureOf(Task call)
    {
        (await Task.WhenAny(call, Task.Delay(Guard))).Should().BeSameAs(call, $"the call should end within {Guard}");

        try
        {
            await call;
        }
        catch (Exception ex)
        {
            return ex;
        }

        throw new AssertionException("The call succeeded.");
    }

    /// <summary>Reads the request body, as a real transport does, then answers <c>/version</c> with <paramref name="version" /> and every other request with <paramref name="respond" />.</summary>
    private sealed class Transport(
        Func<CancellationToken, Task<HttpResponseMessage>> respond,
        Func<CancellationToken, Task<HttpResponseMessage>>? version = null) : HttpMessageHandler
    {
        private int _calls;

        public int Calls => this._calls;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref this._calls);

            if (request.Content is not null)
            {
                await request.Content.CopyToAsync(Stream.Null, cancellationToken);
            }

            if (request.RequestUri!.AbsolutePath.EndsWith("/version", StringComparison.Ordinal))
            {
                return version != null
                    ? await version(cancellationToken)
                    : new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("8.40.0") };
            }

            return await respond(cancellationToken);
        }
    }
}
