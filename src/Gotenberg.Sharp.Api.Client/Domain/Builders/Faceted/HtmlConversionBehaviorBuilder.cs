// Copyright 2019-2025 Chris Mohan, Jaben Cargman
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

using Newtonsoft.Json.Linq;

namespace Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;

/// <summary>
/// Configures Chromium rendering behaviors for HTML and URL to PDF conversions.
/// Includes settings for wait delays, HTTP headers, cookies, media emulation, and error handling.
/// </summary>
/// <remarks>
/// PDF output options (PDF/A, PDF/UA, flatten, tagged PDF, metadata) have moved to
/// <see cref="PdfOutputOptionsBuilder"/> which is available via <c>SetPdfOutputOptions()</c> on all builders.
/// </remarks>
public sealed class HtmlConversionBehaviorBuilder
{
    private readonly HtmlConversionBehaviors _htmlConversionBehaviors;

    internal HtmlConversionBehaviorBuilder(HtmlConversionBehaviors htmlConversionBehaviors)
    {
        _htmlConversionBehaviors = htmlConversionBehaviors;
    }

    /// <summary>
    ///     Sets the wait duration when loading an HTML document before converting it to PDF
    /// </summary>
    /// <param name="seconds"></param>
    /// <returns></returns>
    /// <remarks>Prefer <see cref="SetBrowserWaitExpression" /> over waitDelay.</remarks>
    public HtmlConversionBehaviorBuilder SetBrowserWaitDelay(int seconds)
    {
        _htmlConversionBehaviors.WaitDelay = $"{seconds}s";

        return this;
    }

    /// <summary>
    ///     Sets a java-script expression to wait before converting an HTML document to PDF until it returns true
    /// </summary>
    /// <param name="expression">The expression to set</param>
    /// <returns></returns>
    /// <remarks>Prefer this option over waitDelay.</remarks>
    /// <example>SetBrowserWaitExpression("window.status === 'ready'")</example>
    /// <exception cref="InvalidOperationException"></exception>
    public HtmlConversionBehaviorBuilder SetBrowserWaitExpression(string expression)
    {
        if (expression.IsNotSet())
        {
            throw new InvalidOperationException("expression is not set");
        }

        _htmlConversionBehaviors.WaitForExpression = expression;

        return this;
    }

    /// <summary>
    ///     Overrides the default User-Agent extraHeaders
    /// </summary>
    /// <param name="userAgent"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    [Obsolete("Deprecated in Gotenberg v8+")]
    public HtmlConversionBehaviorBuilder SetUserAgent(string userAgent)
    {
        if (userAgent.IsNotSet())
        {
            throw new InvalidOperationException("headerName is not set");
        }

        _htmlConversionBehaviors.UserAgent = userAgent;

        return this;
    }

    /// <summary>
    /// Adds custom HTTP headers that Chromium will send when loading the HTML. Useful for authentication tokens,
    /// custom request identification, or triggering specific server behavior.
    /// </summary>
    /// <param name="headerName">HTTP header name.</param>
    /// <param name="headerValue">HTTP header value.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when header name or value is invalid.</exception>
    public HtmlConversionBehaviorBuilder AddAdditionalHeaders(string headerName, string headerValue)
    {
        var header = string.Format("{0}{2}{1}", "{", "}", $"{'"'}{headerName}{'"'} : {'"'}{headerValue}{'"'}");

        return AddAdditionalHeaders(JObject.Parse(header));
    }

    /// <summary>
    /// Adds multiple custom HTTP headers as a JSON object. Useful for adding several headers at once.
    /// </summary>
    /// <param name="extraHeaders">JSON object containing header name-value pairs.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when extraHeaders is null.</exception>
    public HtmlConversionBehaviorBuilder AddAdditionalHeaders(JObject extraHeaders)
    {
        if (extraHeaders == null)
        {
            throw new InvalidOperationException("extraHeaders is null");
        }

        _htmlConversionBehaviors.ExtraHeaders = extraHeaders;

        return this;
    }

    /// <summary>
    ///     Adds a cookie to store in the Chromium cookie jar.
    /// </summary>
    /// <param name="cookie">The cookie to add</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HtmlConversionBehaviorBuilder AddCookie(Cookie cookie)
    {
        if (cookie == null)
        {
            throw new ArgumentNullException(nameof(cookie));
        }

        _htmlConversionBehaviors.Cookies ??= new List<Cookie>();

        Cookie.Validate(cookie);

        _htmlConversionBehaviors.Cookies.Add(cookie);

        return this;
    }

