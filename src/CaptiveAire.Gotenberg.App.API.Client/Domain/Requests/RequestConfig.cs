using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using CaptiveAire.Gotenberg.App.API.Sharp.Client.Extensions;

// ReSharper disable UnusedMember.Global

namespace CaptiveAire.Gotenberg.App.API.Sharp.Client.Domain.Requests
{
    /// <summary>
    /// All endpoints accept form fields for each property
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class RequestConfig
    {
        Uri _webHook;

        /// <summary>
        ///     If provided, the API will wait the given seconds before it considers the
        ///     conversion unsuccessful and return a 504 HTTP code.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public float? TimeOut { get; set; }
    
        /// <summary>
        /// If set the Gotenberg API will send the resulting PDF file in a POST with
        /// the application-pdf content type to the given url. Requests to the API
        /// complete before the conversion is performed.
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public Uri WebHook
        {
            get => _webHook;
            set => _webHook = value?.IsAbsoluteUri ?? false ? value : throw new ArgumentException("WebHook url must be absolute");
        }

        /// <summary>
        /// If provided, the API will return the resulting PDF file with the given filename. Otherwise a random filename is used.
        /// </summary>
        /// <remarks>
        /// Attention: this feature does not work if the form field webHookURL is given.
        /// </remarks>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public string ResultFileName { get; set; }

        /// <summary>
        /// Converts the instance to a collection of http content items
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<HttpContent> ToHttpContent()
        {
            if (this.TimeOut.HasValue)
            {
                yield return CreateItem(this.TimeOut.ToString(), "waitTimeout");
            }

            if (this.WebHook != null)
            {
                yield return CreateItem(this.WebHook.ToString(), "webhookURL");
            }
            
            // ReSharper disable once InvertIf
            if (this.ResultFileName.IsSet())
            {
                yield return CreateItem(this.ResultFileName, "resultFilename");
            }
        }

        static StringContent CreateItem(string value, string fieldName)
        {
            var item = new StringContent(value);
            item.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data") { Name = fieldName };
            return item;
        }
    }
}
