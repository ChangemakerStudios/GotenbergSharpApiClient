using System;
using System.IO;
using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Requests.Content;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Builders.FacetedBuilders
{
    public sealed class DocumentBuilder: PdfRequestBuilder
    {
        public DocumentBuilder(ContentRequest request, PdfRequestBuilder parent)
        {
            this.Parent = parent;
            this.Request = request ?? throw new ArgumentNullException(nameof(request));
            this.Request.Content ??= new FullDocument();
        }

        [PublicAPI]
        public PdfRequestBuilder Parent { get; }

        #region body
        
        [PublicAPI]
        public DocumentBuilder AddBody(ContentItem body)
        {
            this.Request.Content.Body = body;
            return this;
        }

        [PublicAPI]
        public DocumentBuilder AddBody(string body) => AddBody(new ContentItem(body));

        [PublicAPI]
        public DocumentBuilder AddBody(byte[] body) => AddBody(new ContentItem(body));

        [PublicAPI]
        public DocumentBuilder AddBody(Stream body) => AddBody(new ContentItem(body));

        #endregion

        #region header
        
        [PublicAPI]
        public DocumentBuilder AddHeader(ContentItem header)
        {
            this.Request.Content.Header = header;
            return this;
        }
        
        [PublicAPI]
        public DocumentBuilder AddHeader(string body) => AddHeader(new ContentItem(body));

        [PublicAPI]
        public DocumentBuilder AddHeader(byte[] body) => AddHeader(new ContentItem(body));

        [PublicAPI]
        public DocumentBuilder AddHeader(Stream body) => AddHeader(new ContentItem(body));

        #endregion
        
        #region footer
        
        [PublicAPI]
        public DocumentBuilder AddFooter(ContentItem footer)
        {
            this.Request.Content.Footer = footer;
            return this;
        }
        
        [PublicAPI]
        public DocumentBuilder AddFooter(string body) => AddFooter(new ContentItem(body));

        [PublicAPI]
        public DocumentBuilder AddFooter(byte[] body) => AddFooter(new ContentItem(body));

        [PublicAPI]
        public DocumentBuilder AddFooter(Stream body) => AddFooter(new ContentItem(body));
        
        #endregion
 
    }
}