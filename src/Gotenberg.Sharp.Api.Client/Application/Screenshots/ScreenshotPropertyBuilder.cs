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

using Gotenberg.Sharp.API.Client.Domain.ValueObjects;

namespace Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;

/// <summary>
/// Configures screenshot-specific properties (device dimensions, format, quality).
/// </summary>
public sealed class ScreenshotPropertyBuilder
{
    private readonly ScreenshotProperties _properties;

    internal ScreenshotPropertyBuilder(ScreenshotProperties properties)
    {
        _properties = properties;
    }

    public ScreenshotPropertyBuilder SetWidth(ScreenDimension width)
    {
        _properties.Width = width ?? throw new ArgumentNullException(nameof(width));
        return this;
    }

    public ScreenshotPropertyBuilder SetWidth(int pixels)
    {
        return SetWidth(ScreenDimension.Create(pixels));
    }

    public ScreenshotPropertyBuilder SetHeight(ScreenDimension height)
    {
        _properties.Height = height ?? throw new ArgumentNullException(nameof(height));
        return this;
    }

    public ScreenshotPropertyBuilder SetHeight(int pixels)
    {
        return SetHeight(ScreenDimension.Create(pixels));
    }

    public ScreenshotPropertyBuilder SetSize(int width, int height)
    {
        return SetWidth(width).SetHeight(height);
    }

    public ScreenshotPropertyBuilder SetClip(bool clip = true)
    {
        _properties.Clip = clip;
        return this;
    }

    public ScreenshotPropertyBuilder SetFormat(ScreenshotFormat format)
    {
        _properties.Format = format;
        return this;
    }

    public ScreenshotPropertyBuilder SetQuality(CompressionQuality quality)
    {
        _properties.Quality = quality ?? throw new ArgumentNullException(nameof(quality));
        return this;
    }

    public ScreenshotPropertyBuilder SetQuality(int quality)
    {
        return SetQuality(CompressionQuality.Create(quality));
    }

    public ScreenshotPropertyBuilder SetOmitBackground(bool omit = true)
    {
        _properties.OmitBackground = omit;
        return this;
    }

    public ScreenshotPropertyBuilder SetOptimizeForSpeed(bool optimize = true)
    {
        _properties.OptimizeForSpeed = optimize;
        return this;
    }
}
