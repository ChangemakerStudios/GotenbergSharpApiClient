using System;

using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;
using Gotenberg.Sharp.API.Client.Extensions;

using JetBrains.Annotations;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;

public sealed class HtmlConversionBehaviorBuilder : BaseFacetedBuilder<ChromeRequest>
{
    public HtmlConversionBehaviorBuilder(ChromeRequest request)
    {
        this.Request = request ?? throw new ArgumentNullException(nameof(request));
        Request.ConversionBehaviors ??= new HtmlConversionBehaviors();
    }

    /// <summary>
    ///  Sets the wait duration when loading an HTML document before converting it to PDF
    /// </summary>
    /// <param name="seconds"></param>
    /// <returns></returns>
    /// <remarks>Prefer <see cref="SetBrowserWaitExpression"/> over waitDelay.</remarks> 
    [PublicAPI]
    public HtmlConversionBehaviorBuilder SetBrowserWaitDelay(int seconds)
    {
        this.Request.ConversionBehaviors.WaitDelay = $"{seconds}s";

        return this;
    }

    /// <summary>
    /// Sets a java-script expression to wait before converting an HTML document to PDF until it returns true
    /// </summary>
    /// <param name="expression">The expression to set</param>
    /// <returns></returns>
    /// <remarks>Prefer this option over waitDelay.</remarks>
    /// <example>SetBrowserWaitExpression("window.status === 'ready'")</example>
    /// <exception cref="InvalidOperationException"></exception>
    [PublicAPI]
    public HtmlConversionBehaviorBuilder SetBrowserWaitExpression(string expression)
    {
        if (expression.IsNotSet()) throw new InvalidOperationException("expression is not set");
        this.Request.ConversionBehaviors.WaitForExpression = expression;

        return this;
    }

    /// <summary>
    ///  Overrides the default User-Agent extraHeaders
    /// </summary>
    /// <param name="userAgent"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    [PublicAPI]
    public HtmlConversionBehaviorBuilder SetUserAgent(string userAgent)
    {
        if (userAgent.IsNotSet()) throw new InvalidOperationException("headerName is not set");
        this.Request.ConversionBehaviors.UserAgent = userAgent;

        return this;
    }

    /// <summary>
    /// Sets extra HTTP headers that Chromium will send when loading the HTML
    /// </summary>
    /// <param name="headerName"></param>
    /// <param name="headerValue"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="JsonReaderException"></exception>
    [PublicAPI]
    public HtmlConversionBehaviorBuilder AddAdditionalHeaders(string headerName, string headerValue)
    {
        var header = string.Format("{0}{2}{1}", "{", "}", $"{'"'}{headerName}{'"'} : {'"'}{headerValue}{'"'}");

        return AddAdditionalHeaders(JObject.Parse(header));
    }

    /// <summary>
    ///   Sets extra HTTP headers that Chromium will send when loading the HTML
    /// </summary>
    /// <param name="extraHeaders"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    [PublicAPI]
    public HtmlConversionBehaviorBuilder AddAdditionalHeaders(JObject extraHeaders)
    {
        if (extraHeaders == null) throw new InvalidOperationException("headerValue is null");

        this.Request.ConversionBehaviors.ExtraHeaders = extraHeaders;

        return this;
    }

    /// <summary>
    ///  Tells gotenberg to return a 409 response if there are exceptions in the Chromium console. 
    /// </summary>
    /// <returns></returns>
    [PublicAPI]
    public HtmlConversionBehaviorBuilder FailOnConsoleExceptions()
    {
        this.Request.ConversionBehaviors.FailOnConsoleExceptions = true;

        return this;
    }

    /// <summary>
    ///  Configures gotenberg to emulate html loading as screen. By default it loads it as print
    /// </summary>
    /// <returns></returns>
    [PublicAPI]
    public HtmlConversionBehaviorBuilder EmulateAsScreen()
    {
        this.Request.ConversionBehaviors.EmulatedMediaType = "screen";

        return this;
    }

    /// <summary>
    /// Sets the format of the resulting PDF document
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    [PublicAPI]
    public HtmlConversionBehaviorBuilder SetPdfFormat(PdfFormats format)
    {
        if(format == default) throw new InvalidOperationException("Invalid PDF format specified");

        this.Request.ConversionBehaviors.PdfFormat = format;

        return this;
    }
 
}