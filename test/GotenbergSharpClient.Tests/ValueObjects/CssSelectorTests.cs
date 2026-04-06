using Gotenberg.Sharp.API.Client.Domain.HtmlBehavior;

namespace GotenbergSharpClient.Tests.ValueObjects;

[TestFixture]
public class CssSelectorTests
{
    [Test]
    public void Create_WithValidSelector_ReturnsInstance()
    {
        var selector = CssSelector.Create("#content");

        selector.Value.Should().Be("#content");
        selector.ToString().Should().Be("#content");
    }

    [TestCase(".loaded")]
    [TestCase("[data-ready]")]
    [TestCase("div > span.highlight")]
    [TestCase("#app .container:nth-child(2)")]
    public void Create_WithVariousSelectors_AcceptsAll(string input)
    {
        var selector = CssSelector.Create(input);

        selector.Value.Should().Be(input);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void Create_WithNullOrEmpty_ThrowsArgumentException(string? input)
    {
        var act = () => CssSelector.Create(input!);

        act.Should().ThrowExactly<ArgumentException>();
    }

    [Test]
    public void ImplicitConversion_ToStringReturnsValue()
    {
        CssSelector selector = CssSelector.Create("#test");
        string result = selector;

        result.Should().Be("#test");
    }

    [Test]
    public void Equals_WithSameValue_ReturnsTrue()
    {
        var a = CssSelector.Create("#test");
        var b = CssSelector.Create("#test");

        a.Should().Be(b);
        (a == b).Should().BeTrue();
    }

    [Test]
    public void Equals_WithDifferentValue_ReturnsFalse()
    {
        var a = CssSelector.Create("#test");
        var b = CssSelector.Create(".test");

        a.Should().NotBe(b);
    }
}
