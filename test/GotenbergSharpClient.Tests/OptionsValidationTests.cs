using Gotenberg.Sharp.API.Client.Domain.Settings;
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

    [Test]
    public void NegativeRetryCount_IsRejectedWhenRetryIsEnabled() =>
        AssertRejected(o => o.RetryPolicy = new RetryOptions { Enabled = true, RetryCount = -1 }, nameof(RetryOptions.RetryCount));

    [Test]
    public void NegativeBackoff_IsRejectedWhenRetryIsEnabled() =>
        AssertRejected(o => o.RetryPolicy = new RetryOptions { Enabled = true, BackoffPower = -1 }, nameof(RetryOptions.BackoffPower));

    [Test]
    public void NegativeRetryCount_IsIgnoredWhenRetryIsDisabled() =>
        Read<GotenbergSharpClientOptions>(o => o.RetryPolicy = new RetryOptions { Enabled = false, RetryCount = -1 })
            .RetryPolicy.RetryCount.Should().Be(-1);

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
