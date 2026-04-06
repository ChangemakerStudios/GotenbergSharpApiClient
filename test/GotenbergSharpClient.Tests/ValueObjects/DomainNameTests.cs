using Gotenberg.Sharp.API.Client.Domain.Shared;

namespace GotenbergSharpClient.Tests.ValueObjects;

[TestFixture]
public class DomainNameTests
{
    [Test]
    public void Create_WithValidDomain_ReturnsInstance()
    {
        var domain = DomainName.Create("cdn.example.com");

        domain.Value.Should().Be("cdn.example.com");
    }

    [Test]
    public void Create_TrimsWhitespace()
    {
        var domain = DomainName.Create("  cdn.example.com  ");

        domain.Value.Should().Be("cdn.example.com");
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void Create_WithNullOrEmpty_ThrowsArgumentException(string? input)
    {
        var act = () => DomainName.Create(input!);

        act.Should().ThrowExactly<ArgumentException>();
    }

    [Test]
    public void Equals_IsCaseInsensitive()
    {
        var a = DomainName.Create("CDN.Example.COM");
        var b = DomainName.Create("cdn.example.com");

        a.Should().Be(b);
        (a == b).Should().BeTrue();
    }

    [Test]
    public void ImplicitConversion_ToStringReturnsValue()
    {
        var domain = DomainName.Create("cdn.example.com");
        string result = domain;

        result.Should().Be("cdn.example.com");
    }
}
