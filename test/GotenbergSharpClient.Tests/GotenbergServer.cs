using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

using Gotenberg.Sharp.API.Client.Domain.Settings;
using Gotenberg.Sharp.API.Client.Extensions;

using Microsoft.Extensions.DependencyInjection;

namespace GotenbergSharpClient.Tests;

/// <summary>
/// A Gotenberg the integration tests run against: a <c>gotenberg/gotenberg</c> container that
/// Testcontainers starts on first use, or a server that is already running.
/// </summary>
public sealed class GotenbergServer : IAsyncDisposable
{
    public const string Username = "testuser";
    public const string Password = "testpass";

    /// <summary>
    /// Basic auth arrived in Gotenberg 8.4.0; containers of older versions run without it.
    /// </summary>
    public const string BasicAuthVersion = "8.4.0";

    private const int Port = 3000;

    private readonly Lazy<Task<Uri>> _url;
    private IContainer? _container;

    private GotenbergServer(string name, Version version, Func<GotenbergServer, Task<Uri>> start)
    {
        Name = name;
        Version = version;
        _url = new Lazy<Task<Uri>>(() => start(this));
    }

    public string Name { get; }

    /// <summary>
    /// The version the server is known to have, for <see cref="GotenbergTestAttribute"/> floors.
    /// Parts a tag leaves out count as the highest: <c>8</c> is the newest 8.x release.
    /// </summary>
    public Version Version { get; }

    public bool UsesBasicAuth => Version >= Version.Parse(BasicAuthVersion);

    /// <summary>
    /// The <c>gotenberg/gotenberg:&lt;tag&gt;</c> image, for example <c>8</c> or <c>8.3.0</c>.
    /// </summary>
    public static GotenbergServer FromImageTag(string tag) =>
        new($"gotenberg:{tag}", ParseTag(tag), server => server.StartContainerAsync($"gotenberg/gotenberg:{tag}"));

    /// <summary>
    /// A running server set up like <c>docker/docker-compose-basic-auth.yml</c>, assumed to be current.
    /// </summary>
    public static GotenbergServer FromUrl(Uri url) =>
        new(url.ToString(), new Version(int.MaxValue, 0), _ => Task.FromResult(url));

    public Task<Uri> GetUrlAsync() => _url.Value;

    public async Task<Gotenberg.Sharp.API.Client.GotenbergSharpClient> CreateClientAsync()
    {
        var url = await GetUrlAsync();

        var services = new ServiceCollection();
        services.AddOptions<GotenbergSharpClientOptions>()
            .Configure(options =>
            {
                options.ServiceUrl = url;

                if (UsesBasicAuth)
                {
                    options.BasicAuthUsername = Username;
                    options.BasicAuthPassword = Password;
                }
            });
        services.AddGotenbergSharpClient();

        return services.BuildServiceProvider()
            .GetRequiredService<Gotenberg.Sharp.API.Client.GotenbergSharpClient>();
    }

    public async ValueTask DisposeAsync()
    {
        if (_container != null) await _container.DisposeAsync();
    }

    public override string ToString() => Name;

    private async Task<Uri> StartContainerAsync(string image)
    {
        var builder = new ContainerBuilder(image)
            .WithPortBinding(Port, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(
                request => request.ForPort(Port).ForPath("/health"),
                wait => wait.WithTimeout(TimeSpan.FromMinutes(2))));

        if (UsesBasicAuth)
        {
            builder = builder
                .WithEnvironment("GOTENBERG_API_BASIC_AUTH_USERNAME", Username)
                .WithEnvironment("GOTENBERG_API_BASIC_AUTH_PASSWORD", Password)
                .WithCommand("gotenberg", "--api-enable-basic-auth");
        }

        _container = builder.Build();
        await _container.StartAsync();

        return new UriBuilder(Uri.UriSchemeHttp, _container.Hostname, _container.GetMappedPublicPort(Port)).Uri;
    }

    private static Version ParseTag(string tag)
    {
        // "8-chromium" is an 8 image; "latest" has no number and counts as the newest.
        var parts = tag.Split('-')[0].Split('.');
        var numbers = new int[3];

        for (var i = 0; i < numbers.Length; i++)
        {
            numbers[i] = i < parts.Length && int.TryParse(parts[i], out var number) ? number : int.MaxValue;
        }

        return new Version(numbers[0], numbers[1], numbers[2]);
    }
}
