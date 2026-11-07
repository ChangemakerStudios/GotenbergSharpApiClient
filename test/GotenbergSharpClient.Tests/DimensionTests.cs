using Gotenberg.Sharp.API.Client.Application.Pages;
using Gotenberg.Sharp.API.Client.Domain.Dimensions;
using Gotenberg.Sharp.API.Client.Domain.Pages;

namespace GotenbergSharpClient.Tests;

[TestFixture]
public class DimensionTests
{
    [TestCase(PaperSizes.A3, "297mm", "420mm")]
    [TestCase(PaperSizes.A4, "210mm", "297mm")]
    [TestCase(PaperSizes.A5, "148mm", "210mm")]
    [TestCase(PaperSizes.A6, "105mm", "148mm")]
    public void SetPaperSize_ASeries_UsesExactMillimetres(PaperSizes size, string width, string height)
    {
        var props = new PageProperties();

        new PagePropertyBuilder(props).SetPaperSize(size);

        props.PaperWidth.ToString().Should().Be(width);
        props.PaperHeight.ToString().Should().Be(height);
    }

    [TestCase(PaperSizes.Letter, "8.5", "11")]
    [TestCase(PaperSizes.Legal, "8.5", "14")]
    [TestCase(PaperSizes.Tabloid, "11", "17")]
    [TestCase(PaperSizes.D, "22", "34")]
    [TestCase(PaperSizes.E, "34", "44")]
    public void SetPaperSize_NorthAmerican_StaysInInches(PaperSizes size, string width, string height)
    {
        var props = new PageProperties();

        new PagePropertyBuilder(props).SetPaperSize(size);

        props.PaperWidth.ToString().Should().Be(width);
        props.PaperHeight.ToString().Should().Be(height);
    }

    [TestCase(Margins.None, "0")]
    [TestCase(Margins.Default, "0.39")]
    [TestCase(Margins.Normal, "1")]
    [TestCase(Margins.Large, "2")]
    public void SetMargins_Presets_AreUnchanged(Margins margins, string expected)
    {
        var props = new PageProperties();

        new PagePropertyBuilder(props).SetMargins(margins);

        props.MarginTop.ToString().Should().Be(expected);
        props.MarginRight.ToString().Should().Be(expected);
        props.MarginBottom.ToString().Should().Be(expected);
        props.MarginLeft.ToString().Should().Be(expected);
    }

    [Test]
    public void ToA4WithNoMargins_UsesExactMillimetres()
    {
        var props = PageProperties.ToA4WithNoMargins();

        props.PaperWidth.ToString().Should().Be("210mm");
        props.PaperHeight.ToString().Should().Be("297mm");
    }

    [Test]
    public void BareNumber_IsInches_ButTheConversionsAreObsolete()
    {
        // A bare number has always meant inches; keep that, but warn at compile time.
        Dimension.Parse("10").Should().Be(Dimension.FromInches(10));

        var conversions = typeof(Dimension)
            .GetMethods()
            .Where(m => m.Name == "op_Implicit" && m.ReturnType == typeof(Dimension))
            .ToList();

        conversions.Should().HaveCount(2);
        conversions.Should().OnlyContain(m => m.GetCustomAttributes(typeof(ObsoleteAttribute), false).Length == 1);
    }
}
