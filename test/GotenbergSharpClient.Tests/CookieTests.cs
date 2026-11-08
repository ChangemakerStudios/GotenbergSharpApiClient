using Gotenberg.Sharp.API.Client.Domain.Cookies;
using Gotenberg.Sharp.API.Client.Domain.HtmlBehavior;

using System.Text.Json.Nodes;

namespace GotenbergSharpClient.Tests;

[TestFixture]
public class CookieTests
{
    [Test]
    public void Cookie_WithRequiredFieldsOnly_SerializesToCorrectJson()
    {
        // Arrange
        var cookie = new Cookie
        {
            Name = "yummy_cookie",
            Value = "choco",
            Domain = "theyummycookie.com"
        };

        // Act
        var json = System.Text.Json.JsonSerializer.Serialize(cookie);
        var jObject = JsonNode.Parse(json)!.AsObject();

        // Assert
        jObject["name"]!.GetValue<string>().Should().Be("yummy_cookie");
        jObject["value"]!.GetValue<string>().Should().Be("choco");
        jObject["domain"]!.GetValue<string>().Should().Be("theyummycookie.com");
        jObject.ContainsKey("path").Should().BeFalse();
        jObject.ContainsKey("secure").Should().BeFalse();
        jObject.ContainsKey("httpOnly").Should().BeFalse();
        jObject.ContainsKey("sameSite").Should().BeFalse();
    }

    [Test]
    public void Cookie_WithAllFields_SerializesToCorrectJson()
    {
        // Arrange
        var cookie = new Cookie
        {
            Name = "session_token",
            Value = "abc123",
            Domain = "example.com",
            Path = "/api",
            Secure = true,
            HttpOnly = true,
            SameSite = "Strict"
        };

        // Act
        var json = System.Text.Json.JsonSerializer.Serialize(cookie);
        var jObject = JsonNode.Parse(json)!.AsObject();

        // Assert
        jObject["name"]!.GetValue<string>().Should().Be("session_token");
        jObject["value"]!.GetValue<string>().Should().Be("abc123");
        jObject["domain"]!.GetValue<string>().Should().Be("example.com");
        jObject["path"]!.GetValue<string>().Should().Be("/api");
        jObject["secure"]!.GetValue<bool>().Should().BeTrue();
        jObject["httpOnly"]!.GetValue<bool>().Should().BeTrue();
        jObject["sameSite"]!.GetValue<string>().Should().Be("Strict");
    }

    [Test]
    public void CookieList_SerializesToCorrectJsonArray()
    {
        // Arrange
        var cookies = new List<Cookie>
        {
            new Cookie
            {
                Name = "yummy_cookie",
                Value = "choco",
                Domain = "theyummycookie.com"
            },
            new Cookie
            {
                Name = "session_token",
                Value = "abc123",
                Domain = "example.com",
                Path = "/",
                Secure = true,
                HttpOnly = true,
                SameSite = "Lax"
            }
        };

        // Act
        var json = System.Text.Json.JsonSerializer.Serialize(cookies);
        var jArray = JsonNode.Parse(json)!.AsArray();

        // Assert
        jArray.Should().HaveCount(2);

        var first = jArray[0]!.AsObject();
        first["name"]!.GetValue<string>().Should().Be("yummy_cookie");
        first["value"]!.GetValue<string>().Should().Be("choco");
        first["domain"]!.GetValue<string>().Should().Be("theyummycookie.com");

        var second = jArray[1]!.AsObject();
        second["name"]!.GetValue<string>().Should().Be("session_token");
        second["value"]!.GetValue<string>().Should().Be("abc123");
        second["domain"]!.GetValue<string>().Should().Be("example.com");
        second["path"]!.GetValue<string>().Should().Be("/");
        second["secure"]!.GetValue<bool>().Should().BeTrue();
        second["httpOnly"]!.GetValue<bool>().Should().BeTrue();
        second["sameSite"]!.GetValue<string>().Should().Be("Lax");
    }

