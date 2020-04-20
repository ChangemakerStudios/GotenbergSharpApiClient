using Gotenberg.Sharp.API.Client.Domain.Requests;

using JetBrains.Annotations;

using System;

namespace Gotenberg.Sharp.API.Client.Domain.Builders
{
    public sealed class UrlRequestBuilder: BaseRequestBuilder<UrlRequest>
    {
        protected sealed override UrlRequest Request { get; set; }

        [PublicAPI]
        public UrlRequestBuilder() => this.Request = new UrlRequest();

        [PublicAPI]
        public UrlRequestBuilder SetUrl(string url) => SetUrl(new Uri(url));

        [PublicAPI]
        public UrlRequestBuilder SetUrl(Uri url)
        {
            this.Request.Url = url;
            return this;
        }

        [PublicAPI]
        public DimensionBuilder<UrlRequestBuilder> Dimensions => new DimensionBuilder<UrlRequestBuilder>(Request, this);

        [PublicAPI]
        public ConfigBuilder<UrlRequestBuilder> ConfigureRequest => new ConfigBuilder<UrlRequestBuilder>(Request, this);

        [PublicAPI]
        public UrlRequest Build() 
        {
            if(this.Request.Url == null) throw new NullReferenceException("Request.Url is null");
            return Request;
        }

    }
}
