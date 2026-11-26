namespace GotenbergSharpClient.Tests;

/// <summary>
/// Runs an integration test once against each of <see cref="GotenbergServers.All"/>, passing the
/// server as the test's <see cref="GotenbergServer"/> argument, in the <c>Integration</c> category.
/// Servers older than <paramref name="since"/>, or at <paramref name="until"/> or newer, are reported as ignored.
/// </summary>
/// <param name="since">The first Gotenberg release that serves what the test calls.</param>
/// <param name="until">The first Gotenberg release that no longer serves it (exclusive ceiling).</param>
[AttributeUsage(AttributeTargets.Method)]
public sealed class GotenbergTestAttribute(string? since = null, string? until = null)
    : TestCaseSourceAttribute(typeof(GotenbergServers), nameof(GotenbergServers.TestCases), [since, until]);
