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

namespace Gotenberg.Sharp.API.Client.Domain.Requests;

/// <summary>
/// Captures a screenshot of a URL using Chromium.
/// </summary>
public sealed class ScreenshotUrlRequest : ScreenshotRequest
{
    protected override string ApiPath => Constants.Gotenberg.Chromium.ApiPaths.ScreenshotUrl;

    private Uri? _url;

    public Uri? Url
    {
        get => _url;
        set
        {
            if (value != null && !value.IsAbsoluteUri)
                throw new ArgumentException("URL must be absolute.", nameof(value));
            _url = value;
        }
    }

    protected override void Validate()
    {
        if (this.Url == null || !this.Url.IsAbsoluteUri)
            throw new InvalidOperationException("An absolute URL is required for screenshot.");

        base.Validate();
    }

    protected override IEnumerable<HttpContent> ToHttpContent()
    {
        yield return CreateFormDataItem(this.Url!, Constants.Gotenberg.Chromium.Routes.Url.RemoteUrl);

        foreach (var item in base.ToHttpContent())
            yield return item;
    }
}
