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

using Gotenberg.Sharp.API.Client.Application.HtmlBehavior;
using Gotenberg.Sharp.API.Client.Application.Requests;
using Gotenberg.Sharp.API.Client.Application.Screenshots;
using Gotenberg.Sharp.API.Client.Domain.HtmlBehavior;

namespace Gotenberg.Sharp.API.Client.Application.Builders;

/// <summary>
/// Base builder for all Chromium screenshot requests. Provides screenshot properties,
/// conversion behaviors, and asset management shared across URL and HTML screenshot builders.
/// </summary>
public abstract class BaseScreenshotBuilder<TRequest, TBuilder>(TRequest request)
    : BaseBuilder<TRequest, TBuilder>(request)
    where TRequest : ScreenshotRequest
    where TBuilder : BaseScreenshotBuilder<TRequest, TBuilder>
{
    /// <summary>
    /// Configures screenshot-specific properties (device dimensions, format, quality).
    /// </summary>
    public TBuilder WithScreenshotProperties(Action<ScreenshotPropertyBuilder> action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));

        action(new ScreenshotPropertyBuilder(this.Request.ScreenshotProperties));

        return (TBuilder)this;
    }

    /// <summary>
    /// Configures Chromium rendering behaviors (wait conditions, cookies, headers, error handling).
    /// </summary>
    public TBuilder SetConversionBehaviors(Action<HtmlConversionBehaviorBuilder> action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));

        action(new HtmlConversionBehaviorBuilder(this.Request.ConversionBehaviors));

        return (TBuilder)this;
    }

    /// <summary>
    /// Sets pre-configured conversion behaviors.
    /// </summary>
    public TBuilder SetConversionBehaviors(HtmlConversionBehaviors behaviors)
    {
        this.Request.ConversionBehaviors = behaviors ?? throw new ArgumentNullException(nameof(behaviors));

        return (TBuilder)this;
    }

    /// <summary>
    /// Adds embedded assets (images, fonts, CSS, JS) referenced by the HTML content.
    /// </summary>
    public TBuilder WithAssets(Action<AssetBuilder> action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));

        this.Request.Assets ??= new AssetDictionary();

        action(new AssetBuilder(this.Request.Assets));

        return (TBuilder)this;
    }

    /// <summary>
    /// Adds embedded assets asynchronously (e.g., from streams or files).
    /// </summary>
    public TBuilder WithAsyncAssets(Func<AssetBuilder, Task> asyncAction)
    {
        if (asyncAction == null) throw new ArgumentNullException(nameof(asyncAction));

        this.Request.Assets ??= new AssetDictionary();

        this.BuildTasks.Add(asyncAction(new AssetBuilder(this.Request.Assets)));

        return (TBuilder)this;
    }
}
