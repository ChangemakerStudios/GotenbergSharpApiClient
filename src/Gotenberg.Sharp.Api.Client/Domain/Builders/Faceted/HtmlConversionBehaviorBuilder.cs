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

using Newtonsoft.Json.Linq;

namespace Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;

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
    ///     Sets extra HTTP headers that Chromium will send when loading the HTML
    /// </summary>
    /// <param name="headerName"></param>
    /// <param name="headerValue"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="JsonReaderException"></exception>
    public HtmlConversionBehaviorBuilder AddAdditionalHeaders(string headerName, string headerValue)
    {
        var header = string.Format("{0}{2}{1}", "{", "}", $"{'"'}{headerName}{'"'} : {'"'}{headerValue}{'"'}");

        return AddAdditionalHeaders(JObject.Parse(header));
    }

    /// <summary>
    ///     Sets extra HTTP headers that Chromium will send when loading the HTML
    /// </summary>
    /// <param name="extraHeaders"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
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
    ///     Sets the document metadata.
    ///     Not all metadata are writable. Consider taking a look at https://exiftool.org/TagNames/XMP.html#pdf for an
    ///     (exhaustive?) list of available metadata.
    /// </summary>
    /// <param name="dictionary"></param>
    /// <returns></returns>
    public HtmlConversionBehaviorBuilder SetMetadata(IDictionary<string, object> dictionary)
    {
        SetMetadata(JObject.FromObject(dictionary));

        return this;
    }

    /// <summary>
    ///     Sets the document metadata.
    ///     Not all metadata are writable. Consider taking a look at https://exiftool.org/TagNames/XMP.html#pdf for an
    ///     (exhaustive?) list of available metadata.
    /// </summary>
    /// <param name="metadata"></param>
    /// <returns></returns>
    public HtmlConversionBehaviorBuilder SetMetadata(JObject metadata)
    {
        if (metadata == null)
        {
            throw new InvalidOperationException("metadata is null");
        }

        _htmlConversionBehaviors.MetaData = metadata;

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
    ///     Sets the format of the resulting PDF document
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public HtmlConversionBehaviorBuilder SetPdfFormat(ConversionPdfFormats format)
    {
        if (format == default)
        {
            throw new InvalidOperationException("Invalid PDF format specified");
        }

        _htmlConversionBehaviors.PdfFormat = format;

        return this;
    }

    /// <summary>
    ///     This tells gotenberg to enable Universal Access for the resulting PDF.
    /// </summary>
    public HtmlConversionBehaviorBuilder SetPdfUa(bool enablePdfUa = true)
    {
        _htmlConversionBehaviors.EnablePdfUa = enablePdfUa;

        return this;
    }
}