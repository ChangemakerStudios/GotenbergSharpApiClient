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

using Gotenberg.Sharp.API.Client.Application.Documents;
using Gotenberg.Sharp.API.Client.Domain.Documents;

namespace Gotenberg.Sharp.API.Client.Application.Builders;

/// <summary>
/// Builds requests for capturing screenshots of HTML content using Chromium.
/// </summary>
public sealed class ScreenshotHtmlRequestBuilder()
    : BaseScreenshotBuilder<ScreenshotHtmlRequest, ScreenshotHtmlRequestBuilder>(new ScreenshotHtmlRequest())
{
    /// <summary>
    /// Configures the HTML document content for the screenshot.
    /// </summary>
    public ScreenshotHtmlRequestBuilder AddDocument(Action<DocumentBuilder> action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));

        this.Request.Content ??= new FullDocument();

        action(new DocumentBuilder(this.Request.Content, _ => { }));

        return this;
    }

    /// <summary>
    /// Configures the HTML document content asynchronously.
    /// </summary>
    public ScreenshotHtmlRequestBuilder AddAsyncDocument(Func<DocumentBuilder, Task> asyncAction)
    {
        if (asyncAction == null) throw new ArgumentNullException(nameof(asyncAction));

        this.Request.Content ??= new FullDocument();

        this.BuildTasks.Add(asyncAction(new DocumentBuilder(this.Request.Content, _ => { })));

        return this;
    }
}
