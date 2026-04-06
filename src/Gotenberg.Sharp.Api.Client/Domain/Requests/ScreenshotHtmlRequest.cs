// Copyright 2019-2026 Chris Mohan, Jaben Cargman
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

namespace Gotenberg.Sharp.API.Client.Domain.Requests;

/// <summary>
/// Captures a screenshot of HTML content using Chromium.
/// </summary>
public sealed class ScreenshotHtmlRequest : ScreenshotRequest
{
    protected override string ApiPath => Constants.Gotenberg.Chromium.ApiPaths.ScreenshotHtml;

    public FullDocument? Content { get; set; }

    protected override void Validate()
    {
        if (this.Content?.Body == null)
            throw new InvalidOperationException("HTML body content is required for screenshot.");

        base.Validate();
    }

    protected override IEnumerable<HttpContent> ToHttpContent()
    {
        if (this.Content != null)
        {
            foreach (var item in this.Content.ToHttpContent())
                yield return item;
        }

        foreach (var item in this.Assets.IfNullEmptyContent())
            yield return item;

        foreach (var item in base.ToHttpContent())
            yield return item;
    }
}
