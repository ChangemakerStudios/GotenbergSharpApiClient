using System;

using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;
using Gotenberg.Sharp.API.Client.Extensions;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Builders.Faceted
{
    public sealed class ConfigBuilder : BaseFacetedBuilder<RequestBase>
    {
        public ConfigBuilder(RequestBase request)
        {
            this.Request = request ?? throw new ArgumentNullException(nameof(request));
            this.Request.Config ??= new RequestConfig();
        }

        [PublicAPI]
        public ConfigBuilder SetPageRanges(string pageRanges)
        {
            if (pageRanges.IsNotSet()) throw new ArgumentException("Cannot be null or empty", nameof(pageRanges));
            this.Request.Config.PageRanges = pageRanges;
            return this;
        }

        [PublicAPI]
        [Obsolete("Use SetPageRanges instead. This is going away")]
        public ConfigBuilder PageRanges(string pageRanges)
        {
            return SetPageRanges(pageRanges);
        }

        [PublicAPI]
        public ConfigBuilder ResultFileName(string resultFileName)
        {
            if (resultFileName.IsNotSet())
                throw new ArgumentException("Cannot be null or empty", nameof(resultFileName));
            this.Request.Config.ResultFileName = resultFileName;
            return this;
        }

        [PublicAPI]
        public ConfigBuilder AddWebhook(Action<WebhookBuilder> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            action(new WebhookBuilder(this.Request));
            return this;
        }

        [PublicAPI]
        public ConfigBuilder AddWebhook(Webhook webhook)
        {
            this.Request.Config.Webhook = webhook ?? throw new ArgumentNullException(nameof(webhook));
            if (!webhook.TargetUrl.IsAbsoluteUri)
                throw new InvalidOperationException("Webhook must have an absolute url");

            return this;
        }
    }
}