// Copyright 2019-2026 Chris Mohan, Jaben Cargman
//  and GotenbergSharpApiClient Contributors
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.

using Gotenberg.Sharp.API.Client.Application.Builders;
using Gotenberg.Sharp.API.Client.Application.Webhooks;
using Gotenberg.Sharp.API.Client.Domain.Shared;
using Gotenberg.Sharp.API.Client.Domain.Webhooks;
using Gotenberg.Sharp.API.Client.Infrastructure;

namespace GotenbergSharpClient.Tests;

/// <summary>
/// The webhook headers per Gotenberg API: Gotenberg 7 and 8 keep the error url (and take the events
/// url from 8.29.0), Gotenberg 9 requires the events url and has no error url.
/// </summary>
[TestFixture]
public class WebhookTests
{
    private const string Target = "https://hooks.example.com/done";

    private const string Error = "https://hooks.example.com/error";

    private const string Events = "https://hooks.example.com/events";

    private const string ExtraHeader = "Gotenberg-Webhook-Extra-Http-Headers: {\"X-Hook-Key\":\"secret\"}";

    private static HtmlRequestBuilder Html(Action<WebhookBuilder> webhook) => new HtmlRequestBuilder()
        .AddDocument(d => d.SetBody("<html><body>body</body></html>"))
        .ConfigureRequest(c => c.AddWebhook(webhook));

    private static string[] WebhookHeaders(RenderedRequest rendered) => rendered.Headers
        .Where(h => h.Name.StartsWith("Gotenberg-Webhook-", StringComparison.Ordinal))
        .Select(h => $"{h.Name}: {h.Value}")
        .ToArray();

    #region Headers

    public static IEnumerable<TestCaseData> HeaderCases()
    {
        TestCaseData Row(string name, Action<WebhookBuilder> webhook, string[] v8, string[]? v9) =>
            new TestCaseData(webhook, v8, v9).SetArgDisplayNames(name);

        yield return Row(
            "error url",
            w => w.SetUrl(Target).SetErrorUrl(Error),
            [$"Gotenberg-Webhook-Url: {Target}", $"Gotenberg-Webhook-Error-Url: {Error}"],
            null);

        yield return Row(
            "error url and both methods",
            w => w.SetUrl(Target, HttpMethod.Put).SetErrorUrl(Error, HttpMethod.Patch),
            [
                $"Gotenberg-Webhook-Url: {Target}", "Gotenberg-Webhook-Method: PUT",
                $"Gotenberg-Webhook-Error-Url: {Error}", "Gotenberg-Webhook-Error-Method: PATCH"
            ],
            null);

        yield return Row(
            "error url and extra headers",
            w => w.SetUrl(Target).SetErrorUrl(Error).AddExtraHeader("X-Hook-Key", "secret"),
            [$"Gotenberg-Webhook-Url: {Target}", $"Gotenberg-Webhook-Error-Url: {Error}", ExtraHeader],
            null);

        yield return Row(
            "events url alone",
            w => w.SetUrl(Target).SetEventsUrl(Events),
            [$"Gotenberg-Webhook-Url: {Target}", $"Gotenberg-Webhook-Events-Url: {Events}"],
            [$"Gotenberg-Webhook-Url: {Target}", $"Gotenberg-Webhook-Events-Url: {Events}"]);

        yield return Row(
            "events url, method and extra headers",
            w => w.SetUrl(Target, HttpMethod.Put).SetEventsUrl(Events).AddExtraHeader("X-Hook-Key", "secret"),
            [$"Gotenberg-Webhook-Url: {Target}", "Gotenberg-Webhook-Method: PUT", $"Gotenberg-Webhook-Events-Url: {Events}", ExtraHeader],
            [$"Gotenberg-Webhook-Url: {Target}", "Gotenberg-Webhook-Method: PUT", $"Gotenberg-Webhook-Events-Url: {Events}", ExtraHeader]);

        yield return Row(
            "error url and events url",
            w => w.SetUrl(Target).SetErrorUrl(Error).SetEventsUrl(Events),
            [$"Gotenberg-Webhook-Url: {Target}", $"Gotenberg-Webhook-Error-Url: {Error}", $"Gotenberg-Webhook-Events-Url: {Events}"],
            [$"Gotenberg-Webhook-Url: {Target}", $"Gotenberg-Webhook-Events-Url: {Events}"]);

        yield return Row(
            "everything",
            w => w
                .SetUrl(Target, HttpMethod.Put)
                .SetErrorUrl(Error, HttpMethod.Patch)
                .SetEventsUrl(Events)
                .AddExtraHeader("X-Hook-Key", "secret"),
            [
                $"Gotenberg-Webhook-Url: {Target}", "Gotenberg-Webhook-Method: PUT",
                $"Gotenberg-Webhook-Error-Url: {Error}", "Gotenberg-Webhook-Error-Method: PATCH",
                $"Gotenberg-Webhook-Events-Url: {Events}", ExtraHeader
            ],
            [$"Gotenberg-Webhook-Url: {Target}", "Gotenberg-Webhook-Method: PUT", $"Gotenberg-Webhook-Events-Url: {Events}", ExtraHeader]);
    }

