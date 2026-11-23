// Copyright 2019-2025 Chris Mohan, Jaben Cargman
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

using Gotenberg.Sharp.API.Client.Domain.Documents;
using Gotenberg.Sharp.API.Client.Domain.ExtraUrls;
using Gotenberg.Sharp.API.Client.Domain.Shared;

namespace Gotenberg.Sharp.API.Client.Domain.Requests;

public sealed class UrlRequest : ChromeRequest
{
    protected override string ApiPath => Constants.Gotenberg.Chromium.ApiPaths.ConvertUrl;

    public Uri? Url { get; set; }

    /// <summary>
    ///  Requires top/bottom margin set to appear
    /// </summary>
    public HeaderFooterDocument? Content { get; set; }

    /// <summary>
    /// Link and script tags to inject into the page, sent as <c>extraLinkTags</c> and <c>extraScriptTags</c> to
    /// Gotenberg 7 and 8. Not sent to Gotenberg 9, whose URL route answers 400 to fields it does not know;
    /// neither Gotenberg 8.40 nor 9.5 reads them, so the result is what Gotenberg 8 gave.
    /// </summary>
    public ExtraUrlResources? ExtraResources { get; set; }

    protected override IEnumerable<HttpContent> ToHttpContent(GotenbergApiVersion api)
    {
        if (this.Url == null) throw new InvalidOperationException("Url is null");
        if (!this.Url.IsAbsoluteUri)
            throw new InvalidOperationException("Url.IsAbsoluteUri equals false");

        return base.ToHttpContent(api)
            .Concat(Content.IfNullEmptyContent(api))
            .Concat(api == GotenbergApiVersion.V9 ? [] : ExtraResources.IfNullEmptyContent())
            .Concat(Assets.IfNullEmptyContent())
            .Concat(
            [
                CreateFormDataItem(this.Url, Constants.Gotenberg.Chromium.Routes.Url.RemoteUrl)
            ]);
    }

    protected override void Validate()
    {
        if (this.Url == null) throw new InvalidOperationException("Request.Url is null");

        base.Validate();
    }
}
