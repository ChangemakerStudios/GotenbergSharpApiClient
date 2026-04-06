using Gotenberg.Sharp.API.Client.Domain.Shared;

namespace GotenbergSharpClient.Tests.ValueObjects;

[TestFixture]
public class GotenbergStatusCodeTests
{
    [TestCase(100)]
    [TestCase(200)]
    [TestCase(404)]
    [TestCase(499)]
    [TestCase(599)]
    public void Create_WithValidStatusCode_ReturnsInstance(int code)
    {
        var statusCode = GotenbergStatusCode.Create(code);

        statusCode.Value.Should().Be(code);
    }

    [TestCase(99)]
    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(600)]
    [TestCase(1000)]
    public void Create_WithOutOfRangeCode_ThrowsArgumentOutOfRangeException(int code)
    {
        var act = () => GotenbergStatusCode.Create(code);

        act.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Test]
    public void ImplicitConversion_ToIntReturnsValue()
    {
        var statusCode = GotenbergStatusCode.Create(404);
        int result = statusCode;

        result.Should().Be(404);
    }

    [Test]
    public void ImplicitConversion_WithNull_Throws()
    {
        GotenbergStatusCode? nullCode = null;

        var act = () => { int _ = nullCode!; };

        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Test]
    public void Equals_WithSameValue_ReturnsTrue()
    {
        var a = GotenbergStatusCode.Create(499);
        var b = GotenbergStatusCode.Create(499);

        a.Should().Be(b);
        (a == b).Should().BeTrue();
    }

    [Test]
    public void CompareTo_OrdersCorrectly()
    {
        var low = GotenbergStatusCode.Create(200);
        var high = GotenbergStatusCode.Create(500);

        low.CompareTo(high).Should().BeNegative();
        high.CompareTo(low).Should().BePositive();
        low.CompareTo(low).Should().Be(0);
    }

    [Test]
    public void ToString_ReturnsInvariantString()
    {
        var statusCode = GotenbergStatusCode.Create(404);

        statusCode.ToString().Should().Be("404");
    }
}
