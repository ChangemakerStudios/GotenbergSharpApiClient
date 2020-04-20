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
    /// For URL to PDF conversions
    /// </summary>
  
    public sealed class UrlRequest : ResourceRequest, IConvertToHttpContent
    {
        [UsedImplicitly]
        public Uri Url { get; set; }

        [UsedImplicitly]
        public KeyValuePair<string, string> RemoteUrlHeader { get; set; }
        
        public  IEnumerable<HttpContent> ToHttpContent()
        {
            if(!this.Url.IsAbsoluteUri) throw new ArgumentException("Absolute Urls only");
            
            var remoteUrl = new StringContent(this.Url.ToString());
            remoteUrl.Headers.ContentDisposition = new ContentDispositionHeaderValue(Constants.Http.Disposition.Types.FormData) {
                                                                                                                                    Name =  Constants.Gotenberg.FormFieldNames.RemoteURL
                                                                                                                                };

            yield return remoteUrl;


            foreach (var item in Config?.ToHttpContent() ?? Enumerable.Empty<HttpContent>()
                                         .Concat(this.Dimensions?.ToHttpContent() ?? Content.Dimensions.ToChromeDefaults().ToHttpContent()))
            {
                yield return item;
            }

        }

    }
}