    [Test]
    public void HtmlConversionBehaviors_WithCookies_ConvertsToCorrectHttpContent()
    {
        // Arrange
        var behaviors = new HtmlConversionBehaviors
        {
            Cookies = new List<Cookie>
            {
                new Cookie
                {
                    Name = "test_cookie",
                    Value = "test_value",
                    Domain = "test.com"
                }
            }
        };

        // Act
        var httpContents = behaviors.ToHttpContent().ToList();

        // Assert
        var cookieContent = httpContents.FirstOrDefault(c =>
            c.Headers.ContentDisposition?.Name == "cookies");

        cookieContent.Should().NotBeNull("Cookie content should be present in HTTP content");

        var contentString = cookieContent!.ReadAsStringAsync().Result;
        var jArray = JsonNode.Parse(contentString)!.AsArray();

        jArray.Should().HaveCount(1);
        var cookie = jArray[0]!.AsObject();
        cookie["name"]!.GetValue<string>().Should().Be("test_cookie");
        cookie["value"]!.GetValue<string>().Should().Be("test_value");
        cookie["domain"]!.GetValue<string>().Should().Be("test.com");
    }

    [Test]
    public void HtmlConversionBehaviors_WithMultipleCookies_ConvertsToCorrectJsonArray()
    {
        // Arrange
        var behaviors = new HtmlConversionBehaviors
        {
            Cookies = new List<Cookie>
            {
                new Cookie { Name = "cookie1", Value = "value1", Domain = "domain1.com" },
                new Cookie { Name = "cookie2", Value = "value2", Domain = "domain2.com", Secure = true },
                new Cookie { Name = "cookie3", Value = "value3", Domain = "domain3.com", HttpOnly = true, SameSite = "None" }
            }
        };

        // Act
        var httpContents = behaviors.ToHttpContent().ToList();
        var cookieContent = httpContents.FirstOrDefault(c =>
            c.Headers.ContentDisposition?.Name == "cookies");

        var contentString = cookieContent!.ReadAsStringAsync().Result;
        var jArray = JsonNode.Parse(contentString)!.AsArray();

        // Assert
        jArray.Should().HaveCount(3);

        var first = jArray[0]!.AsObject();
        first["name"]!.GetValue<string>().Should().Be("cookie1");
        first.ContainsKey("secure").Should().BeFalse();

        var second = jArray[1]!.AsObject();
        second["name"]!.GetValue<string>().Should().Be("cookie2");
        second["secure"]!.GetValue<bool>().Should().BeTrue();

        var third = jArray[2]!.AsObject();
        third["name"]!.GetValue<string>().Should().Be("cookie3");
        third["httpOnly"]!.GetValue<bool>().Should().BeTrue();
        third["sameSite"]!.GetValue<string>().Should().Be("None");
    }

    [Test]
    public void HtmlConversionBehaviors_WithNullCookies_DoesNotIncludeCookieContent()
    {
        // Arrange
        var behaviors = new HtmlConversionBehaviors
        {
            Cookies = null
        };

        // Act
        var httpContents = behaviors.ToHttpContent().ToList();

        // Assert
        var cookieContent = httpContents.FirstOrDefault(c =>
            c.Headers.ContentDisposition?.Name == "cookies");

        cookieContent.Should().BeNull("Cookie content should not be present when Cookies is null");
    }

    [Test]
    public void HtmlConversionBehaviors_WithEmptyCookieList_IncludesEmptyArray()
    {
        // Arrange
        var behaviors = new HtmlConversionBehaviors
        {
            Cookies = new List<Cookie>()
        };

        // Act
        var httpContents = behaviors.ToHttpContent().ToList();
        var cookieContent = httpContents.FirstOrDefault(c =>
            c.Headers.ContentDisposition?.Name == "cookies");

        var contentString = cookieContent!.ReadAsStringAsync().Result;
        var jArray = JsonNode.Parse(contentString)!.AsArray();

        // Assert
        jArray.Should().HaveCount(0, "Empty cookie list should serialize to empty JSON array");
    }
}
