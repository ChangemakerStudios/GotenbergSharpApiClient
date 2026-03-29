using Gotenberg.Sharp.API.Client.Domain.Builders;
using Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;
using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;
using Gotenberg.Sharp.API.Client.Domain.Settings;
using Gotenberg.Sharp.API.Client.Domain.ValueObjects;
using Gotenberg.Sharp.API.Client.Extensions;
using Gotenberg.Sharp.API.Client.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace GotenbergSharpClient.Tests;

[TestFixture]
public class EncryptionOptionsTests
{
    #region PdfPassword Value Object Tests

    [Test]
    public void PdfPassword_Create_WithValidPassword_ReturnsInstance()
    {
        var password = PdfPassword.Create("secret123");

        password.Value.Should().Be("secret123");
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void PdfPassword_Create_WithNullOrEmpty_ThrowsArgumentException(string? input)
    {
        var act = () => PdfPassword.Create(input!);

        act.Should().ThrowExactly<ArgumentException>();
    }

    [Test]
    public void PdfPassword_Equality_WithSameValue_ReturnsTrue()
    {
        var a = PdfPassword.Create("secret");
        var b = PdfPassword.Create("secret");

        (a == b).Should().BeTrue();
        a.Equals(b).Should().BeTrue();
    }

    [Test]
    public void PdfPassword_ToString_ReturnsRedactedValue()
    {
        var password = PdfPassword.Create("secret");

        password.ToString().Should().Be("****");
    }

    #endregion

    #region Builder Tests

    [Test]
    public void SetUserPassword_WithString_SetsProperty()
    {
        var builder = new HtmlRequestBuilder();

        builder.SetPdfOutputOptions(o => o.SetUserPassword("openme"));
        var request = builder.Build();

        request.PdfOutputOptions!.UserPassword.Should().NotBeNull();
        request.PdfOutputOptions.UserPassword!.Value.Should().Be("openme");
    }

    [Test]
    public void SetOwnerPassword_WithString_SetsProperty()
    {
        var builder = new HtmlRequestBuilder();

        builder.SetPdfOutputOptions(o => o.SetOwnerPassword("editme"));
        var request = builder.Build();

        request.PdfOutputOptions!.OwnerPassword.Should().NotBeNull();
        request.PdfOutputOptions.OwnerPassword!.Value.Should().Be("editme");
    }

    [Test]
    public void SetEncryption_SetsBothPasswords()
    {
        var builder = new HtmlRequestBuilder();

        builder.SetPdfOutputOptions(o => o.SetEncryption("user123", "owner456"));
        var request = builder.Build();

        request.PdfOutputOptions!.UserPassword!.Value.Should().Be("user123");
        request.PdfOutputOptions.OwnerPassword!.Value.Should().Be("owner456");
    }

    [Test]
    public void SetUserPassword_WithEmptyString_ThrowsArgumentException()
    {
        var builder = new HtmlRequestBuilder();

        var act = () => builder.SetPdfOutputOptions(o => o.SetUserPassword(""));

        act.Should().ThrowExactly<ArgumentException>();
    }

    #endregion

    #region HTTP Content Serialization Tests

    [Test]
    public async Task UserPassword_SerializesToCorrectHttpContent()
    {
        var options = new PdfOutputOptions
        {
            UserPassword = PdfPassword.Create("openme")
        };

        var httpContents = options.ToHttpContent().ToList();
        var content = httpContents.FirstOrDefault(c =>
            c.Headers.ContentDisposition?.Name == Constants.Gotenberg.PdfOutput.UserPassword);

        content.Should().NotBeNull();
        (await content!.ReadAsStringAsync()).Should().Be("openme");
    }

    [Test]
    public async Task OwnerPassword_SerializesToCorrectHttpContent()
    {
        var options = new PdfOutputOptions
        {
            OwnerPassword = PdfPassword.Create("editme")
        };

        var httpContents = options.ToHttpContent().ToList();
        var content = httpContents.FirstOrDefault(c =>
            c.Headers.ContentDisposition?.Name == Constants.Gotenberg.PdfOutput.OwnerPassword);

        content.Should().NotBeNull();
        (await content!.ReadAsStringAsync()).Should().Be("editme");
    }

    [Test]
    public void NullPasswords_NotIncludedInHttpContent()
    {
        var options = new PdfOutputOptions();

        var httpContents = options.ToHttpContent().ToList();

        httpContents.FirstOrDefault(c =>
            c.Headers.ContentDisposition?.Name == Constants.Gotenberg.PdfOutput.UserPassword).Should().BeNull();
        httpContents.FirstOrDefault(c =>
            c.Headers.ContentDisposition?.Name == Constants.Gotenberg.PdfOutput.OwnerPassword).Should().BeNull();
    }

    #endregion

    #region Integration Tests

    [Category("Integration")]
    [Test]
    public async Task HtmlToPdf_WithEncryption_Succeeds()
    {
        var services = new ServiceCollection();
        services.AddOptions<GotenbergSharpClientOptions>()
            .Configure(options =>
            {
                options.ServiceUrl = new Uri("http://localhost:3000");
                options.BasicAuthUsername = "testuser";
                options.BasicAuthPassword = "testpass";
            });
        services.AddGotenbergSharpClient();
        var client = services.BuildServiceProvider()
            .GetRequiredService<Gotenberg.Sharp.API.Client.GotenbergSharpClient>();

        var builder = new HtmlRequestBuilder()
            .AddDocument(doc => doc.SetBody(
                "<html><body><h1>Encrypted PDF</h1></body></html>"))
            .SetPdfOutputOptions(o => o.SetEncryption("user123", "owner456"));

        var result = await client.HtmlToPdfAsync(builder);

        result.Should().NotBeNull();
        result.Length.Should().BeGreaterThan(0);
    }

    #endregion
}
