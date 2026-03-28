using Gotenberg.Sharp.API.Client.Domain.ValueObjects;

namespace GotenbergSharpClient.Tests.ValueObjects;

[TestFixture]
public class HttpStatusCodeTests
{
    [TestCase(100)]
    [TestCase(200)]
    [TestCase(404)]
    [TestCase(499)]
    [TestCase(599)]
    public void Create_WithValidStatusCode_ReturnsInstance(int code)
    {
        var statusCode = HttpStatusCode.Create(code);

        statusCode.Value.Should().Be(code);
    }

    [TestCase(99)]
    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(600)]
    [TestCase(1000)]
    public void Create_WithOutOfRangeCode_ThrowsArgumentOutOfRangeException(int code)
    {
        var act = () => HttpStatusCode.Create(code);

        act.Should().ThrowExactly<ArgumentOutOfRangeException>();
    }

    [Test]
    public void ImplicitConversion_ToIntReturnsValue()
    {
        var statusCode = HttpStatusCode.Create(404);
        int result = statusCode;

        result.Should().Be(404);
    }

    [Test]
    public void Equals_WithSameValue_ReturnsTrue()
    {
        var a = HttpStatusCode.Create(499);
        var b = HttpStatusCode.Create(499);

        a.Should().Be(b);
        (a == b).Should().BeTrue();
    }

    [Test]
    public void CompareTo_OrdersCorrectly()
    {
        var low = HttpStatusCode.Create(200);
        var high = HttpStatusCode.Create(500);

        low.CompareTo(high).Should().BeNegative();
        high.CompareTo(low).Should().BePositive();
        low.CompareTo(low).Should().Be(0);
    }

    [Test]
    public void ToString_ReturnsInvariantString()
    {
        var statusCode = HttpStatusCode.Create(404);

        statusCode.ToString().Should().Be("404");
    }
}
