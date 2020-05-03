using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gotenberg.Sharp.API.Client.Domain.Builders.FacetedBuilders;
using Gotenberg.Sharp.API.Client.Domain.Requests;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Builders
{
    /// <remarks>
    ///     Any non office files sent in are just ignored.
    ///     A nice surprise: Gotenberg/Chrome will merge in all sheets within a multi-sheet excel workbook.
    ///     If you send in a csv file but with an xlsx extension, it will merge it in as text.
    /// </remarks>
    public sealed class MergeOfficeBuilder: BaseBuilder<MergeOfficeRequest>
    {
        readonly List<Task> _asyncTasks = new List<Task>();

        public MergeOfficeBuilder() => Init();

        protected override MergeOfficeRequest Request { get; set; }

        [PublicAPI]
        public MergeOfficeBuilder WithAssets(Action<AssetBuilder> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            action(new AssetBuilder(this.Request));
            return this;
        }

        [PublicAPI]
        public MergeOfficeBuilder WithAsyncAssets(Func<AssetBuilder, Task> asyncAction)
        {
            if (asyncAction == null) throw new ArgumentNullException(nameof(asyncAction));
            this._asyncTasks.Add(asyncAction(new AssetBuilder(this.Request)));
            return this;
        }

        [PublicAPI]
        public MergeOfficeBuilder ConfigureRequest(Action<ConfigBuilder> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            action(new ConfigBuilder(this.Request));
            return this;
        }

        [PublicAPI]
        public MergeOfficeRequest Build()
        {
            if (_asyncTasks.Any()) throw new InvalidOperationException("Call BuildAsync");
            if (Request.Count == 0) throw new InvalidOperationException("There are no items to merge");
            return Request;
        }


        [PublicAPI]
        public async Task<MergeOfficeRequest> BuildAsync()
        {
            if (_asyncTasks.Count == 0) throw new InvalidOperationException("Call the synchronous Build");

            await Task.WhenAll(_asyncTasks).ConfigureAwait(false);

            return Request;
        }

        void Init() => this.Request = new MergeOfficeRequest();
    }
}