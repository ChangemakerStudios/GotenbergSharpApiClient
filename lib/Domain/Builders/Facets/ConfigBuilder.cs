using System;

using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;
using Gotenberg.Sharp.API.Client.Extensions;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Builders.Facets
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
        public ConfigBuilder PageRanges(string value)
        {
            this.Request.Config.PageRanges = value;
            return this;
        }

        [PublicAPI]
        public ConfigBuilder ResultFileName(string value)
        {
            if (value.IsNotSet()) throw new ArgumentException("ResultFileName was null || empty");
            this.Request.Config.ResultFileName = value;
            return this;
        }

        [PublicAPI]
        public ConfigBuilder AddWebhook(Action<WebhookBuilder> builder)
        {
            builder(new WebhookBuilder(this.Request));
            return this;
        }
    }
}