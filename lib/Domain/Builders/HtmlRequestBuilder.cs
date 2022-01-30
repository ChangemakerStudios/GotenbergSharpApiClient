using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;
using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Extensions;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Builders
{
    public class HtmlRequestBuilder : BaseBuilder<HtmlRequest>
    {
        readonly List<Task> _asyncTasks = new List<Task>();

        protected sealed override HtmlRequest Request { get; set; }

        public HtmlRequestBuilder() 
            => this.Request = new HtmlRequest();

        [PublicAPI]
        public HtmlRequestBuilder(bool containsMarkdown)
            => this.Request = new HtmlRequest(containsMarkdown);

        /// <summary>
        ///  Configures gotenberg to emulate html loading as screen. By default it loads it as print
        /// </summary>
        /// <returns></returns>
        [PublicAPI]
        public HtmlRequestBuilder EmulateAsScreen()
        {
            this.Request.ConversionBehaviors.EmulatedMediaType = "screen";

            return this;
        }

        /// <summary>
        ///  Tells gotenberg to return a 409 response if there are exceptions in the Chromium console. 
        /// </summary>
        /// <returns></returns>
        [PublicAPI]
        public HtmlRequestBuilder FailOnConsoleExceptions()
        {
            this.Request.ConversionBehaviors.FailOnConsoleExceptions = true;

            return this;
        }

        [PublicAPI]
        public HtmlRequestBuilder SetBrowserWaitDelay(int seconds)
        {
            this.Request.ConversionBehaviors.WaitDelay = $"{seconds}s";

            return this;
        }

        [PublicAPI]
        public HtmlRequestBuilder SetBrowserWaitExpression(string expression)
        {
            if (expression.IsNotSet()) throw new InvalidOperationException("expression is not set");
            this.Request.ConversionBehaviors.WaitForExpression = expression;

            return this;
        }


        [PublicAPI]
        public HtmlRequestBuilder AddAdditionalHeaders(string headerName, string headerValue)
        {
            if (headerName.IsNotSet()) throw new InvalidOperationException("headerName is not set");
            this.Request.ConversionBehaviors.AddExtraHeaders(headerName, headerValue);

            return this;
        }

        [PublicAPI]
        public HtmlRequestBuilder AddDocument(Action<DocumentBuilder> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            action(new DocumentBuilder(this.Request));
            return this;
        }

        [PublicAPI]
        public HtmlRequestBuilder AddAsyncDocument(Func<DocumentBuilder, Task> asyncAction)
        {
            if (asyncAction == null) throw new ArgumentNullException(nameof(asyncAction));

            this._asyncTasks.Add(asyncAction(new DocumentBuilder(this.Request)));
            return this;
        }

        [PublicAPI]
        public HtmlRequestBuilder WithDimensions(Action<DimensionBuilder> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            action(new DimensionBuilder(this.Request));
            return this;
        }

        [PublicAPI]
        public HtmlRequestBuilder WithAssets(Action<AssetBuilder> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            action(new AssetBuilder(this.Request));
            return this;
        }

        [PublicAPI]
        public HtmlRequestBuilder WithAsyncAssets(Func<AssetBuilder, Task> asyncAction)
        {
            if (asyncAction == null) throw new ArgumentNullException(nameof(asyncAction));
            this._asyncTasks.Add(asyncAction(new AssetBuilder(this.Request)));
            return this;
        }

        [PublicAPI]
        public HtmlRequestBuilder ConfigureRequest(Action<ConfigBuilder> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            action(new ConfigBuilder(this.Request));
            return this;
        }


        [PublicAPI]
        public HtmlRequest Build()
        {
            if (_asyncTasks.Any()) throw new InvalidOperationException(CallBuildAsyncErrorMessage);
            if (Request.Content?.Body == null)
                throw new InvalidOperationException("Request.Content or Content.Body is null");

            return Request;
        }

        [PublicAPI]
        public async Task<HtmlRequest> BuildAsync()
        {
            if (_asyncTasks.Any())
            {
                await Task.WhenAll(_asyncTasks).ConfigureAwait(false);
            }

            if (this.Request.Content?.Body == null)
                throw new InvalidOperationException("Request.Content or Content.Body is null");

            return Request;
        }
    }
}