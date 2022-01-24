using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;
using Gotenberg.Sharp.API.Client.Extensions;
using Gotenberg.Sharp.API.Client.Infrastructure;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public sealed class UrlRequest : ChromeRequest
    {
        public override string ApiPath => Constants.Gotenberg.ApiPaths.ConvertUrl;

        public Uri Url { get; set; }

        public KeyValuePair<string, string> RemoteUrlHeader { get; set; }

        public HeaderFooterDocument Content { get; set; }

        public override IEnumerable<HttpContent> ToHttpContent()
        {
            if (this.Url == null) throw new InvalidOperationException("Url is null");
            if (!this.Url.IsAbsoluteUri) throw new InvalidOperationException("Url.IsAbsoluteUri equals false");

            return new[] { CreateFormDataItem(this.Url, Constants.Gotenberg.FormFieldNames.RemoteUrl) }
                .Concat(Content.IfNullEmptyContent())
                .Concat(Config.IfNullEmptyContent())
                .Concat(Dimensions.IfNullEmptyContent())
                .Concat(GetExtraHeaderHttpContent().IfNullEmpty());
        }
 
    }
}