using System;
using System.Collections.Generic;

using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;
using Gotenberg.Sharp.API.Client.Extensions;
using Gotenberg.Sharp.API.Client.Infrastructure;

using JetBrains.Annotations;


namespace Gotenberg.Sharp.API.Client.Domain.Builders.Facets
{
    public sealed class WebhookBuilder : BaseBuilder<RequestBase>
    {
        public WebhookBuilder(RequestBase request)
        {
            this.Request = request ?? throw new ArgumentNullException(nameof(request));
            this.Request.Config ??= new RequestConfig();
            this.Request.Config.Webhook ??= new Webhook();
        }

        /// <summary>
        /// When testing web hooks against a local container and a service
        /// running on localhost to receive the posts, use http://host.docker.internal 
        /// Reference: https://docs.docker.com/docker-for-windows/networking/#use-cases-and-workarounds
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        [PublicAPI]
        public WebhookBuilder SetUrl(string url)
        {
            if(url.IsNotSet()) throw new ArgumentException("url is either null or empty");
            return SetUrl(new Uri(url));
        }

        [PublicAPI]
        public WebhookBuilder SetUrl(Uri url)
        {
            this.Request.Config.Webhook.TargetUrl = url ?? throw new ArgumentNullException(nameof(url));
            if (!url.IsAbsoluteUri) throw new InvalidOperationException("Url base href is not absolute");
            return this;
        }

        [PublicAPI]
        public WebhookBuilder SetTimeout(float seconds)
        {
            this.Request.Config.Webhook.Timeout = seconds;
            return this;
        }

        [PublicAPI]
        public WebhookBuilder AddRequestHeader(string name, string value)
            => AddRequestHeader(name, new[] { value });

        [PublicAPI]
        public WebhookBuilder AddRequestHeader(KeyValuePair<string, string> header)
            => AddRequestHeader(header.Key, new[] { header.Value });

        [PublicAPI]
        public WebhookBuilder AddRequestHeader(string name, IEnumerable<string> values)
        {
            if (name.IsNotSet()) throw new ArgumentException("request header name is null || empty");

            var headerName = $"{Constants.Gotenberg.CustomRemoteHeaders.WebhookHeaderKeyPrefix}{name}";
            this.Request.CustomHeaders.Add(headerName, values);
            return this;
        }
    }
}