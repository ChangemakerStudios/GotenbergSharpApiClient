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
}
