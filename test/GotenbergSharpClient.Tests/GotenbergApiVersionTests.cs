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

using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;

using Gotenberg.Sharp.API.Client.Application.Builders;
using Gotenberg.Sharp.API.Client.Domain.PdfFormat;
using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Settings;
using Gotenberg.Sharp.API.Client.Domain.Shared;
using Gotenberg.Sharp.API.Client.Extensions;
using Gotenberg.Sharp.API.Client.Infrastructure;

using Microsoft.Extensions.DependencyInjection;

using Client = Gotenberg.Sharp.API.Client.GotenbergSharpClient;

namespace GotenbergSharpClient.Tests;

/// <summary>
/// How the client picks the Gotenberg API it renders requests for, and how it caches and refreshes
/// the version of the service behind that choice.
/// </summary>
[TestFixture]
public class GotenbergApiVersionTests
{
    private static readonly Uri Address = new("http://gotenberg.invalid:3000");

    private static readonly byte[] Pdf = "%PDF-1.4 fake"u8.ToArray();

    private static HtmlRequestBuilder Html() => new HtmlRequestBuilder()
        .AddDocument(d => d.SetBody("<html><body>hi</body></html>"));

    private static Client DirectClient(FakeGotenberg gotenberg, Uri? address = null) =>
        new(new HttpClient(gotenberg) { BaseAddress = address ?? Address });

    #region Resolution

    [TestCase(GotenbergApiVersion.V8)]
    [TestCase(GotenbergApiVersion.V9)]
    public async Task ExplicitApi_IsUsedWithoutAnyRequest(GotenbergApiVersion api)
    {
        var gotenberg = new FakeGotenberg("8.30.0");
        var client = DirectClient(gotenberg);
        client.ApiVersion = api;

        (await client.GetApiVersionAsync()).Should().Be(api);
        await client.HtmlToPdfAsync(Html());

        gotenberg.VersionRequests.Should().Be(0);
        gotenberg.ConversionRequests.Should().Be(1);
    }

    [TestCase("7.10.0", GotenbergApiVersion.V8)]
    [TestCase("8.0.0", GotenbergApiVersion.V8)]
    [TestCase("8.40.0", GotenbergApiVersion.V8)]
    [TestCase("8.17.0-live-demo-snapshot", GotenbergApiVersion.V8)]
    [TestCase("9.0.0", GotenbergApiVersion.V9)]
    [TestCase("9.5.1", GotenbergApiVersion.V9)]
    [TestCase("10.0.0", GotenbergApiVersion.V9)]
    [TestCase("snapshot", GotenbergApiVersion.V8)]
    public async Task Auto_FollowsTheMajorTheServiceReports(string reported, GotenbergApiVersion expected)
    {
        var client = DirectClient(new FakeGotenberg(reported));

        client.ApiVersion.Should().Be(GotenbergApiVersion.Auto);
        (await client.GetApiVersionAsync()).Should().Be(expected);
    }

    [Test]
    public async Task Auto_WithoutAVersionRoute_UsesV8()
    {
        // Gotenberg 7 and 8.0-8.4 predate /version and answer 404.
        var gotenberg = new FakeGotenberg((string?)null);
        var client = DirectClient(gotenberg);

        (await client.GetApiVersionAsync()).Should().Be(GotenbergApiVersion.V8);
        (await client.GetGotenbergVersionAsync()).IsKnown.Should().BeFalse();

        gotenberg.VersionRequests.Should().Be(1, "the 404 is cached as an unknown version");
    }

