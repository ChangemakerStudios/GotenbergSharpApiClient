using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Gotenberg.Sharp.API.Client.Infrastructure;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class UrlRequest : IConvertToHttpContent
    {
        [UsedImplicitly]
        public Uri Url { get; set; }

        [UsedImplicitly]
        public DocumentDimensions Dimensions { get; set; } = DocumentDimensions.ToChromeDefaults();

        [UsedImplicitly]
        public HttpMessageConfig Config { get; set; } = new HttpMessageConfig();
        
        [UsedImplicitly]
        public KeyValuePair<string, string> RemoteUrlHeader { get; set; }
        
        public IEnumerable<HttpContent> ToHttpContent()
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