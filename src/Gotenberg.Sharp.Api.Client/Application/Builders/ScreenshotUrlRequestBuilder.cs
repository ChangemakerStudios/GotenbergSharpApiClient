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

namespace Gotenberg.Sharp.API.Client.Application.Builders;

/// <summary>
/// Builds requests for capturing screenshots of URLs using Chromium.
/// </summary>
public sealed class ScreenshotUrlRequestBuilder()
    : BaseScreenshotBuilder<ScreenshotUrlRequest, ScreenshotUrlRequestBuilder>(new ScreenshotUrlRequest())
{
    /// <summary>
    /// Sets the URL to screenshot.
    /// </summary>
    public ScreenshotUrlRequestBuilder SetUrl(string url)
    {
        return SetUrl(new Uri(url));
    }

    /// <summary>
    /// Sets the URL to screenshot.
    /// </summary>
    public ScreenshotUrlRequestBuilder SetUrl(Uri url)
    {
        this.Request.Url = url ?? throw new ArgumentNullException(nameof(url));
        return this;
    }
}