    [TestCase(HttpStatusCode.Unauthorized)]
    [TestCase(HttpStatusCode.Forbidden)]
    [TestCase(HttpStatusCode.InternalServerError)]
    [TestCase(HttpStatusCode.ServiceUnavailable)]
    public async Task Auto_WhenTheVersionLookupFails_ThrowsAClearErrorAndDoesNotCache(HttpStatusCode status)
    {
        var gotenberg = new FakeGotenberg("8.30.0") { VersionFailure = status };
        var client = DirectClient(gotenberg);

        var lookup = async () => await client.GetApiVersionAsync();
        var thrown = (await lookup.Should().ThrowExactlyAsync<GotenbergApiException>()).Which;

        thrown.StatusCode.Should().Be(status);
        thrown.RequestUri!.AbsolutePath.Should().Be("/version");
        thrown.Message.Should().Contain($"Gotenberg answered {(int)status} (")
            .And.Contain("GET /version")
            .And.Contain("GotenbergSharpClientOptions.ApiVersion")
            .And.Contain("V8 or V9")
            .And.Contain("With V8, a request that needs a minimum Gotenberg release")
            .And.EndWith("Gotenberg's response: failed");
        thrown.InnerException.Should().BeOfType<GotenbergApiException>()
            .Which.Should().Match<GotenbergApiException>(inner => inner.Message == "failed" && inner.StatusCode == status);

        if (status == HttpStatusCode.Unauthorized)
            thrown.Message.Should().Contain("refused the credentials").And.Contain("BasicAuthUsername").And.Contain("BasicAuthPassword");
        else
            thrown.Message.Should().NotContain("credentials");

        var convert = async () => await client.HtmlToPdfAsync(Html());
        (await convert.Should().ThrowExactlyAsync<GotenbergApiException>()).Which.Message.Should().Be(thrown.Message);

        gotenberg.ConversionRequests.Should().Be(0, "nothing is sent while the API is unknown");
        gotenberg.VersionRequests.Should().Be(2, "a failed lookup is retried by the next request, not cached");
    }

    [Test]
    public async Task Auto_ErrorOfTheVersionLookup_KeepsWhatTheOriginalReports()
    {
        var gotenberg = new FakeGotenberg("8.30.0")
        {
            VersionFailure = HttpStatusCode.BadGateway,
            ResponseTrace = "trace-1"
        };
        var client = DirectClient(gotenberg);

        var lookup = async () => await client.GetGotenbergVersionAsync();
        var thrown = (await lookup.Should().ThrowExactlyAsync<GotenbergApiException>()).Which;
        var original = (GotenbergApiException)thrown.InnerException!;

        thrown.StatusCode.Should().Be(original.StatusCode);
        thrown.ReasonPhrase.Should().Be(original.ReasonPhrase).And.Be("Bad Gateway");
        thrown.RequestUri.Should().Be(original.RequestUri);
        thrown.Trace.Should().Be(original.Trace).And.Be("trace-1");
        thrown.Message.Should().Contain("502 (Bad Gateway)");
        thrown.ToVerboseJson().Should().Contain("GotenbergResponseReceived").And.Contain("Bad Gateway").And.Contain("/version");
        thrown.ToVerboseJson(includeGotenbergResponse: false, indentJson: true).Should().Contain("GotenbergMessage");
    }

    [Test]
    public async Task Auto_WhenTheVersionLookupFailsWithoutABody_StillSaysWhatToDo()
    {
        var gotenberg = new FakeGotenberg("8.30.0") { VersionFailure = HttpStatusCode.Forbidden, VersionFailureBody = "" };
        var client = DirectClient(gotenberg);

        var lookup = async () => await client.GetApiVersionAsync();
        var thrown = (await lookup.Should().ThrowExactlyAsync<GotenbergApiException>()).Which;

        thrown.Message.Should().Contain("403 (Forbidden)").And.Contain("to skip the lookup.").And.EndWith("EnforceMinimumVersion is false.");
    }

    [Test]
    public async Task Auto_DoesNotRewrapAConnectionFailure()
    {
        var gotenberg = new FakeGotenberg("8.30.0") { VersionException = new HttpRequestException("refused") };
        var client = DirectClient(gotenberg);

        var lookup = async () => await client.GetApiVersionAsync();

        (await lookup.Should().ThrowExactlyAsync<HttpRequestException>()).Which.Message.Should().Be("refused");
    }

    [Test]
    public async Task Auto_ARecoveredServiceIsReadOnTheNextRequest()
    {
        var gotenberg = new FakeGotenberg("9.5.1") { VersionFailure = HttpStatusCode.ServiceUnavailable };
        var client = DirectClient(gotenberg);

        var failing = async () => await client.HtmlToPdfAsync(Html());
        await failing.Should().ThrowExactlyAsync<GotenbergApiException>();

        gotenberg.VersionFailure = null;
        await client.HtmlToPdfAsync(Html());

        (await client.GetApiVersionAsync()).Should().Be(GotenbergApiVersion.V9);
        gotenberg.VersionRequests.Should().Be(2);
    }

