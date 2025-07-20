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

using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;

namespace Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;

public sealed class ScreenshotBehaviorBuilder
{
    private readonly ScreenshotBehaviors _screenshotBehaviors;

    internal ScreenshotBehaviorBuilder(ScreenshotBehaviors screenshotBehaviors)
    {
        this._screenshotBehaviors = screenshotBehaviors;
    }

    /// <summary>
    /// Sets the device screen width in pixels.
    /// </summary>
    /// <param name="width">The width in pixels</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public ScreenshotBehaviorBuilder SetWidth(int width)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width), "Width must be greater than 0");

        this._screenshotBehaviors.Width = width;
        return this;
    }

    /// <summary>
    /// Sets the device screen height in pixels.
    /// </summary>
    /// <param name="height">The height in pixels</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public ScreenshotBehaviorBuilder SetHeight(int height)
    {
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height), "Height must be greater than 0");

        this._screenshotBehaviors.Height = height;
        return this;
    }

    /// <summary>
    /// Sets both width and height of the device screen in pixels.
    /// </summary>
    /// <param name="width">The width in pixels</param>
    /// <param name="height">The height in pixels</param>
    /// <returns></returns>
    public ScreenshotBehaviorBuilder SetDimensions(int width, int height)
    {
        return this.SetWidth(width).SetHeight(height);
    }

    /// <summary>
    /// Define whether to clip the screenshot according to the device dimensions.
    /// </summary>
    /// <param name="clip">True to clip according to device dimensions</param>
    /// <returns></returns>
    public ScreenshotBehaviorBuilder SetClip(bool clip = true)
    {
        this._screenshotBehaviors.Clip = clip;
        return this;
    }

    /// <summary>
    /// Sets the image compression format.
    /// </summary>
    /// <param name="format">The image format</param>
    /// <returns></returns>
    public ScreenshotBehaviorBuilder SetFormat(ScreenshotImageFormat format)
    {
        this._screenshotBehaviors.Format = format;
        return this;
    }

    /// <summary>
    /// Sets the compression quality from range 0 to 100 (jpeg only).
    /// </summary>
    /// <param name="quality">Quality value from 0 to 100</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public ScreenshotBehaviorBuilder SetQuality(int quality)
    {
        if (quality < 0 || quality > 100) 
            throw new ArgumentOutOfRangeException(nameof(quality), "Quality must be between 0 and 100");

        this._screenshotBehaviors.Quality = quality;
        return this;
    }

    /// <summary>
    /// Hide the default white background and allow generating screenshots with transparency.
    /// </summary>
    /// <param name="omitBackground">True to omit background</param>
    /// <returns></returns>
    public ScreenshotBehaviorBuilder SetOmitBackground(bool omitBackground = true)
    {
        this._screenshotBehaviors.OmitBackground = omitBackground;
        return this;
    }

    /// <summary>
    /// Define whether to optimize image encoding for speed, not for resulting size.
    /// </summary>
    /// <param name="optimizeForSpeed">True to optimize for speed</param>
    /// <returns></returns>
    public ScreenshotBehaviorBuilder SetOptimizeForSpeed(bool optimizeForSpeed = true)
    {
        this._screenshotBehaviors.OptimizeForSpeed = optimizeForSpeed;
        return this;
    }

    /// <summary>
    /// Do not wait for chromium network idle event before taking screenshot.
    /// </summary>
    /// <param name="skipNetworkIdleEvent">True to skip waiting for network idle</param>
    /// <returns></returns>
    public ScreenshotBehaviorBuilder SkipNetworkIdleEvent(bool skipNetworkIdleEvent = true)
    {
        this._screenshotBehaviors.SkipNetworkIdleEvent = skipNetworkIdleEvent;
        return this;
    }
}
