using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;
using Gotenberg.Sharp.API.Client.Extensions;
using Gotenberg.Sharp.API.Client.Infrastructure;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public sealed class UrlRequest : ChromeRequest
    {
        public override string ApiPath => Constants.Gotenberg.ApiPaths.UrlConvert;

        public Uri Url { get; set; }

        public KeyValuePair<string, string> RemoteUrlHeader { get; set; }

        public HeaderFooterDocument Content { get; set; }

        public override IEnumerable<HttpContent> ToHttpContent()
        {
            if (this.Url == null) throw new InvalidOperationException("Url is null");
            if (!this.Url.IsAbsoluteUri) throw new InvalidOperationException("Url.IsAbsoluteUri equals false");

            TryAddRemoteHeader();

            return new[] { AddRemoteUrl(this.Url) }
                .Concat(Content.IfNullEmptyContent())
                .Concat(Config.IfNullEmptyContent())
                .Concat(Dimensions.IfNullEmptyContent());
        }

        #region add url/header

        void TryAddRemoteHeader()
        {
            if (!RemoteUrlHeader.Key.IsSet()) return;

            var name = $"{Constants.Gotenberg.CustomRemoteHeaders.RemoteUrlKeyPrefix}{RemoteUrlHeader.Key}";
            this.CustomHeaders.AddItem(name, RemoteUrlHeader.Value);
        }

        static StringContent AddRemoteUrl(Uri url)
        {
            var remoteUrl = new StringContent(url.ToString());
            remoteUrl.Headers.ContentDisposition =
                new ContentDispositionHeaderValue(Constants.HttpContent.Disposition.Types.FormData)
                {
                    Name = Constants.Gotenberg.FormFieldNames.RemoteUrl
                };

            return remoteUrl;
        }

        #endregion
    }
}