    [TestCase(HttpStatusCode.Unauthorized)]
    [TestCase(HttpStatusCode.Forbidden)]
    [TestCase(HttpStatusCode.InternalServerError)]
    [TestCase(HttpStatusCode.ServiceUnavailable)]
    public async Task ExplicitV8_WhenTheVersionLookupForAGatedRequestFails_NamesEnforceMinimumVersion(HttpStatusCode status)
    {
        var gotenberg = new FakeGotenberg("8.30.0") { VersionFailure = status };
        var client = DirectClient(gotenberg);
        client.ApiVersion = GotenbergApiVersion.V8;

        var rotate = async () => await client.ExecutePdfEngineAsync(Rotate());
        var thrown = (await rotate.Should().ThrowExactlyAsync<GotenbergApiException>()).Which;

        thrown.StatusCode.Should().Be(status);
        thrown.InnerException.Should().BeOfType<GotenbergApiException>();
        thrown.Message.Should().Contain($"Gotenberg answered {(int)status} (")
            .And.Contain("GET /version")
            .And.Contain("minimum Gotenberg release")
            .And.Contain("GotenbergSharpClient.EnforceMinimumVersion")
            .And.Contain("to false")
            .And.EndWith("Gotenberg's response: failed")
            .And.NotContain("V8 or V9");
        (thrown.Message.Contains("refused the credentials")).Should().Be(status == HttpStatusCode.Unauthorized);

        gotenberg.ConversionRequests.Should().Be(0);

        await rotate.Should().ThrowExactlyAsync<GotenbergApiException>();
        gotenberg.VersionRequests.Should().Be(2, "a failed lookup is not cached");
    }

    [Test]
    public async Task ExplicitV8_WithTheCheckDisabled_NeverReadsTheVersion()
    {
        var gotenberg = new FakeGotenberg("8.30.0") { VersionFailure = HttpStatusCode.Forbidden };
        var client = DirectClient(gotenberg);
        client.ApiVersion = GotenbergApiVersion.V8;
        client.EnforceMinimumVersion = false;

        await client.ExecutePdfEngineAsync(Rotate());

        gotenberg.VersionRequests.Should().Be(0);
        gotenberg.ConversionRequests.Should().Be(1);
    }

    [Test]
    public async Task ExplicitV9_SatisfiesTheMinimumOfAGatedRequestWithoutReadingTheVersion()
    {
        // Were the version read, the 403 would stop the request.
        var gotenberg = new FakeGotenberg("8.1.0") { VersionFailure = HttpStatusCode.Forbidden };
        var client = DirectClient(gotenberg);
        client.ApiVersion = GotenbergApiVersion.V9;

        var request = Rotate().Build();

        (await client.SupportsAsync(request)).Should().BeTrue();
        await client.Invoking(c => c.EnsureSupportedAsync(request)).Should().NotThrowAsync();
        await client.ExecutePdfEngineAsync(Rotate());

        gotenberg.VersionRequests.Should().Be(0);
        gotenberg.ConversionRequests.Should().Be(1);
    }

    [Test]
    public async Task ExplicitV8_ReadsTheVersionForTheMinimumOfAGatedRequest()
    {
        var gotenberg = new FakeGotenberg("8.1.0");
        var client = DirectClient(gotenberg);
        client.ApiVersion = GotenbergApiVersion.V8;

        var request = Rotate().Build();

        (await client.SupportsAsync(request)).Should().BeFalse();
        await client.Invoking(c => c.EnsureSupportedAsync(request)).Should().ThrowExactlyAsync<GotenbergVersionNotSupportedException>();
        gotenberg.VersionRequests.Should().Be(1);
    }

    [TestCase("8.30.0", false)]
    [TestCase("9.0.0", true)]
    [TestCase("9.2.0", true)]
    public async Task ExplicitV9_ReadsTheVersionForAMinimumOfGotenberg9OrNewer(string reported, bool supported)
    {
        var gotenberg = new FakeGotenberg(reported);
        var client = DirectClient(gotenberg);
        client.ApiVersion = GotenbergApiVersion.V9;

        var request = WithPdf(new NewerThanEightRequest());

        (await client.SupportsAsync(request)).Should().Be(supported);

        var ensure = async () => await client.EnsureSupportedAsync(request);

        if (supported)
            await ensure.Should().NotThrowAsync();
        else
            await ensure.Should().ThrowExactlyAsync<GotenbergVersionNotSupportedException>();

        gotenberg.VersionRequests.Should().Be(1);
    }

