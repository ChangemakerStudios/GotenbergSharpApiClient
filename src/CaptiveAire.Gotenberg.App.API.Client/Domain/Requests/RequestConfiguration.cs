using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using CaptiveAire.Gotenberg.App.API.Sharp.Client.Extensions;
using CaptiveAire.Gotenberg.App.API.Sharp.Client.Infrastructure;
// ReSharper disable UnusedMember.Global

namespace CaptiveAire.Gotenberg.App.API.Sharp.Client.Domain.Requests
{
    /// <summary>
    /// All endpoints accept form fields for each property
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class RequestConfiguration
    {
        Uri _webHook;
        static readonly Type _attribType = typeof(MultiFormHeaderAttribute);

        /// <summary>
        ///     If provided, the API will wait the given seconds before it considers the
        ///     conversion to be unsuccessful. If unsuccessful, it returns a 504 HTTP code.
        /// </summary>
        public float? TimeOut { get; set; }
    
 
        /// <summary>
        /// If set the Gotenberg API will send the resulting PDF file in a POST request with
        /// the application-pdf content type to the given url. If used your requests to the API
        /// will be over before the conversions are done.
        /// </summary>
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
        public string ResultFileName { get; set; }

        /// <summary>
        /// Converts the instance to a collection of http content items
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<StringContent> ToStringContent()
        {
            var result = new List<StringContent>();

            if (this.TimeOut.HasValue)
            {
                var item = new StringContent(this.TimeOut.ToString());
                item.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data") { Name = "waitTimeout" };
                result.Add(item);
            }

            if (this.WebHook != null)
            {
                var item = new StringContent(this.WebHook.ToString());
                item.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data") { Name = "webhookURL" };
                result.Add(item);

            }
            
            if (this.ResultFileName.IsSet())
            {
                var item = new StringContent(this.ResultFileName);
                item.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data") { Name = "resultFilename" };
                result.Add(item);
            }

            return result;
        }

    }
}
