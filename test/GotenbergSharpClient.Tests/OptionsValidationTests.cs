using Gotenberg.Sharp.API.Client.Domain.Settings;
using Gotenberg.Sharp.API.Client.Domain.Shared;
using Gotenberg.Sharp.API.Client.Extensions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace GotenbergSharpClient.Tests;

[TestFixture]
public class OptionsValidationTests
{
    [Test]
    public void Defaults_AreValid() =>
        Read<GotenbergSharpClientOptions>(_ => { }).ServiceUrl.Should().Be(new Uri("http://localhost:3000"));

    [Test]
    public void RelativeServiceUrl_IsRejected() =>
        AssertRejected(o => o.ServiceUrl = new Uri("/gotenberg", UriKind.Relative), nameof(GotenbergSharpClientOptions.ServiceUrl));

    [Test]
    public void ServiceUrlWithoutScheme_IsRejected() =>
        AssertRejected(o => o.ServiceUrl = new Uri("localhost:3000"), nameof(GotenbergSharpClientOptions.ServiceUrl));

    [Test]
    public void ZeroTimeout_IsRejected() =>
        AssertRejected(o => o.TimeOut = TimeSpan.Zero, nameof(GotenbergSharpClientOptions.TimeOut));

    [Test]
    public void InfiniteTimeout_IsAccepted() =>
        Read<GotenbergSharpClientOptions>(o => o.TimeOut = Timeout.InfiniteTimeSpan)
            .TimeOut.Should().Be(Timeout.InfiniteTimeSpan);

#pragma warning disable CS0618 // These tests check what happens to the obsolete retry settings.
    [Test]
    public void EnabledRetry_IsRejected_WithAPointerToTheResilienceDocs() =>
        AssertRejected(o => o.RetryPolicy = new RetryOptions { Enabled = true }, "/resilience/");

    [Test]
    public void EnabledRetryFromConfiguration_IsRejected()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GotenbergSharpClient:ServiceUrl"] = "http://gotenberg:3000",
                ["GotenbergSharpClient:RetryPolicy:Enabled"] = "true",
                ["GotenbergSharpClient:RetryPolicy:RetryCount"] = "4"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddOptions<GotenbergSharpClientOptions>().Bind(configuration.GetSection("GotenbergSharpClient"));
        services.AddGotenbergSharpClient();

        var act = () => services.BuildServiceProvider().GetRequiredService<Gotenberg.Sharp.API.Client.GotenbergSharpClient>();

        act.Should().Throw<OptionsValidationException>()
            .WithMessage($"*{nameof(GotenbergSharpClientOptions.RetryPolicy)}.{nameof(RetryOptions.Enabled)} is not supported*");
    }

    [Test]
    public void DisabledRetry_IsAccepted() =>
        Read<GotenbergSharpClientOptions>(o => o.RetryPolicy = new RetryOptions { Enabled = false, RetryCount = -1 })
            .RetryPolicy.RetryCount.Should().Be(-1, "nothing reads the retry settings any more");
#pragma warning restore CS0618

    [Test]
    public void UndefinedApiVersion_IsRejected() =>
        AssertRejected(o => o.ApiVersion = (GotenbergApiVersion)7, nameof(GotenbergSharpClientOptions.ApiVersion));

    [Test]
    public void OptionsSubclass_IsValidated()
    {
        var act = () => Read<SubclassOptions>(o => o.TimeOut = TimeSpan.Zero);

        act.Should().Throw<OptionsValidationException>()
            .WithMessage($"*{nameof(GotenbergSharpClientOptions.TimeOut)}*");
    }

    private static void AssertRejected(Action<GotenbergSharpClientOptions> configure, string expectedInMessage)
    {
        var act = () => Read(configure);

        act.Should().Throw<OptionsValidationException>().WithMessage($"*{expectedInMessage}*");
    }

    private static TOptions Read<TOptions>(Action<TOptions> configure)
        where TOptions : GotenbergSharpClientOptions, new()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions<TOptions>().Configure(configure);
        services.AddGotenbergSharpClient<TOptions>();

        return services.BuildServiceProvider().GetRequiredService<IOptions<TOptions>>().Value;
    }

    private sealed class SubclassOptions : GotenbergSharpClientOptions;
}