    private static PdfEngineBuilder<RotatePdfRequest> Rotate() =>
        PdfEngineBuilders.Rotate(90).WithPdfs(a => a.AddItem("in.pdf", Pdf));

    [Test]
    public async Task Auto_ReadsTheVersionOnceForGatedAndUngatedRequests()
    {
        var gotenberg = new FakeGotenberg("8.30.0");
        var client = DirectClient(gotenberg);

        await client.HtmlToPdfAsync(Html());
        await client.ExecutePdfEngineAsync(PdfEngineBuilders.Rotate(90).WithPdfs(a => a.AddItem("in.pdf", Pdf)));
        await client.HtmlToPdfAsync(Html());

        gotenberg.VersionRequests.Should().Be(1);
        gotenberg.ConversionRequests.Should().Be(3);
    }

    [Test]
    public void ApiVersion_RejectsUndefinedValues()
    {
        var client = DirectClient(new FakeGotenberg("8.30.0"));

        var act = () => client.ApiVersion = (GotenbergApiVersion)7;

        act.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Test]
    public void RenderingForAuto_IsRejected()
    {
        var act = () => Html().Build().RenderFor(GotenbergApiVersion.Auto);

        act.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    #endregion

    #region Shared cache

    [Test]
    public async Task ClientsFromOneServiceProvider_ReadTheVersionOnce()
    {
        var gotenberg = new FakeGotenberg("8.30.0") { VersionDelay = TimeSpan.FromMilliseconds(50) };

        var services = new ServiceCollection();
        services.AddOptions<GotenbergSharpClientOptions>().Configure(o => o.ServiceUrl = Address);
        services.AddGotenbergSharpClient().ConfigurePrimaryHttpMessageHandler(() => gotenberg);
        using var provider = services.BuildServiceProvider();

        var conversions = Enumerable.Range(0, 20)
            .Select(_ => provider.GetRequiredService<Client>())
            .Select(client => client.HtmlToPdfAsync(Html()));

        await Task.WhenAll(conversions);

        gotenberg.VersionRequests.Should().Be(1);
        gotenberg.ConversionRequests.Should().Be(20);
    }

    [Test]
    public async Task ClientsForTwoAddresses_DoNotShareAVersion()
    {
        var eight = new Uri("http://gotenberg-8.invalid:3000");
        var nine = new Uri("http://gotenberg-9.invalid:3000");
        var gotenberg = new FakeGotenberg(request => request.RequestUri!.Host == nine.Host ? "9.5.1" : "8.30.0");

        var services = new ServiceCollection();
        var next = eight;
        services.AddGotenbergSharpClient((_, http) => http.BaseAddress = next)
            .ConfigurePrimaryHttpMessageHandler(() => gotenberg);
        using var provider = services.BuildServiceProvider();

        var clientFor8 = provider.GetRequiredService<Client>();
        next = nine;
        var clientFor9 = provider.GetRequiredService<Client>();
        next = eight;
        var anotherClientFor8 = provider.GetRequiredService<Client>();

        (await clientFor8.GetApiVersionAsync()).Should().Be(GotenbergApiVersion.V8);
        (await clientFor9.GetApiVersionAsync()).Should().Be(GotenbergApiVersion.V9);
        (await anotherClientFor8.GetApiVersionAsync()).Should().Be(GotenbergApiVersion.V8);

        gotenberg.VersionRequests.Should().Be(2, "one lookup per address");
    }

    [Test]
    public async Task DirectlyConstructedClients_KeepTheirOwnCache()
    {
        var gotenberg = new FakeGotenberg("8.30.0");

        await DirectClient(gotenberg).GetApiVersionAsync();
        await DirectClient(gotenberg).GetApiVersionAsync();

        gotenberg.VersionRequests.Should().Be(2);
    }

    [Test]
    public async Task ApiVersionOption_IsGivenToTheClient()
    {
        var gotenberg = new FakeGotenberg("8.30.0");

        var services = new ServiceCollection();
        services.AddOptions<GotenbergSharpClientOptions>()
            .Configure(o =>
            {
                o.ServiceUrl = Address;
                o.ApiVersion = GotenbergApiVersion.V9;
            });
        services.AddGotenbergSharpClient().ConfigurePrimaryHttpMessageHandler(() => gotenberg);
        using var provider = services.BuildServiceProvider();

        var client = provider.GetRequiredService<Client>();
        await client.HtmlToPdfAsync(Html());

        client.ApiVersion.Should().Be(GotenbergApiVersion.V9);
        gotenberg.VersionRequests.Should().Be(0);
    }

    #endregion

    #region Refresh from the Gotenberg-Version response header

    [Test]
    public async Task ResponseFromANewerMajor_SwitchesTheNextRequestToItsApi()
    {
        // The service is upgraded from 8 to 9 while the application runs.
        var gotenberg = new FakeGotenberg("8.30.0");
        var client = DirectClient(gotenberg);

        (await client.GetApiVersionAsync()).Should().Be(GotenbergApiVersion.V8);

        gotenberg.ResponseVersionHeader = "9.1.0";
        await client.HtmlToPdfAsync(Html());

        (await client.GetApiVersionAsync()).Should().Be(GotenbergApiVersion.V9);
        (await client.GetGotenbergVersionAsync()).Should().Be(GotenbergVersion.Parse("9.1.0"));
        gotenberg.VersionRequests.Should().Be(1, "the header replaces the cached version without another lookup");
    }

    [Test]
    public async Task FailedResponse_RefreshesTheVersionToo()
    {
        var gotenberg = new FakeGotenberg("8.30.0");
        var client = DirectClient(gotenberg);
        await client.GetApiVersionAsync();

        gotenberg.ResponseVersionHeader = "9.1.0";
        gotenberg.ConversionFailure = HttpStatusCode.BadRequest;

        var act = async () => await client.HtmlToPdfAsync(Html());
        await act.Should().ThrowExactlyAsync<GotenbergApiException>();

        (await client.GetApiVersionAsync()).Should().Be(GotenbergApiVersion.V9);
        gotenberg.ConversionRequests.Should().Be(1, "the failed request is not retried");
    }

    [Test]
    public async Task ResponseFromTheSameVersion_KeepsTheCachedVersion()
    {
        var gotenberg = new FakeGotenberg("8.30.0") { ResponseVersionHeader = "8.30.0" };
        var client = DirectClient(gotenberg);

        await client.HtmlToPdfAsync(Html());

        (await client.GetGotenbergVersionAsync()).Should().Be(GotenbergVersion.Parse("8.30.0"));
        gotenberg.VersionRequests.Should().Be(1);
    }

    [Test]
    public async Task ResponseFromAnotherMinor_ReplacesTheCachedVersion()
    {
        var gotenberg = new FakeGotenberg("8.30.0") { ResponseVersionHeader = "8.31.0" };
        var client = DirectClient(gotenberg);

        await client.HtmlToPdfAsync(Html());

        (await client.GetGotenbergVersionAsync()).Should().Be(GotenbergVersion.Parse("8.31.0"));
        gotenberg.VersionRequests.Should().Be(1);
    }

    [Test]
    public async Task InPlaceMinorUpgrade_IsSeenByAClientSharingTheCache()
    {
        // 8.20.0 predates the rotate route (8.28.0). The service is upgraded to 8.30.0 in place; a
        // conversion by one client tells the cache, and the route is then open to every client.
        var gotenberg = new FakeGotenberg("8.20.0");
        using var provider = SharedCacheProvider(gotenberg);
        var first = provider.GetRequiredService<Client>();
        var second = provider.GetRequiredService<Client>();
        var rotate = PdfEngineBuilders.Rotate(90).WithPdfs(a => a.AddItem("in.pdf", Pdf));

        (await second.SupportsAsync(rotate.Build())).Should().BeFalse();

        gotenberg.Version = "8.30.0";
        gotenberg.ResponseVersionHeader = "8.30.0";
        await first.HtmlToPdfAsync(Html());

        (await second.SupportsAsync(rotate.Build())).Should().BeTrue();
        await second.EnsureSupportedAsync(rotate.Build());
        await second.ExecutePdfEngineAsync(rotate);
        gotenberg.VersionRequests.Should().Be(1, "the header replaces the cached version without another lookup");
    }

    [Test]
    public async Task InPlaceRollback_IsSeenByAClientSharingTheCache()
    {
        // The service goes back from 8.30.0 to 8.20.0: the rotate route is gone, and the next request
        // is refused with the reason instead of answering with a bare 404.
        var gotenberg = new FakeGotenberg("8.30.0");
        using var provider = SharedCacheProvider(gotenberg);
        var first = provider.GetRequiredService<Client>();
        var second = provider.GetRequiredService<Client>();
        var rotate = PdfEngineBuilders.Rotate(90).WithPdfs(a => a.AddItem("in.pdf", Pdf));

        (await second.SupportsAsync(rotate.Build())).Should().BeTrue();

        gotenberg.ResponseVersionHeader = "8.20.0";
        await first.HtmlToPdfAsync(Html());

        (await second.SupportsAsync(rotate.Build())).Should().BeFalse();

        var act = async () => await second.ExecutePdfEngineAsync(rotate);
        await act.Should().ThrowExactlyAsync<GotenbergVersionNotSupportedException>();
        gotenberg.VersionRequests.Should().Be(1);
    }

    private static ServiceProvider SharedCacheProvider(FakeGotenberg gotenberg)
    {
        var services = new ServiceCollection();
        services.AddOptions<GotenbergSharpClientOptions>().Configure(o => o.ServiceUrl = Address);
        services.AddGotenbergSharpClient().ConfigurePrimaryHttpMessageHandler(() => gotenberg);

        return services.BuildServiceProvider();
    }

    [Test]
    public async Task ResponseWithAVersion_ReplacesAnUnknownOne()
    {
        var gotenberg = new FakeGotenberg((string?)null) { ResponseVersionHeader = "8.31.0" };
        var client = DirectClient(gotenberg);

        (await client.GetGotenbergVersionAsync()).IsKnown.Should().BeFalse();

        await client.HtmlToPdfAsync(Html());

        (await client.GetGotenbergVersionAsync()).Should().Be(GotenbergVersion.Parse("8.31.0"));
    }

    [Test]
    public async Task ResponseHeader_DoesNotStandInForALookupNotYetMade()
    {
        var gotenberg = new FakeGotenberg("8.30.0") { ResponseVersionHeader = "9.1.0" };
        var client = DirectClient(gotenberg);
        client.ApiVersion = GotenbergApiVersion.V8;

        await client.HtmlToPdfAsync(Html());

        (await client.GetGotenbergVersionAsync()).Should().Be(GotenbergVersion.Parse("8.30.0"));
        gotenberg.VersionRequests.Should().Be(1);
    }

    #endregion

    #region Subclasses written before rendering per API

    [Test]
    public void LegacySubclass_RendersForV8AsBefore()
    {
        var rendered = WithPdf(new LegacyRequest()).RenderFor(GotenbergApiVersion.V8);

        // Its own part first, then everything PdfEngineRequest sent through base.ToHttpContent().
        rendered.PartNames.Should().Equal("custom", "files", "pdfa");
        rendered.Path.Should().Be("forms/custom/legacy");
        rendered.Should().BeEquivalentTo(WithPdf(new LegacyRequest()).Render());
    }

    [Test]
    public void LegacySubclassThatReplacesTheContent_RendersOnlyItsOwnParts()
    {
        WithPdf(new ReplacingLegacyRequest()).RenderFor(GotenbergApiVersion.V8)
            .PartNames.Should().Equal("only");
    }

    [Test]
    public void LegacySubclass_CannotBeRenderedForV9()
    {
        var act = () => WithPdf(new LegacyRequest()).RenderFor(GotenbergApiVersion.V9);

        act.Should().ThrowExactly<InvalidOperationException>()
            .WithMessage($"*{typeof(LegacyRequest).FullName}*ToHttpContent(GotenbergApiVersion api)*");
    }

    [Test]
    public async Task LegacySubclass_IsNotSentToGotenberg9()
    {
        var gotenberg = new FakeGotenberg("9.5.1");
        var client = DirectClient(gotenberg);

        var act = async () => await client.ExecutePdfEngineAsync(WithPdf(new LegacyRequest()));

        await act.Should().ThrowExactlyAsync<InvalidOperationException>();
        gotenberg.ConversionRequests.Should().Be(0);
    }

    [TestCase(GotenbergApiVersion.V8)]
    [TestCase(GotenbergApiVersion.V9)]
    public void MigratedSubclass_RendersForEitherApi(GotenbergApiVersion api)
    {
        WithPdf(new MigratedRequest()).RenderFor(api)
            .PartNames.Should().Equal("files", PostProcessingField(api), "custom");
    }

    [TestCase(GotenbergApiVersion.V8)]
    [TestCase(GotenbergApiVersion.V9)]
    public void SubclassOverridingBoth_RendersTheVersionedContent(GotenbergApiVersion api)
    {
        WithPdf(new BothOverridesRequest()).RenderFor(api)
            .PartNames.Should().Equal("files", PostProcessingField(api), "versioned");
    }

    // The PDF/A format WithPdf sets: a flat field on 8, a stage on 9.
    private static string PostProcessingField(GotenbergApiVersion api) => api == GotenbergApiVersion.V9 ? "stages" : "pdfa";

    private static T WithPdf<T>(T request) where T : BuildRequestBase
    {
        request.Assets = new AssetDictionary { { "in.pdf", new ContentItem(Pdf) } };
        request.PdfOutputOptions = new() { PdfFormat = PdfFormat.A2b };

        return request;
    }

    private static StringContent Field(string name, string value)
    {
        var content = new StringContent(value);
        content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data") { Name = name };

        return content;
    }

    /// <summary>
    /// A custom route written against the parameterless method only, as docs/advanced-features.md shows.
    /// </summary>
    private sealed class LegacyRequest : PdfEngineRequest
    {
        protected override string ApiPath => "forms/custom/legacy";

        protected override IEnumerable<HttpContent> ToHttpContent()
        {
            yield return Field("custom", "value");

            foreach (var content in base.ToHttpContent()) yield return content;
        }
    }

    private sealed class ReplacingLegacyRequest : PdfEngineRequest
    {
        protected override string ApiPath => "forms/custom/replacing";

        protected override IEnumerable<HttpContent> ToHttpContent() => [Field("only", "value")];
    }

    /// <summary>
    /// A route of Gotenberg 9 or newer: the floors of the client's own routes are all 8.x releases.
    /// </summary>
    [MinimumGotenbergVersion("9.0.0", Feature = "A route of Gotenberg 9")]
    private sealed class NewerThanEightRequest : PdfEngineRequest
    {
        protected override string ApiPath => "forms/custom/newer";
    }

    private sealed class MigratedRequest : PdfEngineRequest
    {
        protected override string ApiPath => "forms/custom/migrated";

        protected override IEnumerable<HttpContent> ToHttpContent(GotenbergApiVersion api) =>
            base.ToHttpContent(api).Append(Field("custom", "value"));
    }

    private sealed class BothOverridesRequest : PdfEngineRequest
    {
        protected override string ApiPath => "forms/custom/both";

        protected override IEnumerable<HttpContent> ToHttpContent() => [Field("legacy", "value")];

        protected override IEnumerable<HttpContent> ToHttpContent(GotenbergApiVersion api) =>
            base.ToHttpContent(api).Append(Field("versioned", "value"));
    }

    #endregion

    #region Integration

    [GotenbergTest]
    public async Task GetApiVersionAsync_IsTheApiOfTheServersMajor(GotenbergServer gotenberg)
    {
        var client = await gotenberg.CreateClientAsync();

        var expected = gotenberg.Version.Major >= 9 ? GotenbergApiVersion.V9 : GotenbergApiVersion.V8;

        (await client.GetApiVersionAsync()).Should().Be(expected);
    }

    #endregion

    /// <summary>
    /// Answers <c>/version</c> with a fixed version (404 when null) and every other route with a
    /// small PDF, counting both, and optionally adds a <c>Gotenberg-Version</c> header to every answer.
    /// </summary>
    private sealed class FakeGotenberg(Func<HttpRequestMessage, string?> version) : HttpMessageHandler
    {
        private readonly ConcurrentQueue<string> _paths = new();

        public FakeGotenberg(string? version) : this(_ => version)
        {
            this.Version = version;
        }

        /// <summary>
        /// What <c>/version</c> answers when the fake was built with a fixed version; set it to move the service.
        /// </summary>
        public string? Version { get; set; }

        public TimeSpan VersionDelay { get; init; }

        public HttpStatusCode? VersionFailure { get; set; }

        public string VersionFailureBody { get; init; } = "failed";

        public Exception? VersionException { get; init; }

        public string? ResponseTrace { get; init; }

        public HttpStatusCode? ConversionFailure { get; set; }

        public string? ResponseVersionHeader { get; set; }

        public int VersionRequests => this._paths.Count(IsVersion);

        public int ConversionRequests => this._paths.Count(p => !IsVersion(p));

        private static bool IsVersion(string path) => path == "/version";

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var path = request.RequestUri!.AbsolutePath;
            this._paths.Enqueue(path);

            HttpResponseMessage response;

            if (IsVersion(path))
            {
                await Task.Delay(this.VersionDelay, cancellationToken);

                if (this.VersionException != null) throw this.VersionException;

                var reported = this.Version ?? version(request);

                response = this.VersionFailure is { } failure
                    ? new HttpResponseMessage(failure) { Content = new StringContent(this.VersionFailureBody) }
                    : reported == null
                        ? new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("Not Found") }
                        : new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(reported) };
            }
            else
            {
                if (request.Content != null) await request.Content.ReadAsByteArrayAsync(cancellationToken);

                response = this.ConversionFailure is { } failure
                    ? new HttpResponseMessage(failure) { Content = new StringContent("rejected") }
                    : new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(Pdf) };
            }

            if (this.ResponseVersionHeader != null)
                response.Headers.Add(Constants.Gotenberg.All.VersionHeader, this.ResponseVersionHeader);

            if (this.ResponseTrace != null)
                response.Headers.Add(Constants.Gotenberg.All.Trace, this.ResponseTrace);

            response.RequestMessage = request;

            return response;
        }
    }
}

