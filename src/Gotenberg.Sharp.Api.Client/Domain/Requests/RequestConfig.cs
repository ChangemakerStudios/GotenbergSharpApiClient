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
//  limitations under the License.

using System.Globalization;
using System.Text;
using Gotenberg.Sharp.API.Client.Domain.Pages;
using GotenbergApiVersion = Gotenberg.Sharp.API.Client.Domain.Shared.GotenbergApiVersion;
using Gotenberg.Sharp.API.Client.Domain.Webhooks;

namespace Gotenberg.Sharp.API.Client.Domain.Requests;

/// <summary>
///     All endpoints accept form fields for each property
/// </summary>
public sealed class RequestConfig : IConvertToHttpContent
{
    #region ToHttpContent

    /// <summary>
    ///     Converts the instance to a collection of http content items
    /// </summary>
    /// <returns></returns>
    public IEnumerable<HttpContent> ToHttpContent() => this.ToHttpContent(GotenbergApiVersion.V8);

    /// <summary>
    ///     Converts the instance to a collection of http content items for <paramref name="api" />.
    /// </summary>
    internal IEnumerable<HttpContent> ToHttpContent(GotenbergApiVersion api)
    {
        if (this.PageRanges?.Pages.Any() ?? false)
            yield return BuildRequestBase.CreateFormDataItem(
                this.PageRanges,
                api == GotenbergApiVersion.V9
                    ? Constants.Gotenberg.Chromium.Gotenberg9.PageRanges
                    : Constants.Gotenberg.Chromium.Shared.PageProperties.PageRanges);
    }

    #endregion

    public void Validate()
    {
        this.Webhook?.Validate();
    }

    public IEnumerable<(string Name, string? Value)> GetHeaders() => this.GetHeaders(GotenbergApiVersion.V8);

    /// <summary>
    ///     The request headers for <paramref name="api" />.
    /// </summary>
    internal IEnumerable<(string Name, string? Value)> GetHeaders(GotenbergApiVersion api)
    {
        if (this.Trace.IsSet())
            yield return (Constants.Gotenberg.All.Trace, this.Trace);

        if (this.ResultFileName.IsSet())
            yield return (Constants.Gotenberg.SharedFormFieldNames.OutputFileName, ToHeaderValue(this.ResultFileName!));

        foreach (var header in (this.Webhook?.GetHeaders(api)).IfNullEmpty()) yield return header;
    }

    /// <summary>
    ///     Makes a result file name fit an HTTP header value: accents are stripped (Müller becomes Muller), and
    ///     every other character outside printable ASCII (0x20-0x7E), line breaks included, becomes <c>_</c>.
    ///     A character outside the Basic Multilingual Plane, such as an emoji, gives one <c>_</c>.
    /// </summary>
    /// <remarks>
    ///     HttpClient refuses line breaks outright and rejects or mangles non-ASCII values, depending on the runtime.
    /// </remarks>
    internal static string ToHeaderValue(string name)
    {
        var result = new StringBuilder(name.Length);

        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];

            if (c >= ' ' && c <= '~')
            {
                result.Append(c);
                continue;
            }

            // A surrogate pair is one character; a lone surrogate cannot be normalized.
            if (char.IsSurrogate(c))
            {
                if (char.IsHighSurrogate(c) && i + 1 < name.Length && char.IsLowSurrogate(name[i + 1])) i++;
                result.Append('_');
                continue;
            }

            // One character at a time, so that a character that decomposes into several (a Hangul
            // syllable into its jamo) still gives one '_'.
            var decomposed = c.ToString().Normalize(NormalizationForm.FormD);
            var letters = new StringBuilder(decomposed.Length);

            foreach (var d in decomposed)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(d);

                if (category is UnicodeCategory.NonSpacingMark
                    or UnicodeCategory.SpacingCombiningMark
                    or UnicodeCategory.EnclosingMark)
                    continue;

                letters.Append(d);
            }

            // Nothing left: the character was a combining mark by itself.
            if (letters.Length == 0) continue;

            result.Append(letters.ToString().All(l => l >= ' ' && l <= '~') ? letters.ToString() : "_");
        }

        return result.ToString();
    }

    #region Basic settings

    /// <summary>
    ///     If provided, the API will return a pdf containing the pages in the specified range.
    /// </summary>
    /// <remarks>
    ///     The format is the same as the one from the print options of Google Chrome, e.g. 1-5,8,11-13.
    ///     Not sent on screenshot requests: no screenshot route accepts it.
    ///     This may move...
    /// </remarks>
    public PageRanges? PageRanges { get; set; }

    /// <summary>
    ///     If provided, the API will return the resulting PDF file with the given filename. Otherwise a random filename is
    ///     used. Sent as the <c>Gotenberg-Output-Filename</c> request header, with accents stripped
    ///     (<c>Müller</c> is sent as <c>Muller</c>) and every other character outside printable ASCII, line breaks
    ///     included, replaced by <c>_</c>. This property keeps the name as set.
    /// </summary>
    /// <remarks>
    ///     Attention: this feature does not work if the form field webHookURL is given.
    /// </remarks>
    public string? ResultFileName { get; set; }

    /// <summary>
    ///     If provided, the API will send the resulting PDF file in a POST request with the application/pdf Content-Type to
    ///     given URL.
    ///     Requests to the API complete before the conversions complete. For web hook configured requests,
    ///     call FireWebhookAndForgetAsync on the client which returns nothing.
    /// </summary>
    /// <remarks>All request types support web hooks</remarks>
    public Webhook? Webhook { get; set; }

    /// <summary>
    ///     If provided, the trace, or request ID, header will be added to the request.
    ///     If you're using the webhook feature, it also adds the header to each request to your callbacks.
    /// </summary>
    public string? Trace { get; set; }

    #endregion
}