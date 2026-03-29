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

using Gotenberg.Sharp.API.Client.Domain.Compression;

namespace Gotenberg.Sharp.API.Client.Domain.Screenshots;

/// <summary>
/// Screenshot-specific properties for Chromium screenshot routes.
/// Controls device dimensions, image format, quality, and rendering options.
/// </summary>
public class ScreenshotProperties : FacetBase
{
    [MultiFormHeader(Constants.Gotenberg.Chromium.Screenshot.Width)]
    public ScreenDimension? Width { get; set; }

    [MultiFormHeader(Constants.Gotenberg.Chromium.Screenshot.Height)]
    public ScreenDimension? Height { get; set; }

    [MultiFormHeader(Constants.Gotenberg.Chromium.Screenshot.Clip)]
    public bool? Clip { get; set; }

    [MultiFormHeader(Constants.Gotenberg.Chromium.Screenshot.Format)]
    public ScreenshotFormat? Format { get; set; }

    [MultiFormHeader(Constants.Gotenberg.Chromium.Screenshot.Quality)]
    public CompressionQuality? Quality { get; set; }

    [MultiFormHeader(Constants.Gotenberg.Chromium.Screenshot.OmitBackground)]
    public bool? OmitBackground { get; set; }

    [MultiFormHeader(Constants.Gotenberg.Chromium.Screenshot.OptimizeForSpeed)]
    public bool? OptimizeForSpeed { get; set; }
}
