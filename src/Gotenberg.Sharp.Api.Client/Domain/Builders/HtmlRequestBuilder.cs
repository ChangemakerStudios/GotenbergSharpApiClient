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

namespace Gotenberg.Sharp.API.Client.Domain.Builders;

/// <summary>
/// Builds requests for converting HTML or Markdown content to PDF using Gotenberg's Chromium module.
/// Supports embedded assets (images, stylesheets, fonts) and custom header/footer documents.
/// </summary>
/// <remarks>
/// Use the parameterless constructor for HTML content. For Markdown conversion, use the constructor
/// overload and pass true, or call SetContainsMarkdown() on the DocumentBuilder.
/// </remarks>
public sealed class HtmlRequestBuilder(bool containsMarkdown)
    : BaseChromiumBuilder<HtmlRequest, HtmlRequestBuilder>(new HtmlRequest(containsMarkdown))
{
    /// <summary>
    /// Initializes a new instance for HTML to PDF conversion.
    /// </summary>
    public HtmlRequestBuilder()
        : this(false)
    {
    }

    /// <summary>
    /// Configures the HTML or Markdown document content including body, header, and footer.
    /// </summary>
    /// <param name="action">Configuration action for setting document content.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public HtmlRequestBuilder AddDocument(Action<DocumentBuilder> action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));

        action(new DocumentBuilder(this.Request.Content, v => this.Request.ContainsMarkdown = v));

        return this;
    }

    /// <summary>
    /// Asynchronously configures the HTML or Markdown document content. Use when loading content from streams or files.
    /// </summary>
    /// <param name="asyncAction">Async configuration action for setting document content.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public HtmlRequestBuilder AddAsyncDocument(Func<DocumentBuilder, Task> asyncAction)
    {
        if (asyncAction == null) throw new ArgumentNullException(nameof(asyncAction));

        this.BuildTasks.Add(
            asyncAction(new DocumentBuilder(this.Request.Content, v => this.Request.ContainsMarkdown = v)));

        return this;
    }
}