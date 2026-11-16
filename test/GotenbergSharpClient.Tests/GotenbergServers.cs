namespace GotenbergSharpClient.Tests;

/// <summary>
/// The Gotenberg servers that <see cref="GotenbergTestAttribute"/> tests run against, and the
/// teardown that stops their containers once every test has run.
/// </summary>
/// <remarks>
/// By default a test runs against the newest image of each Gotenberg major the client supports.
/// <c>GOTENBERG_VERSIONS</c> picks other image tags, comma-separated (<c>8.3.0,9</c>).
/// <c>GOTENBERG_URL</c> runs the tests once against a server that is already running instead,
/// with <c>GOTENBERG_VERSIONS</c> as its version when it is not the newest release.
/// </remarks>
[SetUpFixture]
public sealed class GotenbergServers
{
    private const string SupportedVersions = "7,8,9";

    public static IReadOnlyList<GotenbergServer> All { get; } = Create();

    public static IEnumerable<TestCaseData> TestCases(string? since, string? until = null)
    {
        foreach (var server in All)
        {
            var testCase = new TestCaseData(server)
                .SetArgDisplayNames(server.Name)
                .SetCategory("Integration");

            var reason = IgnoreReason(server.Version, since, until);
            if (reason != null) testCase.Ignore(reason);

            yield return testCase;
        }
    }

    /// <summary>
    /// Why a server of <paramref name="version"/> is outside <c>[since, until)</c>, or null when it is inside.
    /// </summary>
    public static string? IgnoreReason(Version version, string? since, string? until)
    {
        if (since != null && version < Version.Parse(since)) return $"Needs Gotenberg {since} or later";
        if (until != null && version >= Version.Parse(until)) return $"Removed in Gotenberg {until}";

        return null;
    }

    [OneTimeTearDown]
    public async Task StopContainers()
    {
        foreach (var server in All) await server.DisposeAsync();
    }

    private static GotenbergServer[] Create()
    {
        var url = Environment.GetEnvironmentVariable("GOTENBERG_URL");
        var versions = Environment.GetEnvironmentVariable("GOTENBERG_VERSIONS");

        if (!string.IsNullOrWhiteSpace(url))
        {
            if (versions?.Contains(',') == true)
                throw new InvalidOperationException(
                    "With GOTENBERG_URL, GOTENBERG_VERSIONS is the version of that one server; give a single version.");

            return [GotenbergServer.FromUrl(new Uri(url), versions)];
        }

        return (string.IsNullOrWhiteSpace(versions) ? SupportedVersions : versions)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(GotenbergServer.FromImageTag)
            .ToArray();
    }
}
