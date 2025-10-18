using AwesomeAssertions;
using Gotenberg.Sharp.API.Client.Domain.Builders;
using Gotenberg.Sharp.API.Client.Domain.Settings;
using Gotenberg.Sharp.API.Client.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace GotenbergSharpClient.Tests;

[TestFixture]
public class BasicAuthTests
{
    private const string GotenbergUrl = "http://localhost:3000";
    private const string TestUsername = "testuser";
    private const string TestPassword = "testpass";

    [Test]
    public async Task Client_WithBasicAuth_ShouldIncludeAuthorizationHeader()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddOptions<GotenbergSharpClientOptions>()
            .Configure(options =>
            {
                options.ServiceUrl = new Uri(GotenbergUrl);
                options.BasicAuthUsername = TestUsername;
                options.BasicAuthPassword = TestPassword;
            });

        services.AddGotenbergSharpClient();

        var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetRequiredService<Gotenberg.Sharp.API.Client.GotenbergSharpClient>();

        // Act - Create a simple HTML to PDF request
        var builder = new HtmlRequestBuilder()
            .AddDocument(doc => doc.SetBody("<html><body><h1>Basic Auth Test</h1></body></html>"));

        // Assert - This will fail if auth is not properly configured or Gotenberg rejects the request
        var result = await client.HtmlToPdfAsync(builder);

        result.Should().NotBeNull("Basic auth should be properly configured and accepted by Gotenberg");
        result.Length.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task Client_WithoutBasicAuth_ShouldFailWhenGotenbergRequiresAuth()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddOptions<GotenbergSharpClientOptions>()
            .Configure(options =>
            {
                options.ServiceUrl = new Uri(GotenbergUrl);
                // Deliberately not setting BasicAuthUsername or BasicAuthPassword
            });

        services.AddGotenbergSharpClient();

        var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetRequiredService<Gotenberg.Sharp.API.Client.GotenbergSharpClient>();

        // Act
        var builder = new HtmlRequestBuilder()
            .AddDocument(doc => doc.SetBody("<html><body><h1>No Auth Test</h1></body></html>"));

        // Assert - Should fail with 401 Unauthorized when Gotenberg requires auth
        var act = () => client.HtmlToPdfAsync(builder);

