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

using Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;

namespace Gotenberg.Sharp.API.Client.Domain.Requests.Facets;

/// <summary>
/// Configuration options for screenshot capture functionality.
/// https://gotenberg.dev/docs/modules/chromium#screenshot-route
/// </summary>
public class ScreenshotBehaviors : FacetBase
{
    /// <summary>
    /// The device screen width in pixels.
    /// </summary>
    [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.Screenshot.Width)]
    public int? Width { get; set; }

    /// <summary>
    /// The device screen height in pixels.
    /// </summary>
    [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.Screenshot.Height)]
    public int? Height { get; set; }

    /// <summary>
    /// Define whether to clip the screenshot according to the device dimensions.
    /// </summary>
    [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.Screenshot.Clip)]
    public bool? Clip { get; set; }

    /// <summary>
    /// The image compression format, either "png", "jpeg" or "webp".
    /// </summary>
    [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.Screenshot.Format)]
    public ScreenshotImageFormat? Format { get; set; }

    /// <summary>
    /// The compression quality from range 0 to 100 (jpeg only).
    /// </summary>
    [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.Screenshot.Quality)]
    public int? Quality { get; set; }

    /// <summary>
    /// Hide the default white background and allow generating screenshots with transparency.
    /// </summary>
    [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.Screenshot.OmitBackground)]
    public bool? OmitBackground { get; set; }

    /// <summary>
    /// Define whether to optimize image encoding for speed, not for resulting size.
    /// </summary>
    [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.Screenshot.OptimizeForSpeed)]
    public bool? OptimizeForSpeed { get; set; }

    /// <summary>
    /// Do not wait for chromium network idle event before taking screenshot. (Gotenberg v8+)
    /// </summary>
    [MultiFormHeader(Constants.Gotenberg.Chromium.Shared.Screenshot.SkipNetworkIdleEvent)]
    public bool? SkipNetworkIdleEvent { get; set; }
}
