﻿// Copyright 2019-2025 Chris Mohan, Jaben Cargman
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

namespace Gotenberg.Sharp.API.Client.Domain.Requests;

/// <summary>
///     Represents a Gotenberg Api conversion request for HTML or Markdown to pdf
/// </summary>
/// <remarks>
///     For Markdown conversions your Content.Body must contain HTML that references one or more markdown files
///     using the Go template function 'toHTML' within the body element. Chrome uses the function to convert the contents
///     of a given markdown file to HTML.
///     See example here: https://gotenberg.dev/docs/modules/chromium#markdown
/// </remarks>
public sealed class HtmlRequest(bool containsMarkdown) : ChromeRequest
{
    public HtmlRequest()
        : this(false)
    {
    }

    protected override string ApiPath =>
        this.ContainsMarkdown
            ? Constants.Gotenberg.Chromium.ApiPaths.ConvertMarkdown
            : Constants.Gotenberg.Chromium.ApiPaths.ConvertHtml;

    public bool ContainsMarkdown { get; internal set; } = containsMarkdown;

    public FullDocument Content { get; internal set; } = new();

    /// <summary>
    ///     Transforms the instance to a list of HttpContent items
    /// </summary>
    protected override IEnumerable<HttpContent> ToHttpContent()
    {
        if (this.Content.Body == null)
            throw new InvalidOperationException("You need to Add at least a body");

        return base.ToHttpContent().Concat(this.Content.IfNullEmptyContent()).Concat(this.Assets.IfNullEmptyContent());
    }

    protected override void Validate()
    {
        if (this.Content?.Body == null)
            throw new InvalidOperationException("Request.Content or Content.Body is null");

        base.Validate();
    }
}