using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace CaptiveAire.Gotenberg.App.API.Sharp.Client.Domain.Requests
{
    /// <summary>
    /// 
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class UrlPdfRequest
    {
        public Uri Url { get; set; }
        public DocumentDimensions Dimensions { get; set; } = DocumentDimensions.ToChromeDefaults();
        public RequestConfig Config { get; set; } = new RequestConfig();
        
        internal IEnumerable<HttpContent> ToHttpContent()
        {
            if(!this.Url.IsAbsoluteUri) throw new ArgumentException("Absolute Urls only");
            
            var remoteUrl = new StringContent(this.Url.ToString());
            remoteUrl.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data") { Name = "remoteURL" };

            yield return remoteUrl;
            
            foreach(var item in Config.ToHttpContent().Concat(Dimensions.ToHttpContent()))
            {
                yield return item;
            }
        }

    }
}