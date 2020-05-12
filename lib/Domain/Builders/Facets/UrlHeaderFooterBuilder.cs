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
            this.Request.Content.Header = header ?? throw new ArgumentNullException(nameof(header));
            return this;
        }

        [PublicAPI]
        public UrlHeaderFooterBuilder SetHeader(string header) =>
            SetHeader(new ContentItem(header));

        [PublicAPI]
        public UrlHeaderFooterBuilder SetHeader(byte[] header) => SetHeader(new ContentItem(header));

        [PublicAPI]
        public UrlHeaderFooterBuilder SetHeader(Stream header) => SetHeader(new ContentItem(header));

        #endregion

        #region footer

        [PublicAPI]
        public UrlHeaderFooterBuilder SetFooter(ContentItem footer)
        {
            this.Request.Content.Footer = footer ?? throw new ArgumentNullException(nameof(footer));
            return this;
        }

        [PublicAPI]
        public UrlHeaderFooterBuilder SetFooter(string footer) => SetFooter(new ContentItem(footer));

        [PublicAPI]
        public UrlHeaderFooterBuilder SetFooter(byte[] footer) => SetFooter(new ContentItem(footer));

        [PublicAPI]
        public UrlHeaderFooterBuilder SetFooter(Stream footer) => SetFooter(new ContentItem(footer));

        #endregion
    }
}