    [TestCaseSource(nameof(HeaderCases))]
    public void Headers_PerApi(Action<WebhookBuilder> webhook, string[] v8, string[]? v9)
    {
        WebhookHeaders(Html(webhook).Build().RenderFor(GotenbergApiVersion.V8)).Should().Equal(v8);

        if (v9 == null)
        {
            var act = () => Html(webhook).Build().RenderFor(GotenbergApiVersion.V9);

            act.Should().Throw<GotenbergVersionNotSupportedException>();
        }
        else
        {
            WebhookHeaders(Html(webhook).Build().RenderFor(GotenbergApiVersion.V9)).Should().Equal(v9);
        }
    }

    [Test]
    public void PublicGetHeaders_AreTheHeadersOfGotenberg8()
    {
        var webhook = new Webhook { TargetUrl = new Uri(Target), ErrorUrl = new Uri(Error), EventsUrl = new Uri(Events) };

        webhook.GetHeaders().Select(h => h.Item1).Should().Equal(
            "Gotenberg-Webhook-Url",
            "Gotenberg-Webhook-Error-Url",
            "Gotenberg-Webhook-Events-Url");
    }

    #endregion

    #region Validation

    [Test]
    public void TargetUrlAlone_IsNotValidAndNotConfigured()
    {
        var webhook = new Webhook { TargetUrl = new Uri(Target) };

        webhook.IsConfigured().Should().BeFalse();
        webhook.Invoking(w => w.Validate()).Should().Throw<ArgumentNullException>().WithMessage("*error url or events url*");
    }

    [Test]
    public void ErrorUrl_OrEventsUrl_MakesTheWebhookValid()
    {
        foreach (var webhook in new[]
                 {
                     new Webhook { TargetUrl = new Uri(Target), ErrorUrl = new Uri(Error) },
                     new Webhook { TargetUrl = new Uri(Target), EventsUrl = new Uri(Events) }
                 })
        {
            webhook.IsConfigured().Should().BeTrue();
            webhook.Invoking(w => w.Validate()).Should().NotThrow();
        }
    }

    [Test]
    public void WithoutATargetUrl_NothingIsConfiguredAndNothingIsSent()
    {
        var webhook = new Webhook { EventsUrl = new Uri(Events) };

        webhook.IsConfigured().Should().BeFalse();
        webhook.Invoking(w => w.Validate()).Should().NotThrow();
        webhook.GetHeaders().Should().BeEmpty();
    }

