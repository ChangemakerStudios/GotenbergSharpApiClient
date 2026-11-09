namespace GotenbergSharpClient.Tests;

/// <summary>
/// The Gotenberg servers that <see cref="GotenbergTestAttribute"/> tests run against, and the
/// teardown that stops their containers once every test has run.
/// </summary>
/// <remarks>
/// By default a test runs against the newest image of each Gotenberg major the client supports.
/// <c>GOTENBERG_VERSIONS</c> picks other image tags, comma-separated (<c>8.3.0,9</c>).
/// <c>GOTENBERG_URL</c> runs the tests once against a server that is already running instead.
/// </remarks>
[SetUpFixture]
public sealed class GotenbergServers
{
    // Add "9" once the client supports Gotenberg 9.
    private const string SupportedVersions = "7,8";

    public static IReadOnlyList<GotenbergServer> All { get; } = Create();

    public static IEnumerable<TestCaseData> TestCases(string? since)
    {
        var floor = since == null ? null : Version.Parse(since);

        foreach (var server in All)
        {
            var testCase = new TestCaseData(server)
                .SetArgDisplayNames(server.Name)
                .SetCategory("Integration");

            if (floor != null && server.Version < floor) testCase.Ignore($"Needs Gotenberg {since} or later");

            yield return testCase;
        }
    }

    [OneTimeTearDown]
    public async Task StopContainers()
    {
        foreach (var server in All) await server.DisposeAsync();
    }

    private static GotenbergServer[] Create()
    {
        var url = Environment.GetEnvironmentVariable("GOTENBERG_URL");

        if (!string.IsNullOrWhiteSpace(url)) return [GotenbergServer.FromUrl(new Uri(url))];

        var versions = Environment.GetEnvironmentVariable("GOTENBERG_VERSIONS");

        return (string.IsNullOrWhiteSpace(versions) ? SupportedVersions : versions)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(GotenbergServer.FromImageTag)
            .ToArray();
    }
}