/// <summary>
/// The form of <see cref="GotenbergVersionNotSupportedException" /> that says a newer major removed a feature.
/// </summary>
[TestFixture]
public class GotenbergVersionNotSupportedExceptionTests
{
    [Test]
    public void Removed_NamesTheReleaseTheRunningVersionAndTheReplacement()
    {
        var exception = GotenbergVersionNotSupportedException.Removed(
            "Watermarks drawn behind the page",
            GotenbergVersion.Parse("9.0.0"),
            GotenbergVersion.Parse("9.5.1"),
            "Use a stamp instead.");

        exception.Message.Should().Be(
            "Watermarks drawn behind the page is not available on Gotenberg 9.0.0 or newer; the service at this address reports 9.5.1. Use a stamp instead.");
        exception.Feature.Should().Be("Watermarks drawn behind the page");
        exception.RemovedInVersion.Should().Be(GotenbergVersion.Parse("9.0.0"));
        exception.RunningVersion.Should().Be(GotenbergVersion.Parse("9.5.1"));
        exception.Replacement.Should().Be("Use a stamp instead.");
        exception.Requirement.Should().BeNull();
        exception.RequiredVersion.Should().BeNull();
    }

    [Test]
    public void Removed_WithoutAKnownRunningVersion_PointsAtTheApiSetting()
    {
        // ApiVersion = V9 renders for 9 without reading the version of the service.
        var exception = GotenbergVersionNotSupportedException.Removed(
            "Watermarks drawn behind the page",
            GotenbergVersion.Parse("9.0.0"),
            GotenbergVersion.Unknown);

        exception.Message.Should().Be(
            "Watermarks drawn behind the page is not available on Gotenberg 9.0.0 or newer, the API GotenbergSharpClient.ApiVersion is set to.");
        exception.Replacement.Should().BeNull();
    }

    [Test]
    public void TooOld_KeepsItsRequirementAndHasNoRemoval()
    {
        var requirement = new ReadBookmarksRequest().Requires!;

        var exception = GotenbergVersionNotSupportedException.Create(requirement, GotenbergVersion.Parse("8.27.0"));

        exception.Feature.Should().Be("Reading PDF bookmarks");
        exception.Requirement.Should().BeSameAs(requirement);
        exception.RequiredVersion.Should().Be(GotenbergVersion.Parse("8.28.0"));
        exception.RemovedInVersion.Should().BeNull();
        exception.Replacement.Should().BeNull();
    }
}
