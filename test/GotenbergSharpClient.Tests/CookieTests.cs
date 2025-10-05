using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
        var json = JsonConvert.SerializeObject(cookie);
        var jObject = JObject.Parse(json);

        // Assert
        jObject["name"]!.Value<string>().Should().Be("yummy_cookie");
        jObject["value"]!.Value<string>().Should().Be("choco");
        jObject["domain"]!.Value<string>().Should().Be("theyummycookie.com");
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
        var json = JsonConvert.SerializeObject(cookie);
        var jObject = JObject.Parse(json);

        // Assert
        jObject["name"]!.Value<string>().Should().Be("session_token");
        jObject["value"]!.Value<string>().Should().Be("abc123");
        jObject["domain"]!.Value<string>().Should().Be("example.com");
        jObject["path"]!.Value<string>().Should().Be("/api");
        jObject["secure"]!.Value<bool>().Should().BeTrue();
        jObject["httpOnly"]!.Value<bool>().Should().BeTrue();
        jObject["sameSite"]!.Value<string>().Should().Be("Strict");
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
        var json = JsonConvert.SerializeObject(cookies);
        var jArray = JArray.Parse(json);

        // Assert
        jArray.Should().HaveCount(2);

        var first = (JObject)jArray[0];
        first["name"]!.Value<string>().Should().Be("yummy_cookie");
        first["value"]!.Value<string>().Should().Be("choco");
        first["domain"]!.Value<string>().Should().Be("theyummycookie.com");

        var second = (JObject)jArray[1];
        second["name"]!.Value<string>().Should().Be("session_token");
        second["value"]!.Value<string>().Should().Be("abc123");
        second["domain"]!.Value<string>().Should().Be("example.com");
        second["path"]!.Value<string>().Should().Be("/");
        second["secure"]!.Value<bool>().Should().BeTrue();
        second["httpOnly"]!.Value<bool>().Should().BeTrue();
        second["sameSite"]!.Value<string>().Should().Be("Lax");
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
        var jArray = JArray.Parse(contentString);

        jArray.Should().HaveCount(1);
        var cookie = (JObject)jArray[0];
        cookie["name"]!.Value<string>().Should().Be("test_cookie");
        cookie["value"]!.Value<string>().Should().Be("test_value");
        cookie["domain"]!.Value<string>().Should().Be("test.com");
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
        var jArray = JArray.Parse(contentString);

        // Assert
        jArray.Should().HaveCount(3);

        var first = (JObject)jArray[0];
        first["name"]!.Value<string>().Should().Be("cookie1");
        first.ContainsKey("secure").Should().BeFalse();

        var second = (JObject)jArray[1];
        second["name"]!.Value<string>().Should().Be("cookie2");
        second["secure"]!.Value<bool>().Should().BeTrue();

        var third = (JObject)jArray[2];
        third["name"]!.Value<string>().Should().Be("cookie3");
        third["httpOnly"]!.Value<bool>().Should().BeTrue();
        third["sameSite"]!.Value<string>().Should().Be("None");
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
        var jArray = JArray.Parse(contentString);

        // Assert
        jArray.Should().HaveCount(0, "Empty cookie list should serialize to empty JSON array");
    }
}