    /// <summary>
    ///     Tells gotenberg to return a 409 response if there are exceptions in the Chromium console.
    /// </summary>
    /// <returns></returns>
    public HtmlConversionBehaviorBuilder FailOnConsoleExceptions()
    {
        _htmlConversionBehaviors.FailOnConsoleExceptions = true;

        return this;
    }

    /// <summary>
    ///     Configures gotenberg to emulate html loading as screen. By default, it loads it as print
    /// </summary>
    /// <returns></returns>
    public HtmlConversionBehaviorBuilder EmulateAsScreen()
    {
        _htmlConversionBehaviors.EmulatedMediaType = "screen";

        return this;
    }

    /// <summary>
    ///     Gotenberg 8+ ONLY: Configures gotenberg to not wait for Chromium network to be idle.
    /// </summary>
    /// <returns></returns>
    public HtmlConversionBehaviorBuilder SkipNetworkIdleEvent()
    {
        _htmlConversionBehaviors.SkipNetworkIdleEvent = true;

        return this;
    }

    /// <summary>
    /// Sets a CSS selector to wait for before conversion.
    /// Chromium will delay conversion until the specified element appears in the DOM.
    /// </summary>
    /// <param name="selector">A validated CSS selector.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public HtmlConversionBehaviorBuilder SetWaitForSelector(CssSelector selector)
    {
        _htmlConversionBehaviors.WaitForSelector = selector ?? throw new ArgumentNullException(nameof(selector));

        return this;
    }

    /// <summary>
    /// Sets a CSS selector to wait for before conversion.
    /// Chromium will delay conversion until the specified element appears in the DOM.
    /// </summary>
    /// <param name="selector">A CSS selector string (e.g., "#content", ".loaded").</param>
    /// <returns>The builder instance for method chaining.</returns>
    public HtmlConversionBehaviorBuilder SetWaitForSelector(string selector)
    {
        return SetWaitForSelector(CssSelector.Create(selector));
    }

    /// <summary>
    /// Adds a CSS media feature override for Chromium rendering.
    /// </summary>
    /// <param name="feature">A validated emulated media feature.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public HtmlConversionBehaviorBuilder AddEmulatedMediaFeature(EmulatedMediaFeature feature)
    {
        if (feature == null) throw new ArgumentNullException(nameof(feature));

        _htmlConversionBehaviors.EmulatedMediaFeatures ??= new List<EmulatedMediaFeature>();
        _htmlConversionBehaviors.EmulatedMediaFeatures.Add(feature);

        return this;
    }

    /// <summary>
    /// Adds a CSS media feature override by name and value.
    /// </summary>
    /// <param name="name">CSS media feature name (e.g., "prefers-color-scheme").</param>
    /// <param name="value">CSS media feature value (e.g., "dark").</param>
    /// <returns>The builder instance for method chaining.</returns>
    public HtmlConversionBehaviorBuilder AddEmulatedMediaFeature(string name, string value)
    {
        return AddEmulatedMediaFeature(EmulatedMediaFeature.Create(name, value));
    }

    /// <summary>
    /// Sets HTTP status codes that trigger a 409 Conflict when the main page returns them.
    /// </summary>
    /// <param name="statusCodes">Validated HTTP status codes.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public HtmlConversionBehaviorBuilder SetFailOnHttpStatusCodes(IEnumerable<HttpStatusCode> statusCodes)
    {
        if (statusCodes == null) throw new ArgumentNullException(nameof(statusCodes));

        _htmlConversionBehaviors.FailOnHttpStatusCodes = statusCodes.ToList();

        return this;
    }

    /// <summary>
    /// Sets HTTP status codes that trigger a 409 Conflict when the main page returns them.
    /// </summary>
    /// <param name="statusCodes">Raw HTTP status code integers (must be 100-599).</param>
    /// <returns>The builder instance for method chaining.</returns>
    public HtmlConversionBehaviorBuilder SetFailOnHttpStatusCodes(params int[] statusCodes)
    {
        return SetFailOnHttpStatusCodes(statusCodes.Select(HttpStatusCode.Create));
    }

