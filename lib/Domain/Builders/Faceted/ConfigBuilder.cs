using System;

using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;
using Gotenberg.Sharp.API.Client.Extensions;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Builders.Faceted
{
    public sealed class ConfigBuilder : BaseBuilder<RequestBase>
    {
        public ConfigBuilder(RequestBase request)
        {
            this.Request = request ?? throw new ArgumentNullException(nameof(request));
            this.Request.Config ??= new RequestConfig();
        }


        [PublicAPI]
        public ConfigBuilder TimeOut(float value)
        {
            this.Request.Config.TimeOut = value;
            return this;
        }

        [PublicAPI]
        public ConfigBuilder ChromeRpccBufferSize(int value)
        {
            this.Request.Config.ChromeRpccBufferSize = value;
            return this;
        }

        [PublicAPI]
        public ConfigBuilder PageRanges(string pageRanges)
        {
            if (pageRanges.IsNotSet()) throw new ArgumentException("Cannot be null or empty", nameof(pageRanges));
            this.Request.Config.PageRanges = pageRanges;
            return this;
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