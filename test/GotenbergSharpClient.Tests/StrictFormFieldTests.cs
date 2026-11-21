//  Copyright 2019-2025 Chris Mohan, Jaben Cargman
//  and GotenbergSharpApiClient Contributors
// 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and

using Gotenberg.Sharp.API.Client.Application.Builders;
using Gotenberg.Sharp.API.Client.Domain.LibreOffice;
using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Requests.ApiRequests;
using Gotenberg.Sharp.API.Client.Domain.Shared;
using Gotenberg.Sharp.API.Client.Infrastructure;

namespace GotenbergSharpClient.Tests;

/// <summary>
/// What goes on the wire for fields Gotenberg 9 is strict about: it answers 400 to every form
/// field a route does not know, and reads some options only as request headers.
/// </summary>
[TestFixture]
public class StrictFormFieldTests
{
    private static string? FieldName(HttpContent content) => content.Headers.ContentDisposition?.Name?.Trim('"');

    private static IEnumerable<HttpContent> Content(IApiRequest request) =>
        ((IConvertToHttpContent)request).ToHttpContent();

    private static List<string?> FieldNames(IApiRequest request) =>
        Content(request).Select(FieldName).ToList();

    private static MergeOfficeBuilder OfficeBuilder() => new MergeOfficeBuilder()
        .WithAssets(a => a.AddItem("one.docx", new byte[] { 1, 2, 3 }));

    #region Result file name

    [Test]
    public void ResultFileName_IsSentAsRequestHeaderNotFormField()
    {
        var request = new HtmlRequestBuilder()
            .AddDocument(d => d.SetBody("<html></html>"))
            .ConfigureRequest(c => c.SetResultFileName("report"))
            .Build()
            .CreateApiRequest();

        using var message = request.ToApiRequestMessage();

        message.Headers.GetValues(Constants.Gotenberg.SharedFormFieldNames.OutputFileName)
            .Should().Equal("report");
        FieldNames(request).Should().NotContain(Constants.Gotenberg.SharedFormFieldNames.OutputFileName);
    }

    [Test]
    public void ResultFileName_NotSet_SendsNoHeader()
    {
        var request = new HtmlRequestBuilder()
            .AddDocument(d => d.SetBody("<html></html>"))
            .Build()
            .CreateApiRequest();

        using var message = request.ToApiRequestMessage();

        message.Headers.Contains(Constants.Gotenberg.SharedFormFieldNames.OutputFileName).Should().BeFalse();
    }

    [TestCase("Müller-Bericht", "Muller-Bericht")]
    [TestCase("résumé", "resume")]
    [TestCase("résumé", "resume")] // already decomposed
    [TestCase("日本語", "___")]
    [TestCase("한글", "__")] // a Hangul syllable is one character, not its jamo
    [TestCase("a\r\nb", "a__b")]
    [TestCase("a\tb", "a_b")]
    [TestCase("report 2026", "report 2026")]
    [TestCase("Straße", "Stra_e")]
    [TestCase("a\U0001F600b", "a_b")] // one character outside the BMP gives one '_'
    [TestCase("a\u007Fb", "a_b")]
    public void ResultFileName_NotPrintableAscii_IsCleanedUpInTheHeader(string name, string sent)
    {
        var request = new HtmlRequestBuilder()
            .AddDocument(d => d.SetBody("<html></html>"))
            .ConfigureRequest(c => c.SetResultFileName(name))
            .Build()
            .CreateApiRequest();

        request.RenderFor(GotenbergApiVersion.V8).Header(Constants.Gotenberg.SharedFormFieldNames.OutputFileName)
            .Should().Equal(sent);
        request.RenderFor(GotenbergApiVersion.V9).Header(Constants.Gotenberg.SharedFormFieldNames.OutputFileName)
            .Should().Equal(sent);
    }

    [Test]
    public void ResultFileName_ALoneSurrogate_IsCleanedUpToo()
    {
        // Built here: a lone surrogate does not survive an attribute argument.
        var name = "a" + (char)0xD83D + "b" + (char)0xDE00 + "c";

        var request = new HtmlRequestBuilder()
            .AddDocument(d => d.SetBody("<html></html>"))
            .ConfigureRequest(c => c.SetResultFileName(name))
            .Build()
            .CreateApiRequest();

        request.RenderFor(GotenbergApiVersion.V8).Header(Constants.Gotenberg.SharedFormFieldNames.OutputFileName)
            .Should().Equal("a_b_c");
    }

