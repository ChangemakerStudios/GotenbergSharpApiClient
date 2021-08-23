﻿using Gotenberg.Sharp.API.Client.Extensions;
using Gotenberg.Sharp.API.Client.Infrastructure;

using JetBrains.Annotations;

using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Gotenberg.Sharp.API.Client.Domain.Requests.Facets
{
    /// <summary>
    /// All endpoints accept form fields for each property
    /// </summary>
    public sealed class RequestConfig : IConvertToHttpContent
    {
        float? _timeOut;

        [PublicAPI] public static readonly int MaxChromeRpccBufferSize = 104857600;

        [PublicAPI] public const int DefaultChromeRpccBufferSize = 1048576;

        #region Basic settings

        /// <summary>
        ///     If provided, the API will wait the given seconds before it considers the
        ///     conversion unsuccessful and return a 504 HTTP code. This overrides the
        ///     the container's DEFAULT_WAIT_TIMEOUT environment variable
        /// </summary>
        public float? TimeOut
        {
            get => _timeOut;
            set => _timeOut = value < 1800
                ? value
                : throw new InvalidDataException($"{nameof(TimeOut)} must be less than 1800");
        }

        /// <summary>
        ///     The default Google Chrome rpcc buffer size may also be overridden per request
        ///     See the rpcc buffer size section. https://thecodingmachine.github.io/gotenberg/#html.rpcc_buffer_size
        ///     Sample value: 1048576 for 1 MB. The hard limit is 100 MB and is defined by Google Chrome itself.
        ///     Using this property will override the container environment variable, DEFAULT_GOOGLE_CHROME_RPCC_BUFFER_SIZE for a given request
        /// </summary>
        /// <remarks>
        ///     Use If/when the Gotenberg API returns a 400 response with the message "increase the Google Chrome rpcc buffer size"
        /// </remarks>
        public int? ChromeRpccBufferSize { get; set; }

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
            if (this.TimeOut.HasValue)
            {
                yield return RequestBase.CreateFormDataItem(this.TimeOut, Constants.Gotenberg.FormFieldNames.WaitTimeout);
            }

            foreach (var httpContent in TryAddWebhookHeaders()) yield return httpContent;

            if (this.PageRanges.IsSet())
            {
                yield return RequestBase.CreateFormDataItem(this.PageRanges, Constants.Gotenberg.FormFieldNames.PageRanges);
            }

            if (this.ResultFileName.IsSet())
            {
                yield return RequestBase.CreateFormDataItem(this.ResultFileName, Constants.Gotenberg.FormFieldNames.ResultFilename);
            }

            /*
            if (ChromeRpccBufferSize.HasValue)
            {
                yield return CreateItem(this.ChromeRpccBufferSize,
                    Constants.Gotenberg.FormFieldNames.ChromeRpccBufferSize);
            }*/
        }

        IEnumerable<HttpContent> TryAddWebhookHeaders()
        {
            if (this.Webhook?.TargetUrl != null)
            {
                yield return RequestBase.CreateFormDataItem(this.Webhook.TargetUrl, Constants.Gotenberg.FormFieldNames.Webhook.Url);

                if (this.Webhook.HttpMethod.IsSet())
                {
                    yield return RequestBase.CreateFormDataItem(this.Webhook.HttpMethod, Constants.Gotenberg.FormFieldNames.Webhook.HttpMethod);
                }

                if (this.Webhook.ErrorUrl != null)
                {
                    yield return RequestBase.CreateFormDataItem(this.Webhook.ErrorUrl, Constants.Gotenberg.FormFieldNames.Webhook.ErrorUrl);
                    if (this.Webhook.ErrorHttpMethod.IsSet())
                    {
                        yield return RequestBase.CreateFormDataItem(this.Webhook.ErrorHttpMethod,
                            Constants.Gotenberg.FormFieldNames.Webhook.ErrorHttpMethod);
                    }
                }
            }
        }

        #endregion
    }
}