﻿using Gotenberg.Sharp.API.Client.Extensions;
using Gotenberg.Sharp.API.Client.Infrastructure;

using System.Collections.Generic;
using System.Net.Http;

namespace Gotenberg.Sharp.API.Client.Domain.Requests.Facets
{
    /// <summary>
    /// All endpoints accept form fields for each property
    /// </summary>
    public sealed class RequestConfig : IConvertToHttpContent
    {
        #region Basic settings

        /// <summary>
        ///     If provided, the API will return a pdf containing the pages in the specified range.
        /// </summary>
        /// <remarks>
        ///     The format is the same as the one from the print options of Google Chrome, e.g. 1-5,8,11-13.
        ///     This may move...
        /// </remarks>
        public string PageRanges { get; set; }

        /// <summary>
        /// If provided, the API will return the resulting PDF file with the given filename. Otherwise a random filename is used.
        /// </summary>
        /// <remarks>
        /// Attention: this feature does not work if the form field webHookURL is given.
        /// </remarks>
        // Not sure this is useful with the way this client is used, although.. maybe Webhook requests honor it?
        public string ResultFileName { get; set; }

        /// <summary>
        ///     If provided, the API will send the resulting PDF file in a POST request with the application/pdf Content-Type to given URL.
        ///     Requests to the API complete before the conversions complete. For web hook configured requests,
        ///     call FireWebhookAndForgetAsync on the client which returns nothing.
        /// </summary>
        /// <remarks>All request types support web hooks</remarks>
        public Webhook Webhook { get; set; }

        #endregion

        #region ToHttpContent

        /// <summary>
        /// Converts the instance to a collection of http content items
        /// </summary>
        /// <returns></returns>
        // ReSharper disable once MethodTooLong
        public IEnumerable<HttpContent> ToHttpContent()
        {
            if (this.PageRanges.IsSet())
            {
                yield return RequestBase.CreateFormDataItem(this.PageRanges, Constants.Gotenberg.Chromium.Shared.Dims.PageRanges);
            }

            if (this.ResultFileName.IsSet())
            {
                yield return RequestBase.CreateFormDataItem(this.ResultFileName, Constants.Gotenberg.SharedFormFieldNames.OutputFileName);
            }
        }

        #endregion
    }
}