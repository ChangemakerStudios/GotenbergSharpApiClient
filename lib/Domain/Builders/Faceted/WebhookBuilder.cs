using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;
using Gotenberg.Sharp.API.Client.Extensions;
using Gotenberg.Sharp.API.Client.Infrastructure;

using JetBrains.Annotations;

using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Gotenberg.Sharp.API.Client.Domain.Builders.Faceted
{
    public sealed class WebhookBuilder : BaseBuilder<RequestBase>
    {
        public WebhookBuilder(RequestBase request)
        {
            this.Request = request ?? throw new ArgumentNullException(nameof(request));
            this.Request.Config ??= new RequestConfig();
            this.Request.Config.Webhook ??= new Webhook();
        }

        [PublicAPI]
        public WebhookBuilder Set(Webhook hook)
        {
            if (hook?.TargetUrl == null) throw new ArgumentNullException(nameof(hook));
            this.Request.Config.Webhook = hook;
            return this;
        }

        /// <summary>
        /// When testing web hooks against a local container and a service
        /// running on localhost, to receive the posts, use http://host.docker.internal:your-port 
        /// Reference: https://docs.docker.com/docker-for-windows/networking/#use-cases-and-workarounds
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        [PublicAPI]
        public WebhookBuilder SetUrl(string url, HttpMethod method = null)
        {
            if (url.IsNotSet()) throw new ArgumentException("url is either null or empty");
            return SetUrl(new Uri(url), method);
        }

        [PublicAPI]
        public WebhookBuilder SetUrl(Uri url, HttpMethod method = null)
        {
            this.Request.Config.Webhook.TargetUrl = url ?? throw new ArgumentNullException(nameof(url));
            this.Request.Config.Webhook.HttpMethod = method?.ToString();
            if (!url.IsAbsoluteUri) throw new InvalidOperationException("Url base href is not absolute");
            return this;
        }

        [PublicAPI]
        public WebhookBuilder SetErrorUrl(string url, HttpMethod method = null)
        {
            if (url.IsNotSet()) throw new ArgumentException("url is either null or empty");
            return SetErrorUrl(new Uri(url), method);
        }

        [PublicAPI]
        public WebhookBuilder SetErrorUrl(Uri url, HttpMethod method = null)
        {
            this.Request.Config.Webhook.ErrorUrl = url ?? throw new ArgumentNullException(nameof(url));

            if (!url.IsAbsoluteUri) throw new InvalidOperationException("Url base href is not absolute");
            
            this.Request.Config.Webhook.ErrorHttpMethod = method?.ToString();

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
            if (name.IsNotSet()) throw new ArgumentException("request header name is null || empty", nameof(name));

            this.Request.CustomHeaders.Add(Constants.Gotenberg.FormFieldNames.Webhook.ExtraHeaders, values);
            return this;
        }
    }
}