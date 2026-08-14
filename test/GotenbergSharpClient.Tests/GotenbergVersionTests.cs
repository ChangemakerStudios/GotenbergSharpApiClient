using Gotenberg.Sharp.API.Client.Application.Builders;
using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Shared;
using Gotenberg.Sharp.API.Client.Infrastructure;

namespace GotenbergSharpClient.Tests;

[TestFixture]
public class GotenbergVersionTests
{
    #region Parsing

    [TestCase("8.28.0", 8, 28, 0)]
    [TestCase("v8.28.0", 8, 28, 0)]
    [TestCase(" 8.28.1\n", 8, 28, 1)]
    [TestCase("8.17", 8, 17, 0)]
    [TestCase("9", 9, 0, 0)]
    public void TryParse_ParsesNumericComponents(string value, int major, int minor, int patch)
    {
        GotenbergVersion.TryParse(value, out var version).Should().BeTrue();

        version!.Major.Should().Be(major);
        version.Minor.Should().Be(minor);
        version.Patch.Should().Be(patch);
        version.IsKnown.Should().BeTrue();
    }

    [Test]
    public void TryParse_KeepsSuffixButIgnoresItForOrdering()
    {
        // The Gotenberg live demo reports this exact shape.
        GotenbergVersion.TryParse("8.17.0-live-demo-snapshot", out var version).Should().BeTrue();

        version!.Suffix.Should().Be("live-demo-snapshot");
        version.Raw.Should().Be("8.17.0-live-demo-snapshot");
        version.Should().Be(GotenbergVersion.Parse("8.17.0"));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    [TestCase("snapshot")]
    public void TryParse_RejectsUnusableValues(string? value)
    {
        GotenbergVersion.TryParse(value, out var version).Should().BeFalse();

        version.Should().BeNull();
    }

    [Test]
    public void Parse_WithUnusableValue_Throws()
    {
        var act = () => GotenbergVersion.Parse("not-a-version");

        act.Should().ThrowExactly<FormatException>();
    }

    #endregion

    #region Comparison

    [Test]
    public void IsAtLeast_ComparesByPrecedence()
    {
        GotenbergVersion.Parse("8.28.0").IsAtLeast(GotenbergVersion.Parse("8.28.0")).Should().BeTrue();
        GotenbergVersion.Parse("8.35.1").IsAtLeast(GotenbergVersion.Parse("8.28.0")).Should().BeTrue();
        GotenbergVersion.Parse("9.0.0").IsAtLeast(GotenbergVersion.Parse("8.28.0")).Should().BeTrue();
        GotenbergVersion.Parse("8.27.9").IsAtLeast(GotenbergVersion.Parse("8.28.0")).Should().BeFalse();
        GotenbergVersion.Parse("7.10.0").IsAtLeast(GotenbergVersion.Parse("8.28.0")).Should().BeFalse();
    }

    [Test]
    public void Unknown_IsNeverConsideredNewEnough()
    {
        GotenbergVersion.Unknown.IsKnown.Should().BeFalse();
        GotenbergVersion.Unknown.IsAtLeast(GotenbergVersion.Parse("8.28.0")).Should().BeFalse();
        (GotenbergVersion.Unknown < GotenbergVersion.Parse("0.0.1")).Should().BeTrue();
    }

    #endregion

    #region Requirement Resolution

    [Test]
    public void BookmarkRequests_DeclareTheirMinimumVersion()
    {
        new ReadBookmarksRequest().Requires!.MinimumVersion.Should().Be(GotenbergVersion.Parse("8.28.0"));
        new WriteBookmarksRequest().Requires!.MinimumVersion.Should().Be(GotenbergVersion.Parse("8.28.0"));
        new ReadBookmarksRequest().Requires!.Feature.Should().Be("Reading PDF bookmarks");
    }

    [Test]
    public void RequestsWithoutAMinimum_DeclareNoRequirement()
    {
        new FlattenPdfRequest().Requires.Should().BeNull();
        new ReadMetadataRequest().Requires.Should().BeNull();
    }

    [Test]
    public void Requirement_FlowsOntoTheApiRequest()
    {
        var request = PdfEngineBuilders.ReadBookmarks()
            .WithPdfs(a => a.AddItem("test.pdf", new byte[] { 1, 2, 3 }))
            .Build();

        request.CreateApiRequest().Requires!.MinimumVersion.Should().Be(GotenbergVersion.Parse("8.28.0"));
    }

    #endregion

    #region Client Enforcement

    [Test]
    public async Task Client_WithOlderService_ThrowsBeforeSending()
    {
        var client = new StubVersionClient("8.27.0");

        var builder = PdfEngineBuilders.WriteBookmarks(b => b.Add("Intro", 1))
            .WithPdfs(a => a.AddItem("test.pdf", new byte[] { 1, 2, 3 }));

        var act = async () => await client.ExecutePdfEngineAsync(builder);

        var thrown = await act.Should().ThrowExactlyAsync<GotenbergVersionNotSupportedException>();

        thrown.Which.RunningVersion.Should().Be(GotenbergVersion.Parse("8.27.0"));
        thrown.Which.RequiredVersion.Should().Be(GotenbergVersion.Parse("8.28.0"));
        thrown.Which.Message.Should().Contain("Writing PDF bookmarks").And.Contain("8.27.0");

        client.SendCount.Should().Be(0, "the request must never reach the wire");
    }

    [Test]
    public async Task Client_WithUndeterminableVersion_ThrowsAndSaysSo()
    {
        // Releases predating the /version route report nothing at all.
        var client = new StubVersionClient(null);

        var builder = PdfEngineBuilders.ReadBookmarks()
            .WithPdfs(a => a.AddItem("test.pdf", new byte[] { 1, 2, 3 }));

        var act = async () => await client.ReadPdfBookmarksAsync(builder);

        var thrown = await act.Should().ThrowExactlyAsync<GotenbergVersionNotSupportedException>();

        thrown.Which.RunningVersion.IsKnown.Should().BeFalse();
        thrown.Which.Message.Should().Contain("could not be determined");
    }

    [Test]
    public async Task Client_WithNewEnoughService_PassesTheCheck()
    {
        var client = new StubVersionClient("8.35.0");

        var request = PdfEngineBuilders.WriteBookmarks(b => b.Add("Intro", 1))
            .WithPdfs(a => a.AddItem("test.pdf", new byte[] { 1, 2, 3 }))
            .Build();

        (await client.SupportsAsync(request)).Should().BeTrue();

        var act = async () => await client.EnsureSupportedAsync(request);

        await act.Should().NotThrowAsync();
    }

    [Test]
    public async Task Client_CachesTheVersionLookup()
    {
        var client = new StubVersionClient("8.35.0");

        await client.GetGotenbergVersionAsync();
        await client.GetGotenbergVersionAsync();
        await client.GetGotenbergVersionAsync();

        client.VersionCallCount.Should().Be(1);
    }

    [Test]
    public async Task Client_WithEnforcementDisabled_SkipsTheLookupEntirely()
    {
        var client = new StubVersionClient("8.1.0") { EnforceMinimumVersion = false };

        var builder = PdfEngineBuilders.ReadBookmarks()
            .WithPdfs(a => a.AddItem("test.pdf", new byte[] { 1, 2, 3 }));

        // Reaches the (stubbed) send instead of being rejected up front.
        var act = async () => await client.ReadPdfBookmarksAsync(builder);

        await act.Should().ThrowExactlyAsync<StubVersionClient.SendReachedException>();

        client.VersionCallCount.Should().Be(0);
    }

    [Test]
    public async Task Client_DoesNotCheckRequestsWithoutAMinimum()
    {
        var client = new StubVersionClient("8.1.0");

        var builder = PdfEngineBuilders.Flatten()
            .WithPdfs(a => a.AddItem("test.pdf", new byte[] { 1, 2, 3 }));

        var act = async () => await client.ExecutePdfEngineAsync(builder);

        await act.Should().ThrowExactlyAsync<StubVersionClient.SendReachedException>();

        client.VersionCallCount.Should().Be(0);
    }

    #endregion

    /// <summary>
    /// Reports a fixed version and fails loudly if a request makes it to the wire, so the tests can
    /// tell "rejected up front" apart from "sent anyway". Only the version lookup is stubbed at the
    /// client level — everything else runs the real send path down to the message handler.
    /// </summary>
    private sealed class StubVersionClient : Gotenberg.Sharp.API.Client.GotenbergSharpClient
    {
        private readonly ThrowingHandler _handler;

        private readonly string? _version;

        private StubVersionClient(ThrowingHandler handler, string? version)
            : base(new HttpClient(handler) { BaseAddress = new Uri("http://gotenberg.invalid:3000") })
        {
            this._handler = handler;
            this._version = version;
        }

        public StubVersionClient(string? version) : this(new ThrowingHandler(), version)
        {
        }

        public int VersionCallCount { get; private set; }

        public int SendCount => this._handler.SendCount;

        public override Task<string?> GetVersion(CancellationToken token = default)
        {
            this.VersionCallCount++;

            return Task.FromResult(this._version);
        }

        private sealed class ThrowingHandler : HttpMessageHandler
        {
            public int SendCount { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                this.SendCount++;

                throw new SendReachedException();
            }
        }

        internal sealed class SendReachedException : Exception;
    }
}
