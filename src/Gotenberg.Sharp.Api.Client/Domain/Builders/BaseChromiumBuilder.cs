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

namespace Gotenberg.Sharp.API.Client.Domain.Builders;

/// <summary>
/// Base class for builders that use Gotenberg's Chromium module for HTML and URL to PDF conversions.
/// Provides configuration for page properties, assets, and Chromium rendering behaviors.
/// </summary>
/// <typeparam name="TRequest">The type of Chromium-based request being built.</typeparam>
/// <typeparam name="TBuilder">The concrete builder type for fluent interface chaining.</typeparam>
public abstract class BaseChromiumBuilder<TRequest, TBuilder>(TRequest request)
    : BaseBuilder<TRequest, TBuilder>(request)
    where TRequest : ChromeRequest
    where TBuilder : BaseChromiumBuilder<TRequest, TBuilder>
{
    [Obsolete("Use WithPageProperties")]
    public TBuilder WithDimensions(Action<PagePropertyBuilder> action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));

        var builder = new PagePropertyBuilder(this.Request.PageProperties);

        action(builder);

        this.Request.PageProperties = builder.GetPageProperties();

        return (TBuilder)this;
    }

    [Obsolete("Use WithPageProperties")]
    public TBuilder WithDimensions(PageProperties pageProperties)
    {
        this.Request.PageProperties = pageProperties ?? throw new ArgumentNullException(nameof(pageProperties));
        return (TBuilder)this;
    }

    /// <summary>
    /// Configures page properties including size, margins, orientation, scale, and rendering options.
    /// </summary>
    /// <param name="action">Configuration action for page properties.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public TBuilder WithPageProperties(Action<PagePropertyBuilder> action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));

        var builder = new PagePropertyBuilder(this.Request.PageProperties);

        action(builder);

        this.Request.PageProperties = builder.GetPageProperties();

        return (TBuilder)this;
    }

    /// <summary>
    /// Sets pre-configured page properties.
    /// </summary>
    /// <param name="pageProperties">Pre-configured PageProperties instance.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public TBuilder WithPageProperties(PageProperties pageProperties)
    {
        this.Request.PageProperties = pageProperties ?? throw new ArgumentNullException(nameof(pageProperties));

        return (TBuilder)this;
    }

    /// <summary>
    /// Adds embedded resources (images, fonts, CSS, JavaScript files) referenced by the HTML content.
    /// The asset filename must match how it's referenced in your HTML.
    /// </summary>
    /// <param name="action">Configuration action for adding assets.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public TBuilder WithAssets(Action<AssetBuilder> action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        action(new AssetBuilder(this.Request.Assets ??= new AssetDictionary()));
        return (TBuilder)this;
    }

    /// <summary>
    /// Asynchronously adds embedded resources. Use when loading assets from streams or files.
    /// </summary>
    /// <param name="asyncAction">Async configuration action for adding assets.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public TBuilder WithAsyncAssets(Func<AssetBuilder, Task> asyncAction)
    {
        if (asyncAction == null) throw new ArgumentNullException(nameof(asyncAction));
        this.BuildTasks.Add(
            asyncAction(new AssetBuilder(this.Request.Assets ??= new AssetDictionary())));
        return (TBuilder)this;
    }

    /// <summary>
    /// Configures Chromium rendering behaviors including wait delays, cookies, HTTP headers, metadata, PDF format, and accessibility options.
    /// </summary>
    /// <param name="action">Configuration action for conversion behaviors.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public TBuilder SetConversionBehaviors(Action<HtmlConversionBehaviorBuilder> action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        action(new HtmlConversionBehaviorBuilder(this.Request.ConversionBehaviors));
        return (TBuilder)this;
    }

    /// <summary>
    /// Sets pre-configured Chromium conversion behaviors.
    /// </summary>
    /// <param name="behaviors">Pre-configured HtmlConversionBehaviors instance.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public TBuilder SetConversionBehaviors(HtmlConversionBehaviors behaviors)
    {
        this.Request.ConversionBehaviors =
            behaviors ?? throw new ArgumentNullException(nameof(behaviors));
        return (TBuilder)this;
    }
}