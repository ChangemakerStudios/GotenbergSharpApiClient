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

namespace Gotenberg.Sharp.API.Client.Domain.ValueObjects;

/// <summary>
/// Represents the output image format for Chromium screenshot routes.
/// </summary>
public enum ScreenshotFormat
{
    Png,
    Jpeg,
    Webp
}

internal static class ScreenshotFormatExtensions
{
    internal static string ToFormValue(this ScreenshotFormat format) => format switch
    {
        ScreenshotFormat.Png => "png",
        ScreenshotFormat.Jpeg => "jpeg",
        ScreenshotFormat.Webp => "webp",
        _ => throw new ArgumentOutOfRangeException(nameof(format))
    };
}
