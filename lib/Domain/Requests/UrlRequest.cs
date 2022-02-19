using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;
using Gotenberg.Sharp.API.Client.Domain.Requests.Facets.UrlExtras;
using Gotenberg.Sharp.API.Client.Extensions;
using Gotenberg.Sharp.API.Client.Infrastructure;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public sealed class UrlRequest : ChromeRequest
    {
        public override string ApiPath 
            => Constants.Gotenberg.Chromium.ApiPaths.ConvertUrl;

        public Uri Url { get; set; }

        /// <summary>
        ///  Requires top/bottom margin set to appear   
        /// </summary>
        public HeaderFooterDocument Content { get; set; }

        public ExtraUrlResources ExtraResources { get; set; }

        public override IEnumerable<HttpContent> ToHttpContent()
        {
            if (this.Url == null) throw new InvalidOperationException("Url is null");
            if (!this.Url.IsAbsoluteUri) throw new InvalidOperationException("Url.IsAbsoluteUri equals false");

            return base.ToHttpContent()
                .Concat(Content.IfNullEmptyContent())
                .Concat(ExtraResources.IfNullEmptyContent())
                .Concat(Assets.IfNullEmptyContent())
                /*.Concat(GetExtraHeaderHttpContent().IfNullEmpty())*/
                .Concat(new[] { CreateFormDataItem(this.Url, Constants.Gotenberg.Chromium.Routes.Url.RemoteUrl) });
        }
    }
}