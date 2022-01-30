using System;
using System.Collections.Generic;
using System.Net.Http;
using Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;
using Gotenberg.Sharp.API.Client.Extensions;
using Gotenberg.Sharp.API.Client.Infrastructure;
using Newtonsoft.Json.Linq;

namespace Gotenberg.Sharp.API.Client.Domain.Requests.Facets;

/// <summary>
/// The right side tabs here: https://gotenberg.dev/docs/modules/chromium#routes
/// </summary>
public class HtmlConversionBehaviors : IConvertToHttpContent
{
    /// <summary>
    /// Duration to wait when loading an HTML document before converting it to PDF
    /// </summary>
    /// <remarks>
    /// When the page relies on JavaScript for rendering, and you don't
    /// have access to the page's code, you may want to wait a certain amount
    /// of time to make sure Chromium has fully rendered the page you're trying to generate.
    /// </remarks>
    public string WaitDelay { get; set; }

    /// <summary>
    /// The JavaScript expression to wait before converting an HTML document to PDF until it returns true
    /// </summary>
    /// <remarks>Prefer this option over waitDelay</remarks>
    public string WaitForExpression { get; set; }

    /// <summary>
    /// Tells gotenberg to return a 409 response if there are exceptions in the Chromium console. 
    /// </summary>
    /// <remarks>
    /// Caution: does not work if JavaScript is disabled at the container level via --chromium-disable-javascript.
    /// </remarks>
    public bool FailOnConsoleExceptions { get; set; }

    /// <summary>
    /// The media type to emulate, either "screen" or "print" - empty means "print".
    /// Builders only provide the ability to emulate screen b/c Gotenberg emulates print by default
    /// </summary>
    public string EmulatedMediaType { get; set; }

    //TODO: does this belong here?
    /// <summary>
    /// After a Chromium conversion, the PDF engines will convert the resulting PDF to a specific format.
    /// </summary>
    public PdfFormats PdfFormat { get; set; }

    /// <summary>
    /// HTTP headers to send by Chromium while loading the HTML document (JSON format)
    /// </summary>
    public JObject ExtraHeaders { get; set; }

    /// <summary>
    ///  Adds HTTP headers to send by Chromium while loading the HTML document
    /// </summary>
    /// <param name="headerName"></param>
    /// <param name="headerValue"></param>
    public void AddExtraHeaders(string headerName, string headerValue)
    {
        if (headerName.IsNotSet()) throw new InvalidOperationException("headerName is not set");

        ExtraHeaders = new JObject(new JProperty(headerName, headerValue));
    }
    
    public IEnumerable<HttpContent> ToHttpContent()
    {
        if (WaitDelay.IsSet())
        {
            yield return RequestBase.CreateFormDataItem(this.WaitDelay,
                Constants.Gotenberg.FormFieldNames.HtmlConvertBehaviors.WaitDelay);
        }

        if (WaitForExpression.IsSet())
        {
            yield return RequestBase.CreateFormDataItem(this.WaitForExpression,
                Constants.Gotenberg.FormFieldNames.HtmlConvertBehaviors.WaitForExpression);
        }

        if (FailOnConsoleExceptions)
        {
            yield return RequestBase.CreateFormDataItem(true,
                Constants.Gotenberg.FormFieldNames.HtmlConvertBehaviors.FailOnConsoleExceptions);

        }

        if (EmulatedMediaType.IsSet())
        {
            yield return RequestBase.CreateFormDataItem(this.EmulatedMediaType,
                Constants.Gotenberg.FormFieldNames.HtmlConvertBehaviors.EmulatedMediaType);
        }


        if (PdfFormat != PdfFormats.None)
        {
            //sloppy...   
            var format = $"PDF/A-{PdfFormat.ToString().Substring(1,2)}";

            yield return RequestBase.CreateFormDataItem(format,
                Constants.Gotenberg.FormFieldNames.PdfEngines.PdfFormat);
        }

        if (ExtraHeaders != null)
        {
            yield return RequestBase.CreateFormDataItem(ExtraHeaders, 
                Constants.Gotenberg.FormFieldNames.HtmlConvertBehaviors.ExtraHttpHeaders);
        }

    }
}