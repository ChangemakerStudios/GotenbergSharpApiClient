using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

using Gotenberg.Sharp.API.Client.Domain.Requests.Content;
using Gotenberg.Sharp.API.Client.Extensions;
using Gotenberg.Sharp.API.Client.Infrastructure;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    /// <summary>
    /// For URL to PDF conversions
    /// </summary>
    public sealed class UrlRequest: ResourceRequest, IConvertToHttpContent
    {
        public Uri Url { get; set; }
        public KeyValuePair<string, string> RemoteUrlHeader { get; set; }
        public HeaderFooterDocument Content { get; set; }
        
        public IEnumerable<HttpContent> ToHttpContent()
        {
            if (this.Url == null) throw new NullReferenceException(nameof(Url));
            if (!this.Url.IsAbsoluteUri) throw new ArgumentException("Absolute Urls only");

            return new [] { AddRemoteUrl(this.Url) }
                   .Concat(Content.IfNullEmptyContent())
                   .Concat(Config.IfNullEmptyContent())
                   .Concat(Dimensions.IfNullEmptyContent());
            
            static StringContent AddRemoteUrl(Uri url)
            {
                var remoteUrl = new StringContent(url.ToString());
                remoteUrl.Headers.ContentDisposition = new ContentDispositionHeaderValue(Constants.HttpContent.Disposition.Types.FormData) 
                {
                    Name =  Constants.Gotenberg.FormFieldNames.RemoteUrl
                };
                
                return remoteUrl;
            }
        }

    }
}