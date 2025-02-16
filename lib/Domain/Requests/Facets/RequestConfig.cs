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

using Gotenberg.Sharp.API.Client.Domain.Pages;

namespace Gotenberg.Sharp.API.Client.Domain.Requests.Facets;

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
    // ReSharper disable once MethodTooLong
    public IEnumerable<HttpContent> ToHttpContent()
    {
        if (this.PageRanges?.Pages.Any() ?? false)
            yield return BuildRequestBase.CreateFormDataItem(
                this.PageRanges,
                Constants.Gotenberg.Chromium.Shared.PageProperties.PageRanges);

        if (this.ResultFileName.IsSet())
            yield return BuildRequestBase.CreateFormDataItem(
                this.ResultFileName,
                Constants.Gotenberg.SharedFormFieldNames.OutputFileName);
    }

    #endregion

    public void Validate()
    {
        this.Webhook?.Validate();
    }

    public IEnumerable<(string Name, string? Value)> GetHeaders()
    {
        if (this.Trace.IsSet())
            yield return (Constants.Gotenberg.All.Trace, this.Trace);

        foreach (var header in (this.Webhook?.GetHeaders()).IfNullEmpty()) yield return header;
    }

    #region Basic settings

    /// <summary>
    ///     If provided, the API will return a pdf containing the pages in the specified range.
    /// </summary>
    /// <remarks>
    ///     The format is the same as the one from the print options of Google Chrome, e.g. 1-5,8,11-13.
    ///     This may move...
    /// </remarks>
    public PageRanges? PageRanges { get; set; }

    /// <summary>
    ///     If provided, the API will return the resulting PDF file with the given filename. Otherwise a random filename is
    ///     used.
    /// </summary>
    /// <remarks>
    ///     Attention: this feature does not work if the form field webHookURL is given.
    /// </remarks>
    // Not sure if this is useful with the way this client is used, although.. maybe Webhook requests honor it?
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