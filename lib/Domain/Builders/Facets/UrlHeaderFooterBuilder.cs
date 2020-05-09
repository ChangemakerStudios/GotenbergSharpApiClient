using System;
using System.IO;
using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Builders.Facets
{
    public sealed class UrlHeaderFooterBuilder : UrlRequestBuilder
    {
        [PublicAPI]
        public UrlHeaderFooterBuilder(UrlRequest request)
        {
            this.Request = request ?? throw new ArgumentNullException(nameof(request));
            this.Request.Content ??= new HeaderFooterDocument();
        }

        #region header

        [PublicAPI]
        public UrlHeaderFooterBuilder SetHeader(ContentItem header)
        {
            this.Request.Content.Header = header;
            return this;
        }

        [PublicAPI]
        public UrlHeaderFooterBuilder SetHeader(string body) => SetHeader(new ContentItem(body));

        [PublicAPI]
        public UrlHeaderFooterBuilder SetHeader(byte[] body) => SetHeader(new ContentItem(body));

        [PublicAPI]
        public UrlHeaderFooterBuilder SetHeader(Stream body) => SetHeader(new ContentItem(body));

        #endregion

        #region footer

        [PublicAPI]
        public UrlHeaderFooterBuilder SetFooter(ContentItem footer)
        {
            this.Request.Content.Footer = footer;
            return this;
        }

        [PublicAPI]
        public UrlHeaderFooterBuilder SetFooter(string body) => SetFooter(new ContentItem(body));

        [PublicAPI]
        public UrlHeaderFooterBuilder SetFooter(byte[] body) => SetFooter(new ContentItem(body));

        [PublicAPI]
        public UrlHeaderFooterBuilder SetFooter(Stream body) => SetFooter(new ContentItem(body));

        #endregion
    }
}