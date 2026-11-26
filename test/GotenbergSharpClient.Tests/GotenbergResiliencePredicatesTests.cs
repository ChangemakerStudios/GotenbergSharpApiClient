using System.Net;

using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Infrastructure.Pipeline;

using Polly.Timeout;

namespace GotenbergSharpClient.Tests;

[TestFixture]
public class GotenbergResiliencePredicatesTests
{
    [TestCase(429, true, TestName = "Queue full (429) is transient")]
    [TestCase(502, true, TestName = "Bad gateway (502) is transient")]
    [TestCase(503, true, TestName = "Service unavailable (503) is transient")]
    [TestCase(500, false, TestName = "Conversion failure (500) is not transient")]
    [TestCase(504, false, TestName = "Gateway timeout (504) is not transient")]
    [TestCase(408, false, TestName = "Request timeout (408) is not transient")]
    [TestCase(400, false, TestName = "Bad request (400) is not transient")]
    [TestCase(200, false, TestName = "Success (200) is not a failure")]
    public void Status_IsTransientOnlyWhenAResendCanFixIt(int status, bool expected)
    {
        using var response = new HttpResponseMessage((HttpStatusCode)status);

        GotenbergResiliencePredicates.IsTransientFailure(response, null).Should().Be(expected);
    }

    [Test]
    public void ConnectionFailure_IsTransient() =>
        GotenbergResiliencePredicates.IsTransientFailure(null, new HttpRequestException("connection refused"))
            .Should().BeTrue();

    private static IEnumerable<TestCaseData> Timeouts()
    {
        yield return new TestCaseData(new TaskCanceledException("HttpClient.Timeout", new TimeoutException()))
            .SetName("TimeOut running out is not transient");
        yield return new TestCaseData(new TimeoutRejectedException())
            .SetName("A timeout of the resilience pipeline is not transient");
    }

    [TestCaseSource(nameof(Timeouts))]
    public void Timeout_IsNotTransient(Exception timeout) =>
        GotenbergResiliencePredicates.IsTransientFailure(null, timeout)
            .Should().BeFalse("a conversion that ran out of time once is likely to do so again");

    [Test]
    public void Cancellation_IsNotTransient() =>
        GotenbergResiliencePredicates.IsTransientFailure(null, new TaskCanceledException())
            .Should().BeFalse();

    [Test]
    public void NoOutcome_IsNotTransient() =>
        GotenbergResiliencePredicates.IsTransientFailure(null, null).Should().BeFalse();

    [Test]
    public void RequestWithoutContent_CanBeResent() =>
        GotenbergResiliencePredicates.CanResend(new HttpRequestMessage(HttpMethod.Get, "/version"))
            .Should().BeTrue();

    [Test]
    public void ByteArrayAndSeekableStreamContent_CanBeResent() =>
        GotenbergResiliencePredicates.CanResend(Post(
                new ContentItem(new byte[] { 1, 2, 3 }),
                new ContentItem(new MemoryStream(new byte[] { 1, 2, 3 }))))
            .Should().BeTrue();

    [Test]
    public void NonSeekableStreamContent_CannotBeResent() =>
        GotenbergResiliencePredicates.CanResend(Post(
                new ContentItem(new byte[] { 1, 2, 3 }),
                new ContentItem(new NonSeekableStream())))
            .Should().BeFalse();

    [Test]
    public void UnknownRequest_CannotBeResent() =>
        GotenbergResiliencePredicates.CanResend(null).Should().BeFalse();

    private static HttpRequestMessage Post(params ContentItem[] items)
    {
        var content = new MultipartFormDataContent();

        foreach (var item in items)
        {
            content.Add(item.ToHttpContentItem(), "files", "file.bin");
        }

        return new HttpRequestMessage(HttpMethod.Post, "/forms/chromium/convert/html") { Content = content };
    }

    private sealed class NonSeekableStream : MemoryStream
    {
        public override bool CanSeek => false;
    }
}
