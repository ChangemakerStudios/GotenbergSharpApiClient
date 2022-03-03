using System;
using System.IO;

using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Builders.Faceted
{
    /// <summary>
    ///   Note:  If you don't specify any dimensions the client sets them to Chrome's defaults
    /// </summary>
    public sealed class DocumentBuilder : BaseFacetedBuilder<HtmlRequest>
    {
        public DocumentBuilder(HtmlRequest request)
        {
            this.Request = request ?? throw new ArgumentNullException(nameof(request));
            this.Request.Content ??= new FullDocument();
        }

        #region body

        [PublicAPI]
        public DocumentBuilder ContainsMarkdown(bool containsMarkdown = true)
        {
            this.Request.ContainsMarkdown = containsMarkdown;
            return this;
        }

        [PublicAPI]
        public DocumentBuilder SetBody(ContentItem body)
        {
            this.Request.Content.Body = body ?? throw new ArgumentNullException(nameof(body));
            return this;
        }

        [PublicAPI]
        public DocumentBuilder SetBody(string body) =>
            SetBody(new ContentItem(body));

        [PublicAPI]
        public DocumentBuilder SetBody(byte[] body) =>
            SetBody(new ContentItem(body));

        [PublicAPI]
        public DocumentBuilder SetBody(Stream body) =>
            SetBody(new ContentItem(body));

        #endregion

        #region header

        [PublicAPI]
        public DocumentBuilder SetHeader(ContentItem header)
        {
            this.Request.Content.Header = header ?? throw new ArgumentNullException(nameof(header));
            return this;
        }

        [PublicAPI]
        public DocumentBuilder SetHeader(string header) =>
            SetHeader(new ContentItem(header));

        [PublicAPI]
        public DocumentBuilder SetHeader(byte[] header) =>
            SetHeader(new ContentItem(header));

        [PublicAPI]
        public DocumentBuilder SetHeader(Stream header) =>
            SetHeader(new ContentItem(header));

        #endregion

        #region footer

        [PublicAPI]
        public DocumentBuilder SetFooter(ContentItem footer)
        {
            this.Request.Content.Footer = footer ?? throw new ArgumentNullException(nameof(footer));
            return this;
        }

        [PublicAPI]
        public DocumentBuilder SetFooter(string footer) =>
            SetFooter(new ContentItem(footer));

        [PublicAPI]
        public DocumentBuilder SetFooter(byte[] footer)
            => SetFooter(new ContentItem(footer));

        [PublicAPI]
        public DocumentBuilder SetFooter(Stream footer)
            => SetFooter(new ContentItem(footer));

        #endregion
    }
}