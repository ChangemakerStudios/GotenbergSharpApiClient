using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;
using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;
using Gotenberg.Sharp.API.Client.Extensions;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Builders
{
    public class UrlRequestBuilder : BaseBuilder<UrlRequest>
    {
        readonly List<Task> _asyncTasks = new List<Task>();

        protected sealed override UrlRequest Request { get; set; }

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

        /// <summary>
        ///  Configures gotenberg to emulate html loading as screen. By default it loads it as print
        /// </summary>
        /// <returns></returns>
        [PublicAPI]
        public UrlRequestBuilder EmulateAsScreen()
        {
            this.Request.ConversionBehaviors.EmulatedMediaType = "screen";

            return this;
        }

        /// <summary>
        ///  Tells gotenberg to return a 409 response if there are exceptions in the Chromium console. 
        /// </summary>
        /// <returns></returns>
        [PublicAPI]
        public UrlRequestBuilder FailOnConsoleExceptions()
        {
            this.Request.ConversionBehaviors.FailOnConsoleExceptions = true;

            return this;
        }

        [PublicAPI]
        public UrlRequestBuilder SetBrowserWaitDelay(int seconds)
        {
            this.Request.ConversionBehaviors.WaitDelay = $"{seconds}s";

            return this;
        }

        [PublicAPI]
        public UrlRequestBuilder SetBrowserWaitExpression(string expression)
        {
            if(expression.IsNotSet()) throw new InvalidOperationException("expression is not set");
            this.Request.ConversionBehaviors.WaitForExpression = expression;

            return this;
        }


        [PublicAPI]
        public UrlRequestBuilder AddAdditionalHeaders(string headerName, string headerValue)
        {
            if (headerName.IsNotSet()) throw new InvalidOperationException("headerName is not set");
            this.Request.ConversionBehaviors.AddExtraHeaders(headerName, headerValue);
          
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

            this._asyncTasks.Add(asyncAction(new UrlHeaderFooterBuilder(this.Request)));
            return this;
        }

        [PublicAPI]
        public UrlRequestBuilder WithDimensions(Dimensions dimensions)
        {
            this.Request.Dimensions = dimensions ?? throw new ArgumentNullException(nameof(dimensions));
            return this;
        }

        [PublicAPI]
        public UrlRequestBuilder WithDimensions(Action<DimensionBuilder> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            action(new DimensionBuilder(this.Request));
            return this;
        }

        [PublicAPI]
        public UrlRequestBuilder ConfigureRequest(RequestConfig config)
        {
            this.Request.Config = config ?? throw new ArgumentNullException(nameof(config));
            return this;
        }

        [PublicAPI]
        public UrlRequestBuilder ConfigureRequest(Action<ConfigBuilder> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            action(new ConfigBuilder(this.Request));
            return this;
        }

        [PublicAPI]
        public UrlRequest Build()
        {
            if (_asyncTasks.Any()) throw new InvalidOperationException(CallBuildAsyncErrorMessage);
            if (this.Request.Url == null) throw new InvalidOperationException("Request.Url is null");
            return Request;
        }

        [PublicAPI]
        public async Task<UrlRequest> BuildAsync()
        {
            if (this.Request.Url == null) throw new InvalidOperationException("Request.Url is null");

            if (_asyncTasks.Any())
            {
                await Task.WhenAll(_asyncTasks).ConfigureAwait(false);
            }

            return Request;
        }
    }
}