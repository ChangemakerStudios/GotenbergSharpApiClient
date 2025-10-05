using Gotenberg.Sharp.API.Client.Domain.Builders;
using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Requests.ApiRequests;
using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;
using Newtonsoft.Json.Linq;

namespace GotenbergSharpClient.Tests;

[TestFixture]
public class HtmlConversionBehaviorBuilderTests
{
    [Test]
    public void AddCookie_WithCookieObject_AddsCookieToCollection()
    {
        // Arrange
        var builder = new HtmlRequestBuilder();
        var cookie = new Cookie
        {
            Name = "test",
            Value = "value",
            Domain = "example.com"
        };

        // Act
        builder.SetConversionBehaviors(b => b.AddCookie(cookie));
        var request = builder.Build();

        // Assert
        request.ConversionBehaviors.Cookies.Should().NotBeNull();
        request.ConversionBehaviors.Cookies.Should().HaveCount(1);
        request.ConversionBehaviors.Cookies![0].Should().Be(cookie);
    }

    [Test]
    public void AddCookie_WithParameters_CreatesCookieCorrectly()
    {
        // Arrange
        var builder = new HtmlRequestBuilder();

        // Act
        builder.SetConversionBehaviors(b => b.AddCookie(
            name: "session",
            value: "abc123",
            domain: "example.com",
            path: "/api",
            secure: true,
            httpOnly: true,
            sameSite: "Strict"));

        var request = builder.Build();

        // Assert
        request.ConversionBehaviors.Cookies.Should().NotBeNull();
        request.ConversionBehaviors.Cookies.Should().HaveCount(1);
        var cookie = request.ConversionBehaviors.Cookies![0];
        cookie.Name.Should().Be("session");
        cookie.Value.Should().Be("abc123");
        cookie.Domain.Should().Be("example.com");
        cookie.Path.Should().Be("/api");
        cookie.Secure.Should().BeTrue();
        cookie.HttpOnly.Should().BeTrue();
        cookie.SameSite.Should().Be("Strict");
    }

    [Test]
    public void AddCookie_WithMinimalParameters_CreatesBasicCookie()
    {
        // Arrange
        var builder = new HtmlRequestBuilder();

        // Act
        builder.SetConversionBehaviors(b => b.AddCookie("name", "value", "domain.com"));
        var request = builder.Build();

        // Assert
        request.ConversionBehaviors.Cookies.Should().NotBeNull();
        request.ConversionBehaviors.Cookies.Should().HaveCount(1);
        var cookie = request.ConversionBehaviors.Cookies![0];
        cookie.Name.Should().Be("name");
        cookie.Value.Should().Be("value");
        cookie.Domain.Should().Be("domain.com");
        cookie.Path.Should().BeNull();
        cookie.Secure.Should().BeNull();
        cookie.HttpOnly.Should().BeNull();
        cookie.SameSite.Should().BeNull();
    }

    [Test]
    public void AddCookies_WithEnumerable_AddsAllCookies()
    {
        // Arrange
        var builder = new HtmlRequestBuilder();
        var cookies = new[]
        {
            new Cookie { Name = "cookie1", Value = "value1", Domain = "domain1.com" },
            new Cookie { Name = "cookie2", Value = "value2", Domain = "domain2.com" },
            new Cookie { Name = "cookie3", Value = "value3", Domain = "domain3.com" }
        };

        // Act
        builder
            .AddDocument(doc => doc.SetBody("<html><body>Test</body></html>"))
            .SetConversionBehaviors(b => b.AddCookies(cookies));
        var apiRequest = (IConvertToHttpContent)builder.Build().CreateApiRequest();

        // Assert - Test the internal HTTP content structure
        var httpContents = apiRequest.ToHttpContent().ToList();
        var cookieContent = httpContents.FirstOrDefault(c =>
            c.Headers.ContentDisposition?.Name == "cookies");

        cookieContent.Should().NotBeNull("Cookie content should be present in HTTP request");

        var contentString = cookieContent!.ReadAsStringAsync().Result;
        var jArray = JArray.Parse(contentString);

        jArray.Should().HaveCount(3);
        jArray[0]["name"]!.Value<string>().Should().Be("cookie1");
        jArray[0]["value"]!.Value<string>().Should().Be("value1");
        jArray[0]["domain"]!.Value<string>().Should().Be("domain1.com");

        jArray[1]["name"]!.Value<string>().Should().Be("cookie2");
        jArray[1]["value"]!.Value<string>().Should().Be("value2");
        jArray[1]["domain"]!.Value<string>().Should().Be("domain2.com");

        jArray[2]["name"]!.Value<string>().Should().Be("cookie3");
        jArray[2]["value"]!.Value<string>().Should().Be("value3");
        jArray[2]["domain"]!.Value<string>().Should().Be("domain3.com");
    }

    [Test]
    public void AddCookie_CalledWithNullRequiredCookieValues_ThrowsException()
    {
        // Arrange
        var builder = new HtmlRequestBuilder();

        // Act
        var act = () => builder.SetConversionBehaviors(b => b
            .AddCookie("cookie1", null!, "domain1.com")
            .AddCookie(null!, "value2", "domain2.com")
            .AddCookie("cookie3", "value3", "domain3.com"));

        act.Should().ThrowExactly<ArgumentException>("Caught cookie required field validation");
    }

    [Test]
    public void AddCookie_CalledMultipleTimes_AccumulatesCookies()
    {
        // Arrange
        var builder = new HtmlRequestBuilder();

        // Act
        builder.SetConversionBehaviors(b => b
            .AddCookie("cookie1", "value1", "domain1.com")
            .AddCookie("cookie2", "value2", "domain2.com")
            .AddCookie("cookie3", "value3", "domain3.com"));

        var request = builder.Build();

        // Assert
        request.ConversionBehaviors.Cookies.Should().NotBeNull();
        request.ConversionBehaviors.Cookies.Should().HaveCount(3);
    }

    [Test]
    public void Builder_WithCookies_ProducesCorrectJsonInHttpContent()
    {
        // Arrange
        var builder = new HtmlRequestBuilder();

        // Act
        builder.SetConversionBehaviors(b => b
            .AddCookie("yummy_cookie", "choco", "theyummycookie.com")
            .AddCookie(
                name: "session",
                value: "token123",
                domain: "secure.com",
                path: "/",
                secure: true,
                httpOnly: true,
                sameSite: "Lax"));

        var request = builder.Build();
        var httpContents = request.ConversionBehaviors.ToHttpContent().ToList();
        var cookieContent = httpContents.FirstOrDefault(c =>
            c.Headers.ContentDisposition?.Name == "cookies");

        // Assert
        cookieContent.Should().NotBeNull();
        var contentString = cookieContent!.ReadAsStringAsync().Result;
        var jArray = JArray.Parse(contentString);

        jArray.Should().HaveCount(2);

        var first = (JObject)jArray[0];
        first["name"]!.Value<string>().Should().Be("yummy_cookie");
        first["value"]!.Value<string>().Should().Be("choco");
        first["domain"]!.Value<string>().Should().Be("theyummycookie.com");

        var second = (JObject)jArray[1];
        second["name"]!.Value<string>().Should().Be("session");
        second["value"]!.Value<string>().Should().Be("token123");
        second["domain"]!.Value<string>().Should().Be("secure.com");
        second["path"]!.Value<string>().Should().Be("/");
        second["secure"]!.Value<bool>().Should().BeTrue();
        second["httpOnly"]!.Value<bool>().Should().BeTrue();
        second["sameSite"]!.Value<string>().Should().Be("Lax");
    }
}
