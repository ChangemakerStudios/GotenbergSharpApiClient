using Gotenberg.Sharp.API.Client.Domain.Requests;
using JetBrains.Annotations;

using System;
using System.Collections.Generic;

namespace Gotenberg.Sharp.API.Client.Domain.Builders
{
    public class UrlRequestBuilder: BaseBuilder<UrlRequest>
    {
        protected sealed override UrlRequest Request { get; set; }

        [PublicAPI]
        public UrlRequestBuilder() => this.Request = new UrlRequest();

        [PublicAPI]
        public UrlRequestBuilder(string url)
        {
            this.Request = new UrlRequest();
            this.SetUrl(url);
        }
        
        [PublicAPI]
        public UrlRequestBuilder(Uri url)
        {
            this.Request = new UrlRequest();
            this.SetUrl(url);
        }
        
        [PublicAPI]
        public UrlRequestBuilder SetUrl(string url) => SetUrl(new Uri(url));
        
        [PublicAPI]
        public UrlRequestBuilder SetRemoteUrlHeader(string name, string value)
        {
            this.Request.RemoteUrlHeader = new KeyValuePair<string, string>(name, value);
            return this;
        }
        

        [PublicAPI]
        public UrlRequestBuilder SetUrl(Uri url)
        {
            this.Request.Url = url;
            return this;
        }

        [PublicAPI]
        public UrlHeaderFooterBuilder Document => new UrlHeaderFooterBuilder(Request, this);
        
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
