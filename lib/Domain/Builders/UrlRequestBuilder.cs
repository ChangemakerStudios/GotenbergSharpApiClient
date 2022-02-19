using System;
using System.Linq;
using System.Threading.Tasks;

using Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;
using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Extensions;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Builders
{
    public sealed class UrlRequestBuilder : BaseChromiumBuilder<UrlRequest, UrlRequestBuilder>
    {
        protected override UrlRequest Request { get; set; }

        [PublicAPI]
        public UrlRequestBuilder() => this.Request = new UrlRequest();

        [PublicAPI]
        public UrlRequestBuilder SetUrl(string url)
        {
            if (url.IsNotSet()) throw new ArgumentException("url is either null or empty");
            return SetUrl(new Uri(url));
        }

        [PublicAPI]
        public UrlRequestBuilder SetUrl(Uri url)
        {
            this.Request.Url = url ?? throw new ArgumentNullException(nameof(url));
            if (!url.IsAbsoluteUri) throw new InvalidOperationException("url is not absolute");
            return this;
        }

        [PublicAPI]
        public UrlRequestBuilder AddHeaderFooter(Action<UrlHeaderFooterBuilder> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            action(new UrlHeaderFooterBuilder(Request));
            return this;
        }

        [PublicAPI]
        public UrlRequestBuilder AddAsyncHeaderFooter(Func<UrlHeaderFooterBuilder, Task> asyncAction)
        {
            if (asyncAction == null) throw new ArgumentNullException(nameof(asyncAction));

            this.AsyncTasks.Add(asyncAction(new UrlHeaderFooterBuilder(this.Request)));
            return this;
        }

        [PublicAPI]
        public UrlRequestBuilder AddExtraResources(Action<UrlExtraResourcesBuilder> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            action(new UrlExtraResourcesBuilder(this.Request));
            return this;
        }

        [PublicAPI]
        public UrlRequest Build()
        {
            if (AsyncTasks.Any()) throw new InvalidOperationException(CallBuildAsyncErrorMessage);
            if (this.Request.Url == null) throw new InvalidOperationException("Request.Url is null");
            return Request;
        }

        [PublicAPI]
        public async Task<UrlRequest> BuildAsync()
        {
            if (this.Request.Url == null) throw new InvalidOperationException("Request.Url is null");

            if (AsyncTasks.Any())
            {
                await Task.WhenAll(AsyncTasks).ConfigureAwait(false);
            }

            return Request;
        }
    }
}