    [Test]
    public void ResultFileName_NotPrintableAscii_KeepsTheValueSetAndValidates()
    {
        var request = new HtmlRequestBuilder()
            .AddDocument(d => d.SetBody("<html></html>"))
            .ConfigureRequest(c => c.SetResultFileName("Müller\r\n"))
            .Build();

        request.Config!.ResultFileName.Should().Be("Müller\r\n");
        request.Config.Invoking(c => c.Validate()).Should().NotThrow();
        request.Invoking(r => r.CreateApiRequest()).Should().NotThrow();
        request.Config.ResultFileName.Should().Be("Müller\r\n");
    }

    #endregion

    #region Screenshots

    [Test]
    public void Screenshot_PageRangesFromConfig_AreNotSent()
    {
        var request = new ScreenshotUrlRequestBuilder()
            .SetUrl("https://example.com")
            .ConfigureRequest(c => c.SetPageRanges("1-2"))
            .Build()
            .CreateApiRequest();

        FieldNames(request).Should().NotContain("nativePageRanges");
    }

    #endregion

    #region Office merge

    [Test]
    public void MergeOffice_MergeIsSentOnce_EvenWhenOptionsSetIt()
    {
        var builder = OfficeBuilder();
        var request = builder.Build();
        request.LibreOfficeOptions = new LibreOfficeOptions { Merge = false };

        FieldNames(request.CreateApiRequest()).Count(n => n == "merge").Should().Be(1);
        var merge = Content(request.CreateApiRequest()).Single(c => FieldName(c) == "merge");
        merge.ReadAsStringAsync().Result.Should().Be("true");
    }

    [Test]
    public void MergeOffice_PageRangesAreSentOnce_ConfigWins()
    {
        var request = OfficeBuilder()
            .SetPageRanges("3-4")
            .ConfigureRequest(c => c.SetPageRanges("1-2"))
            .Build()
            .CreateApiRequest();

        var ranges = Content(request).Where(c => FieldName(c) == "nativePageRanges").ToList();

        ranges.Should().ContainSingle();
        ranges[0].ReadAsStringAsync().Result.Should().Be("1-2");
    }

    [Test]
    public void MergeOffice_PageRangesOfRequestAlone_AreSent()
    {
        var request = OfficeBuilder().SetPageRanges("3-4").Build().CreateApiRequest();

        var ranges = Content(request).Where(c => FieldName(c) == "nativePageRanges").ToList();

        ranges.Should().ContainSingle();
        ranges[0].ReadAsStringAsync().Result.Should().Be("3-4");
    }

    #endregion

    #region PDF conversion

    [Test]
    public void PdfConversion_EachPdfIsSentOnce()
    {
        var request = new PdfConversionBuilder()
            .WithPdfs(a => a
                .AddItem("a.pdf", new byte[] { 1, 2, 3 })
                .AddItem("b.pdf", new byte[] { 4, 5, 6 }))
            .Build()
            .CreateApiRequest();

        var files = Content(request).Where(c => FieldName(c) == "files").ToList();

        files.Select(f => f.Headers.ContentDisposition!.FileName!.Trim('"')).Should().BeEquivalentTo("a.pdf", "b.pdf");
        files.Should().HaveCount(2);
    }

    #endregion

    #region Integration

    [GotenbergTest("8.0.0")]
    public async Task ResultFileName_IsHonouredByServer(GotenbergServer gotenberg)
    {
        var http = new HttpClient { BaseAddress = await gotenberg.GetUrlAsync() };

        if (gotenberg.UsesBasicAuth)
            http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{GotenbergServer.Username}:{GotenbergServer.Password}")));

        var client = new DispositionCapturingClient(http);

        // Gotenberg appends the extension; the accent of the second name is stripped before it is sent.
        foreach (var (name, returned) in new[] { ("report", "report.pdf"), ("Müller", "Muller.pdf") })
        {
            var builder = new HtmlRequestBuilder()
                .AddDocument(d => d.SetBody("<html><body>named</body></html>"))
                .ConfigureRequest(c => c.SetResultFileName(name));

            using var result = await client.HtmlToPdfAsync(builder);

            result.Length.Should().BeGreaterThan(0);
            client.ContentDisposition.Should().Contain(returned);
        }
    }

    private sealed class DispositionCapturingClient(HttpClient http)
        : Gotenberg.Sharp.API.Client.GotenbergSharpClient(http)
    {
        public string? ContentDisposition { get; private set; }

        protected override async Task<HttpResponseMessage> SendRequestAsync(
            IApiRequest request,
            HttpCompletionOption option,
            CancellationToken cancelToken)
        {
            var response = await base.SendRequestAsync(request, option, cancelToken);
            ContentDisposition = response.Content.Headers.ContentDisposition?.ToString();
            return response;
        }
    }

    #endregion
}
