using System;

using Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;
using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Builders
{
    public abstract class BaseBuilder<TRequest, TBuilder>
        where TRequest : RequestBase
        where TBuilder : BaseBuilder<TRequest, TBuilder>
    {
        protected virtual TRequest Request { get; set; }

        protected const string CallBuildAsyncErrorMessage = "Request has asynchronous items. Call BuildAsync instead.";

        [PublicAPI]
        public TBuilder ConfigureRequest(Action<ConfigBuilder> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            action(new ConfigBuilder(this.Request));
            return (TBuilder) this;
        }

        [PublicAPI]
        public TBuilder ConfigureRequest(RequestConfig config)
        {
            this.Request.Config = config ?? throw new ArgumentNullException(nameof(config));
            return (TBuilder) this;
        }
    }
}