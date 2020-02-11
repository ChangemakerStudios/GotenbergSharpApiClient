using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using Gotenberg.Sharp.API.Client.Extensions;
using Gotenberg.Sharp.API.Client.Infrastructure;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    /// <summary>
    /// All endpoints accept form fields for each property
    /// </summary>
    public sealed class RequestConfig : IConvertToHttpContent
    {
        Uri _webHook;
        float? _timeOut;
        const string _dispositionType = Constants.Http.Disposition.Types.FormData;
      
        #region Basic settings

        /// <summary>
        ///     If provided, the API will wait the given seconds before it considers the
        ///     conversion unsuccessful and return a 504 HTTP code. This overrides the
        ///     the container's DEFAULT_WAIT_TIMEOUT environment variable
        /// </summary>
       
        public float? TimeOut
        {
            get => _timeOut;
            set => _timeOut = value < 1800 ? value: throw new InvalidDataException($"{nameof(TimeOut)} must be less than 1800");
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
        /// If provided, the API will return the resulting PDF file with the given filename. Otherwise a random filename is used.
        /// </summary>
        /// <remarks>
        /// Attention: this feature does not work if the form field webHookURL is given.
        /// </remarks>
        // Not sure this is useful with the way this client is used, although.. maybe Webhook requests honor it?
        public string ResultFileName { get; set; }        
    
        /// <summary>
        /// If set the Gotenberg API will send the resulting PDF file in a POST with
        /// the application-pdf content type to the given url. Requests to the API
        /// complete before the conversion is performed.
        /// </summary>
        public Uri WebHook
        {
            get => _webHook;
            [UsedImplicitly] set => _webHook = value?.IsAbsoluteUri ?? false ? value : throw new ArgumentException("WebHook url must be absolute");
        }

        /// <summary>
        ///  By default, the API will wait 10 seconds before it considers the sending of the resulting PDF to be unsuccessful.
        ///  On a per request basis, this property can override the container environment variable, DEFAULT_WEBHOOK_URL_TIMEOUT
        /// </summary>
        public float? WebHookTimeOut { get; set; }

        #endregion

        #region ToHttpContent
        
        /// <summary>
        /// Converts the instance to a collection of http content items
        /// </summary>
        /// <returns></returns>
        public IEnumerable<HttpContent> ToHttpContent()
        {
            if (this.TimeOut.HasValue)
            {
                yield return CreateItem(this.TimeOut,  Constants.Gotenberg.FormFieldNames.WaitTimeout);
            }

            if (this.WebHook != null)
            {
                yield return CreateItem(this.WebHook,  Constants.Gotenberg.FormFieldNames.WebhookURL);

                if (this.WebHookTimeOut.HasValue)
                {
                    yield return CreateItem(this.WebHookTimeOut,  Constants.Gotenberg.FormFieldNames.WebhookTimeout);
                }
            }

            if (this.ResultFileName.IsSet())
            {
                yield return CreateItem(this.ResultFileName,  Constants.Gotenberg.FormFieldNames.ResultFilename);
            }

            if (ChromeRpccBufferSize.HasValue)
            {
                yield return CreateItem(this.ChromeRpccBufferSize,  Constants.Gotenberg.FormFieldNames.ChromeRpccBufferSize);
            }
        }

        static StringContent CreateItem<T>(T value, string fieldName)
        {
            var item = new StringContent(value.ToString());
            item.Headers.ContentDisposition = new ContentDispositionHeaderValue(_dispositionType) { Name = fieldName };
            return item;
        }
        
        #endregion
    }
}
