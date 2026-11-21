using Gotenberg.Sharp.API.Client.Domain.Settings;
using Gotenberg.Sharp.API.Client.Extensions;
using Gotenberg.Sharp.API.Client.Infrastructure.Pipeline;

using Microsoft.Extensions.DependencyInjection;

using Polly.NoOp;

namespace GotenbergSharpClient.Tests;

[TestFixture]
public class RetryPolicyTests
{
    private sealed class CustomClientOptions : GotenbergSharpClientOptions
    {
    }

    [Test]
    public void Policy_ReadsRetrySettingsFromRegisteredOptionsType()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions<CustomClientOptions>()
            .Configure(o => o.RetryPolicy = new RetryOptions { Enabled = true, RetryCount = 2 });

        using var sp = services.BuildServiceProvider();

        var policy = PolicyFactory.CreatePolicyFromSettings<CustomClientOptions>(sp, new HttpRequestMessage());

        policy.Should().NotBeAssignableTo<AsyncNoOpPolicy<HttpResponseMessage>>(
            "retry is enabled on the registered options subclass, even though the base options type is unconfigured");
    }

    [Test]
    public void Policy_IsNoOpWhenRetryDisabledOnRegisteredOptionsType()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions<GotenbergSharpClientOptions>()
            .Configure(o => o.RetryPolicy = new RetryOptions { Enabled = true });
        services.AddOptions<CustomClientOptions>()
            .Configure(o => o.RetryPolicy = new RetryOptions { Enabled = false });

        using var sp = services.BuildServiceProvider();

        var policy = PolicyFactory.CreatePolicyFromSettings<CustomClientOptions>(sp, new HttpRequestMessage());

        policy.Should().BeAssignableTo<AsyncNoOpPolicy<HttpResponseMessage>>();
    }

    [Test]
    public async Task Client_RegisteredWithOptionsSubclass_RetriesPerSubclassSettings()
    {
        var handler = new FailingHandler();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions<CustomClientOptions>()
            .Configure(o =>
            {
                o.ServiceUrl = new Uri("http://gotenberg.invalid");
                o.RetryPolicy = new RetryOptions { Enabled = true, RetryCount = 2, BackoffPower = 0 };
            });
        services.AddGotenbergSharpClient<CustomClientOptions>()
            .ConfigurePrimaryHttpMessageHandler(() => handler);

        using var sp = services.BuildServiceProvider();
        var client = sp.GetRequiredService<Gotenberg.Sharp.API.Client.GotenbergSharpClient>();

        var act = () => client.GetVersion();

        await act.Should().ThrowAsync<HttpRequestException>();
        handler.Calls.Should().Be(3, "one attempt plus the two retries configured on the subclass");
    }

    private sealed class FailingHandler : HttpMessageHandler
    {
        public int Calls { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Calls++;
            throw new HttpRequestException("connection refused");
        }
    }
}
