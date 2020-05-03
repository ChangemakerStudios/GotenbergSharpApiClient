using System;
using System.IO;
using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Builders.Facets
{
    /// <remarks>
    ///     If you don't specify any dimensions the client sets them to Chrome's defaults
    /// </remarks>
    public sealed class DocumentBuilder: HtmlRequestBuilder
    {
        public DocumentBuilder(HtmlRequest request)
        {
            this.Request = request ?? throw new ArgumentNullException(nameof(request));
            this.Request.Content ??= new FullDocument();
        }

        #region body
        
        [PublicAPI]
        public DocumentBuilder SetBody(ContentItem body)
        {
            this.Request.Content.Body = body;
            return this;
        }

        [PublicAPI]
        public DocumentBuilder SetBody(string body) => SetBody(new ContentItem(body));

        [PublicAPI]
        public DocumentBuilder SetBody(byte[] body) => SetBody(new ContentItem(body));

        [PublicAPI]
        public DocumentBuilder SetBody(Stream body) => SetBody(new ContentItem(body));

        #endregion

        #region header
        
        [PublicAPI]
        public DocumentBuilder SetHeader(ContentItem header)
        {
            this.Request.Content.Header = header;
            return this;
        }
        
        [PublicAPI]
        public DocumentBuilder SetHeader(string body) => SetHeader(new ContentItem(body));

        [PublicAPI]
        public DocumentBuilder SetHeader(byte[] body) => SetHeader(new ContentItem(body));

        [PublicAPI]
        public DocumentBuilder SetHeader(Stream body) => SetHeader(new ContentItem(body));

        #endregion
        
        #region footer
        
        [PublicAPI]
        public DocumentBuilder SetFooter(ContentItem footer)
        {
            this.Request.Content.Footer = footer;
            return this;
        }
        
        [PublicAPI]
        public DocumentBuilder SetFooter(string body) => SetFooter(new ContentItem(body));

        [PublicAPI]
        public DocumentBuilder SetFooter(byte[] body) => SetFooter(new ContentItem(body));

        [PublicAPI]
        public DocumentBuilder SetFooter(Stream body) => SetFooter(new ContentItem(body));
        
        #endregion
 
    }
}