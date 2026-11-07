using System.Net;

using Gotenberg.Sharp.API.Client.Domain.Settings;
using Gotenberg.Sharp.API.Client.Extensions;
using Gotenberg.Sharp.API.Client.Infrastructure;

using Microsoft.Extensions.DependencyInjection;

namespace GotenbergSharpClient.Tests;

[TestFixture]
public class RetryPredicateTests
{
    private const int RetryCount = 2;

    [TestCase(429, 1 + RetryCount, TestName = "Queue full (429) is retried")]
    [TestCase(502, 1 + RetryCount, TestName = "Bad gateway (502) is retried")]
    [TestCase(500, 1, TestName = "Conversion failure (500) is not retried")]
    [TestCase(503, 1, TestName = "Timed-out request (503) is not retried")]
    [TestCase(408, 1, TestName = "Request timeout (408) is not retried")]
    [TestCase(400, 1, TestName = "Bad request (400) is not retried")]
    public async Task Status_IsRetriedOnlyWhenTransient(int status, int expectedCalls)
    {
        var handler = new StubHandler(() => new HttpResponseMessage((HttpStatusCode)status)
        {
            Content = new StringContent("error")
        });

        var client = CreateClient(handler);

        var act = () => client.GetVersion();

        await act.Should().ThrowAsync<GotenbergApiException>();
        handler.Calls.Should().Be(expectedCalls);
    }

    [Test]
    public async Task ConnectionFailure_IsRetried()
    {
        var handler = new StubHandler(() => throw new HttpRequestException("connection refused"));

        var client = CreateClient(handler);

        var act = () => client.GetVersion();

        await act.Should().ThrowAsync<HttpRequestException>();
        handler.Calls.Should().Be(1 + RetryCount);
    }

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

    private sealed class StubHandler(Func<HttpResponseMessage> respond) : HttpMessageHandler
    {
        public int Calls { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Calls++;
            return Task.FromResult(respond());
        }
    }
}
