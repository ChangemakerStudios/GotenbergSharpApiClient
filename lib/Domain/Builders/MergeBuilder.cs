
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gotenberg.Sharp.API.Client.Domain.Builders.Facets;
using Gotenberg.Sharp.API.Client.Domain.Requests;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Builders
{
    public sealed class MergeBuilder: BaseBuilder<MergeRequest>
    {
        readonly List<Task> _asyncTasks = new List<Task>();

        public MergeBuilder() => Init();
    
        protected override MergeRequest Request { get; set; }

        [PublicAPI]
        public MergeBuilder WithAssets(Action<AssetBuilder> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            action(new AssetBuilder(this.Request));
            return this;
        }

        [PublicAPI]
        public MergeBuilder WithAsyncAssets(Func<AssetBuilder, Task> asyncAction)
        {
            if (asyncAction == null) throw new ArgumentNullException(nameof(asyncAction));
            this._asyncTasks.Add(asyncAction(new AssetBuilder(this.Request)));
            return this;
        }

        [PublicAPI]
        public MergeBuilder ConfigureRequest(Action<ConfigBuilder> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            action(new ConfigBuilder(this.Request));
            return this;
        }

        [PublicAPI]
        public MergeRequest Build()
        {
            if (_asyncTasks.Any()) throw new InvalidOperationException("Call BuildAsync");
            if (Request.Count == 0) throw new InvalidOperationException("There are no items to merge");
            return Request;
        }


        [PublicAPI]
        public async Task<MergeRequest> BuildAsync()
        {
            if (_asyncTasks.Count == 0) throw new InvalidOperationException("Call the synchronous Build");

            await Task.WhenAll(_asyncTasks).ConfigureAwait(false);

            return Request;
        }

        void Init() => this.Request = new MergeRequest();
       
    }

}