    /// <summary>
    /// Sets HTTP status codes that trigger a failure when page resources return them.
    /// </summary>
    /// <param name="statusCodes">Validated HTTP status codes.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public HtmlConversionBehaviorBuilder SetFailOnResourceHttpStatusCodes(IEnumerable<HttpStatusCode> statusCodes)
    {
        if (statusCodes == null) throw new ArgumentNullException(nameof(statusCodes));

        _htmlConversionBehaviors.FailOnResourceHttpStatusCodes = statusCodes.ToList();

        return this;
    }

    /// <summary>
    /// Sets HTTP status codes that trigger a failure when page resources return them.
    /// </summary>
    /// <param name="statusCodes">Raw HTTP status code integers (must be 100-599).</param>
    /// <returns>The builder instance for method chaining.</returns>
    public HtmlConversionBehaviorBuilder SetFailOnResourceHttpStatusCodes(params int[] statusCodes)
    {
        return SetFailOnResourceHttpStatusCodes(statusCodes.Select(HttpStatusCode.Create));
    }

    /// <summary>
    /// Adds a domain to exclude from HTTP status code checks on resources.
    /// </summary>
    /// <param name="domain">A validated domain name.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public HtmlConversionBehaviorBuilder AddIgnoreResourceHttpStatusDomain(DomainName domain)
    {
        if (domain == null) throw new ArgumentNullException(nameof(domain));

        _htmlConversionBehaviors.IgnoreResourceHttpStatusDomains ??= new List<DomainName>();
        _htmlConversionBehaviors.IgnoreResourceHttpStatusDomains.Add(domain);

        return this;
    }

    /// <summary>
    /// Adds a domain to exclude from HTTP status code checks on resources.
    /// </summary>
    /// <param name="domain">A domain string (e.g., "cdn.example.com").</param>
    /// <returns>The builder instance for method chaining.</returns>
    public HtmlConversionBehaviorBuilder AddIgnoreResourceHttpStatusDomain(string domain)
    {
        return AddIgnoreResourceHttpStatusDomain(DomainName.Create(domain));
    }

    /// <summary>
    /// Adds multiple domains to exclude from HTTP status code checks on resources.
    /// </summary>
    /// <param name="domains">Domain strings to exclude.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public HtmlConversionBehaviorBuilder AddIgnoreResourceHttpStatusDomains(params string[] domains)
    {
        foreach (var domain in domains)
        {
            AddIgnoreResourceHttpStatusDomain(domain);
        }

        return this;
    }

    /// <summary>
    /// Tells Gotenberg to return a 409 Conflict if any resource fails to load due to network errors.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    public HtmlConversionBehaviorBuilder FailOnResourceLoadingFailed()
    {
        _htmlConversionBehaviors.FailOnResourceLoadingFailed = true;

        return this;
    }

    /// <summary>
    ///     Sets the format of the resulting PDF document.
    /// </summary>
    [Obsolete("Use SetPdfOutputOptions(o => o.SetPdfFormat(...)) on the builder instead")]
    public HtmlConversionBehaviorBuilder SetPdfFormat(ConversionPdfFormats format)
    {
        return this;
    }

    /// <summary>
    ///     This tells gotenberg to enable Universal Access for the resulting PDF.
    /// </summary>
    [Obsolete("Use SetPdfOutputOptions(o => o.SetPdfUa()) on the builder instead")]
    public HtmlConversionBehaviorBuilder SetPdfUa(bool enablePdfUa = true)
    {
        return this;
    }

    /// <summary>
    ///     This tells gotenberg to enable embeds logical structure tags for accessibility during generation.
    /// </summary>
    [Obsolete("Use SetPdfOutputOptions(o => o.SetGenerateTaggedPdf()) on the builder instead")]
    public HtmlConversionBehaviorBuilder SetGenerateTaggedPdf(bool generateTaggedPdf = true)
    {
        return this;
    }

    /// <summary>
    ///     Sets the document metadata.
    /// </summary>
    [Obsolete("Use SetPdfOutputOptions(o => o.SetMetadata(...)) on the builder instead")]
    public HtmlConversionBehaviorBuilder SetMetadata(IDictionary<string, object> dictionary)
    {
        return this;
    }

    /// <summary>
    ///     Sets the document metadata.
    /// </summary>
    [Obsolete("Use SetPdfOutputOptions(o => o.SetMetadata(...)) on the builder instead")]
    public HtmlConversionBehaviorBuilder SetMetadata(JObject metadata)
    {
        return this;
    }
}
