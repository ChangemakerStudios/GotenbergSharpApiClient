using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using CaptiveAire.Gotenberg.App.API.Sharp.Client.Infrastructure;

namespace CaptiveAire.Gotenberg.App.API.Sharp.Client.Domain.Requests
{
    /// <summary>
    /// 
    /// </summary>
    public class UrlPdfRequest
    {
        public Uri Url { get; set; }
        public DocumentDimensions Dimensions { get; set; } = DocumentDimensions.ToChromeDefaults();
        public RequestConfig Config { get; set; } = new RequestConfig();
        
        internal IEnumerable<HttpContent> ToHttpContent()
        {
            if(!this.Url.IsAbsoluteUri) throw new ArgumentException("Absolute Urls only");
            
            var remoteUrl = new StringContent(this.Url.ToString());
            remoteUrl.Headers.ContentDisposition = new ContentDispositionHeaderValue(Constants.Http.Disposition.Types.FormData) {
                Name =  Constants.Gotenberg.FormFieldNames.RemoteURL
            };

            yield return remoteUrl;
            
            foreach(var item in Config.ToHttpContent().Concat(Dimensions.ToHttpContent()))
            {
                yield return item;
            }
        }

    }
}