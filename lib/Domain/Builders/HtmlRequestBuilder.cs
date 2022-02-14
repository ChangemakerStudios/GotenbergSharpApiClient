using System;
using System.Linq;
using System.Threading.Tasks;

using Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;
using Gotenberg.Sharp.API.Client.Domain.Requests;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Builders
{
    public sealed class HtmlRequestBuilder : BaseChromiumBuilder<HtmlRequestBuilder, HtmlRequest>
    {
        protected override HtmlRequest Request { get; set; }

        public HtmlRequestBuilder() 
            => this.Request = new HtmlRequest();

        [PublicAPI]
        public HtmlRequestBuilder(bool containsMarkdown)
            => this.Request = new HtmlRequest(containsMarkdown);

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

            this.AsyncTasks.Add(asyncAction(new DocumentBuilder(this.Request)));
            return this;
        }

        [PublicAPI]
        public HtmlRequest Build()
        {
            if (AsyncTasks.Any()) throw new InvalidOperationException(CallBuildAsyncErrorMessage);
            if (Request.Content?.Body == null)
                throw new InvalidOperationException("Request.Content or Content.Body is null");

            return Request;
        }

        [PublicAPI]
        public async Task<HtmlRequest> BuildAsync()
        {
            if (AsyncTasks.Any())
            {
                await Task.WhenAll(AsyncTasks).ConfigureAwait(false);
            }

            if (this.Request.Content?.Body == null)
                throw new InvalidOperationException("Request.Content or Content.Body is null");

            return Request;
        }
    }
}