        await act.Should().ThrowAsync<Exception>("Gotenberg should reject requests without proper authentication");
    }

    [Test]
    public async Task Client_WithInvalidCredentials_ShouldFailWithUnauthorized()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddOptions<GotenbergSharpClientOptions>()
            .Configure(options =>
            {
                options.ServiceUrl = new Uri(GotenbergUrl);
                options.BasicAuthUsername = "wronguser";
                options.BasicAuthPassword = "wrongpassword";
            });

        services.AddGotenbergSharpClient();

        var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetRequiredService<Gotenberg.Sharp.API.Client.GotenbergSharpClient>();

        // Act
        var builder = new HtmlRequestBuilder()
            .AddDocument(doc => doc.SetBody("<html><body><h1>Invalid Auth Test</h1></body></html>"));

        // Assert - Should fail with 401 Unauthorized
        var act = () => client.HtmlToPdfAsync(builder);

        await act.Should().ThrowAsync<Exception>("Invalid credentials should be rejected");
    }

    [Test]
    public void GotenbergSharpClientOptions_BasicAuthProperties_ShouldBeNullableAndOptional()
    {
        // Arrange & Act
        var options = new GotenbergSharpClientOptions();

        // Assert
        options.BasicAuthUsername.Should().BeNull("BasicAuthUsername should be nullable and default to null");
        options.BasicAuthPassword.Should().BeNull("BasicAuthPassword should be nullable and default to null");
    }

    [Test]
    public void GotenbergSharpClientOptions_WithBasicAuthSet_ShouldRetainValues()
    {
        // Arrange
        var options = new GotenbergSharpClientOptions
        {
            BasicAuthUsername = TestUsername,
            BasicAuthPassword = TestPassword
        };

        // Assert
        options.BasicAuthUsername.Should().Be(TestUsername);
        options.BasicAuthPassword.Should().Be(TestPassword);
    }

    [Test]
    public void Client_WithHybridConfiguration_ShouldApplyProgrammaticOverrides()
    {
        // Arrange
        var services = new ServiceCollection();

        // Hybrid configuration: Start with binding from configuration (simulating appsettings.json)
        // then apply programmatic overrides via PostConfigure
        services.AddOptions<GotenbergSharpClientOptions>()
            .Configure(options =>
            {
                // Base configuration (simulating appsettings.json binding)
                options.ServiceUrl = new Uri(GotenbergUrl);
                options.TimeOut = TimeSpan.FromMinutes(3);
                // BasicAuth not set in "appsettings"
            })
            .PostConfigure(options =>
            {
                // Programmatic overrides applied via PostConfigure
                options.BasicAuthUsername = TestUsername;
                options.BasicAuthPassword = TestPassword;
                options.TimeOut = TimeSpan.FromMinutes(10);
            });

        services.AddGotenbergSharpClient();

        var serviceProvider = services.BuildServiceProvider();

        // Act - Verify options were properly configured with overrides
        var resolvedOptions = serviceProvider.GetRequiredService<IOptions<GotenbergSharpClientOptions>>().Value;

        // Assert - PostConfigure should have applied overrides after Configure
        resolvedOptions.BasicAuthUsername.Should().Be(TestUsername, "PostConfigure should override with username");
        resolvedOptions.BasicAuthPassword.Should().Be(TestPassword, "PostConfigure should override with password");
        resolvedOptions.TimeOut.Should().Be(TimeSpan.FromMinutes(10), "PostConfigure should override timeout");
        resolvedOptions.ServiceUrl.Should().Be(new Uri(GotenbergUrl), "Configure should set base values");
    }

    [Test]
    public async Task Client_WithHybridConfigurationAndRunningGotenberg_ShouldAuthenticate()
    {
        // This test requires a running Gotenberg instance with basic auth enabled
        // It will be skipped in CI/CD environments where Gotenberg isn't running

        // Arrange
        var services = new ServiceCollection();

        // Hybrid configuration: base configuration + programmatic overrides
        services.AddOptions<GotenbergSharpClientOptions>()
            .Configure(options =>
            {
                // Base configuration (simulating appsettings.json)
                options.ServiceUrl = new Uri(GotenbergUrl);
                options.TimeOut = TimeSpan.FromMinutes(3);
            })
            .PostConfigure(options =>
            {
                // Programmatic overrides for credentials
                options.BasicAuthUsername = TestUsername;
                options.BasicAuthPassword = TestPassword;
            });

        services.AddGotenbergSharpClient();

        var serviceProvider = services.BuildServiceProvider();
        var client = serviceProvider.GetRequiredService<Gotenberg.Sharp.API.Client.GotenbergSharpClient>();

        // Act - Create a simple HTML to PDF request
        var builder = new HtmlRequestBuilder()
            .AddDocument(doc => doc.SetBody("<html><body><h1>Hybrid Config Test</h1></body></html>"));

        // Assert - Should succeed with the programmatically configured auth
        var result = await client.HtmlToPdfAsync(builder);

        result.Should().NotBeNull("Hybrid configuration with basic auth should work properly");
        result.Length.Should().BeGreaterThan(0);
    }

    [Test]
    public void Client_WithHybridConfiguration_ShouldFailWithIncompleteAuth()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddOptions<GotenbergSharpClientOptions>()
            .Configure(options =>
            {
                options.ServiceUrl = new Uri(GotenbergUrl);
            })
            .PostConfigure(options =>
            {
                // Incomplete auth - only username provided
                options.BasicAuthUsername = TestUsername;
                // BasicAuthPassword deliberately omitted - should fail validation
            });

        services.AddGotenbergSharpClient();

        // Act & Assert - Building the client should throw when validation occurs
        var act = () =>
        {
            var serviceProvider = services.BuildServiceProvider();
            // Trigger the HttpMessageHandler factory which contains the validation
            var client = serviceProvider.GetRequiredService<Gotenberg.Sharp.API.Client.GotenbergSharpClient>();
        };

        act.Should().Throw<InvalidOperationException>("Incomplete basic auth configuration should fail validation")
            .WithMessage("*BasicAuth configuration is incomplete*");
    }

    [Test]
    public void Client_WithProgrammaticOnlyConfiguration_ShouldApplyBasicAuth()
    {
        // Arrange - Programmatic configuration only (no appsettings.json binding)
        var services = new ServiceCollection();

        services.AddOptions<GotenbergSharpClientOptions>()
            .Configure(options =>
            {
                // All configuration is programmatic
                options.ServiceUrl = new Uri(GotenbergUrl);
                options.BasicAuthUsername = TestUsername;
                options.BasicAuthPassword = TestPassword;
                options.TimeOut = TimeSpan.FromMinutes(5);
            });

        services.AddGotenbergSharpClient();

        var serviceProvider = services.BuildServiceProvider();

        // Act - Verify options were properly configured
        var resolvedOptions = serviceProvider.GetRequiredService<IOptions<GotenbergSharpClientOptions>>().Value;

        // Assert - All programmatic values should be applied
        resolvedOptions.BasicAuthUsername.Should().Be(TestUsername, "Programmatic-only config should set username");
        resolvedOptions.BasicAuthPassword.Should().Be(TestPassword, "Programmatic-only config should set password");
        resolvedOptions.TimeOut.Should().Be(TimeSpan.FromMinutes(5), "Programmatic-only config should set timeout");
        resolvedOptions.ServiceUrl.Should().Be(new Uri(GotenbergUrl), "Programmatic-only config should set service URL");
    }
}
