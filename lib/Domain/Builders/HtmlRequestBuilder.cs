using System;

using Gotenberg.Sharp.API.Client.Domain.Builders.FacetedBuilders;
using Gotenberg.Sharp.API.Client.Domain.Requests;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Builders
{
    [PublicAPI]
    public class HtmlRequestBuilder: BaseBuilder<HtmlRequest>
    {
        protected sealed override HtmlRequest Request { get; set; }

        [PublicAPI]
        public HtmlRequestBuilder(bool containsMarkdown = false) => this.Request = new HtmlRequest(containsMarkdown);

        [PublicAPI]
        public DocumentBuilder Document => new DocumentBuilder(Request, this);

        [PublicAPI]
        public AssetBuilder<HtmlRequestBuilder> Assets => new AssetBuilder<HtmlRequestBuilder>(this.Request, this);

        [PublicAPI]
        public DimensionBuilder<HtmlRequestBuilder> Dimensions => new DimensionBuilder<HtmlRequestBuilder>(this.Request, this);

        [PublicAPI]
        public ConfigBuilder<HtmlRequestBuilder> ConfigureRequest => new ConfigBuilder<HtmlRequestBuilder>(this.Request, this);

        [PublicAPI]
        public HtmlRequest Build() 
        {
            if(this.Request.Content?.Body == null) throw new NullReferenceException("Request.Content or Content.Body is null");
            return Request;
        }
    }
 }