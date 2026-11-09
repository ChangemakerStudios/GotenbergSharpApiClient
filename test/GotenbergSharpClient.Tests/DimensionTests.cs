using System.Globalization;

using Gotenberg.Sharp.API.Client.Application.Builders;
using Gotenberg.Sharp.API.Client.Application.Pages;
using Gotenberg.Sharp.API.Client.Domain.Dimensions;
using Gotenberg.Sharp.API.Client.Domain.Pages;

namespace GotenbergSharpClient.Tests;

[TestFixture]
public class DimensionTests
{
    [TestCase(PaperSizes.A3, 297d, 420d)]
    [TestCase(PaperSizes.A4, 210d, 297d)]
    [TestCase(PaperSizes.A5, 148d, 210d)]
    [TestCase(PaperSizes.A6, 105d, 148d)]
    public void SetPaperSize_ASeries_IsExactAndSentAsBareInches(PaperSizes size, double widthMm, double heightMm)
    {
        var props = new PageProperties();

        new PagePropertyBuilder(props).SetPaperSize(size);

        SentMillimetres(props.PaperWidth).Should().BeApproximately(widthMm, 1e-9);
        SentMillimetres(props.PaperHeight).Should().BeApproximately(heightMm, 1e-9);
    }

    // Gotenberg before 8.3.0 parses paper sizes as plain floats, so the sent value must carry no unit.
    private static double SentMillimetres(Dimension? dimension) =>
        double.Parse(dimension!.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture) * 25.4;

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
    public void ToA4WithNoMargins_IsExactAndSentAsBareInches()
    {
        var props = PageProperties.ToA4WithNoMargins();

        SentMillimetres(props.PaperWidth).Should().BeApproximately(210d, 1e-9);
        SentMillimetres(props.PaperHeight).Should().BeApproximately(297d, 1e-9);
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

    [GotenbergTest]
    public async Task HtmlToPdf_WithEachPaperSize_IsAccepted(GotenbergServer gotenberg)
    {
        var client = await gotenberg.CreateClientAsync();

        foreach (var size in Enum.GetValues<PaperSizes>().Where(size => size != PaperSizes.None))
        {
            var builder = new HtmlRequestBuilder()
                .AddDocument(doc => doc.SetBody("<html><body><p>Paper size</p></body></html>"))
                .WithPageProperties(page => page.SetPaperSize(size).SetMargins(Margins.Normal));

            await using var pdf = await client.HtmlToPdfAsync(builder);

            pdf.Length.Should().BeGreaterThan(0, $"Gotenberg should accept {size}");
        }
    }
}
