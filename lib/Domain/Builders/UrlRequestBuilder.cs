using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Gotenberg.Sharp.API.Client.Domain.Builders.FacetedBuilders;
using Gotenberg.Sharp.API.Client.Domain.Requests;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Builders
{
    public class UrlRequestBuilder: BaseBuilder<UrlRequest>
    {
        readonly List<Task> _asyncTasks = new List<Task>();

        protected sealed override UrlRequest Request { get; set; }

        [PublicAPI]
        public UrlRequestBuilder() => this.Request = new UrlRequest();

        [PublicAPI]
        public UrlRequestBuilder SetUrl(string url) => SetUrl(new Uri(url));
        
        [PublicAPI]
        public UrlRequestBuilder SetUrl(Uri url)
        {
            this.Request.Url = url;
            return this;
        }
   

        [PublicAPI]
        public UrlRequestBuilder SetRemoteUrlHeader(string name, string value)
        {
            this.Request.RemoteUrlHeader = new KeyValuePair<string, string>(name, value);
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
        public UrlRequestBuilder AddAsyncHeaderFooter(Func<UrlHeaderFooterBuilder,Task> asyncAction)
        {
            if (asyncAction == null) throw new ArgumentNullException(nameof(asyncAction));

            this._asyncTasks.Add(asyncAction(new UrlHeaderFooterBuilder(this.Request)));
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
        public UrlRequestBuilder ConfigureRequest(Action<ConfigBuilder> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            action(new ConfigBuilder(this.Request));
            return this;
        }


        [PublicAPI]
        public UrlRequest Build() 
        {
            if (_asyncTasks.Any()) throw new InvalidOperationException("Call BuildAsync");
            if (this.Request.Url == null) throw new NullReferenceException("Request.Url is null");
            return Request;
        }

        [PublicAPI]
        public async Task<UrlRequest> BuildAsync()
        {
            if (_asyncTasks.Count == 0) throw new InvalidOperationException("Call Build");
            if (this.Request.Url == null) throw new NullReferenceException("Request.Url is null");

            await Task.WhenAll(_asyncTasks).ConfigureAwait(false);

            return Request;
        }
    }

}