    [Test]
    public void EventsUrl_MustBeAbsolute()
    {
        var webhook = new Webhook();

        webhook.Invoking(w => w.EventsUrl = new Uri("/events", UriKind.Relative))
            .Should().Throw<InvalidOperationException>();
        webhook.Invoking(w => w.EventsUrl = null!).Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void SetEventsUrl_ValidatesLikeSetErrorUrl()
    {
        var builder = new WebhookBuilder(new Webhook());

        builder.Invoking(b => b.SetEventsUrl("")).Should().Throw<ArgumentException>();
        builder.Invoking(b => b.SetEventsUrl((string)null!)).Should().Throw<ArgumentException>();
        builder.Invoking(b => b.SetEventsUrl((Uri)null!)).Should().Throw<ArgumentNullException>();
        builder.Invoking(b => b.SetEventsUrl(new Uri("/events", UriKind.Relative))).Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void RequestWithOnlyATargetUrl_IsRejectedWhenBuilt()
    {
        var act = () => Html(w => w.SetUrl(Target)).Build().RenderFor(GotenbergApiVersion.V8);

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Gotenberg 9 without an events url

    [Test]
    public void ErrorUrlWithoutEventsUrl_IsRemovedOnGotenberg9()
    {
        var request = Html(w => w.SetUrl(Target).SetErrorUrl(Error)).Build();

        var act = () => request.RenderFor(GotenbergApiVersion.V9, GotenbergVersion.Parse("9.5.1"));

        var thrown = act.Should().Throw<GotenbergVersionNotSupportedException>().Which;

        thrown.RemovedInVersion.Should().Be(GotenbergVersion.Parse("9.0.0"));
        thrown.RunningVersion.Should().Be(GotenbergVersion.Parse("9.5.1"));
        thrown.Replacement.Should().Contain("WebhookBuilder.SetEventsUrl").And.Contain("webhook.error");
    }

    [Test]
    public async Task ErrorUrlWithoutEventsUrl_IsNotSentToGotenberg9ButSentToGotenberg8()
    {
        var request = Html(w => w.SetUrl(Target).SetErrorUrl(Error)).Build();

        var nine = new GotenbergVersionTests.StubVersionClient("9.5.1");
        var eight = new GotenbergVersionTests.StubVersionClient("8.40.0");

        var fire = async () => await nine.FireWebhookAndForgetAsync(request);

        (await fire.Should().ThrowExactlyAsync<GotenbergVersionNotSupportedException>())
            .Which.RunningVersion.Should().Be(GotenbergVersion.Parse("9.5.1"));
        nine.SendCount.Should().Be(0, "the request must never reach the wire");

        (await nine.SupportsAsync(request)).Should().BeFalse();
        (await eight.SupportsAsync(request)).Should().BeTrue();

        var ensure = async () => await nine.EnsureSupportedAsync(request);

        await ensure.Should().ThrowExactlyAsync<GotenbergVersionNotSupportedException>();
        await eight.Invoking(c => c.EnsureSupportedAsync(request)).Should().NotThrowAsync();

        var fireOnEight = async () => await eight.FireWebhookAndForgetAsync(request);

        await fireOnEight.Should().ThrowExactlyAsync<GotenbergVersionTests.StubVersionClient.SendReachedException>();
        eight.SendCount.Should().Be(1);
    }

    [Test]
    public async Task EventsUrl_IsSupportedAndSentOnGotenberg9()
    {
        var request = Html(w => w.SetUrl(Target).SetEventsUrl(Events)).Build();

        var nine = new GotenbergVersionTests.StubVersionClient("9.5.1");

        (await nine.SupportsAsync(request)).Should().BeTrue();

        var fire = async () => await nine.FireWebhookAndForgetAsync(request);

        await fire.Should().ThrowExactlyAsync<GotenbergVersionTests.StubVersionClient.SendReachedException>();
        nine.SendCount.Should().Be(1);
    }

    [Test]
    public void RequestWithoutAWebhook_IsUnaffectedOnGotenberg9()
    {
        var request = new HtmlRequestBuilder().AddDocument(d => d.SetBody("<html></html>")).Build();

        request.Invoking(r => r.RenderFor(GotenbergApiVersion.V9)).Should().NotThrow();
    }

    #endregion

    #region Integration

    // Gotenberg answers 204 at once and delivers later, so the target only has to be a host the
    // server accepts: a literal loopback address resolves, is not filtered by default, and needs no
    // listener (the failed delivery is reported to the events url, which fails as well).
    private const string Unreachable = "http://127.0.0.1:9/webhook";

    private static async Task Fire(GotenbergServer gotenberg, Action<WebhookBuilder> webhook)
    {
        var client = await gotenberg.CreateClientAsync();

        await client.FireWebhookAndForgetAsync(Html(webhook));
    }

    [GotenbergTest(until: "9.0.0")]
    public async Task ErrorUrl_IsAcceptedByGotenberg7And8(GotenbergServer gotenberg)
    {
        var act = () => Fire(gotenberg, w => w.SetUrl(Unreachable).SetErrorUrl(Unreachable));

        await act.Should().NotThrowAsync();
    }

    [GotenbergTest("8.29.0")]
    public async Task EventsUrl_IsAcceptedFromGotenberg8_29(GotenbergServer gotenberg)
    {
        var act = () => Fire(gotenberg, w => w.SetUrl(Unreachable).SetEventsUrl(Unreachable));

        await act.Should().NotThrowAsync();
    }

    [GotenbergTest("8.29.0")]
    public async Task ErrorUrlAndEventsUrl_AreAcceptedFromGotenberg8_29(GotenbergServer gotenberg)
    {
        var act = () => Fire(gotenberg, w => w.SetUrl(Unreachable).SetErrorUrl(Unreachable).SetEventsUrl(Unreachable));

        await act.Should().NotThrowAsync();
    }

    #endregion
}
