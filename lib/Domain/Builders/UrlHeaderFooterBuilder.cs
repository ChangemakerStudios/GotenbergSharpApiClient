using System;
using System.IO;

using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Requests.Content;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Builders
{
    public sealed class UrlHeaderFooterBuilder: UrlRequestBuilder
    {
        [PublicAPI]
        public UrlHeaderFooterBuilder(UrlRequest request, UrlRequestBuilder parent)
        {
            this.Parent = parent;
            this.Request = request ?? throw new ArgumentNullException(nameof(request));
            this.Request.Content = new HeaderFooterDocument();
        } 
        
        [PublicAPI]
        public UrlRequestBuilder Parent { get; }

        #region header
        
        [PublicAPI]
        public UrlHeaderFooterBuilder AddHeader(ContentItem header)
        {
            this.Request.Content.Header = header;
            return this;
        }
        
        [PublicAPI]
        public UrlHeaderFooterBuilder AddHeader(string body) => AddHeader(new ContentItem(body));

        [PublicAPI]
        public UrlHeaderFooterBuilder AddHeader(byte[] body) => AddHeader(new ContentItem(body));

        [PublicAPI]
        public UrlHeaderFooterBuilder AddHeader(Stream body) => AddHeader(new ContentItem(body));

        #endregion
        
        #region footer
        
        [PublicAPI]
        public UrlHeaderFooterBuilder AddFooter(ContentItem footer)
        {
            this.Request.Content.Footer = footer;
            return this;
        }
        
        [PublicAPI]
        public UrlHeaderFooterBuilder AddFooter(string body) => AddFooter(new ContentItem(body));

        [PublicAPI]
        public UrlHeaderFooterBuilder AddFooter(byte[] body) => AddFooter(new ContentItem(body));

        [PublicAPI]
        public UrlHeaderFooterBuilder AddFooter(Stream body) => AddFooter(new ContentItem(body));
        
        #endregion

    }
}