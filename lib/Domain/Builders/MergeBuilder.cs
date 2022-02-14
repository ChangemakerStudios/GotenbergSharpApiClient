using System;
using System.Linq;
using System.Threading.Tasks;

using Gotenberg.Sharp.API.Client.Domain.Requests;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Builders
{
    public sealed class MergeBuilder : BaseMergeBuilder<MergeBuilder, MergeRequest>
    {
        public MergeBuilder() => this.Request = new MergeRequest();

        protected override MergeRequest Request { get; set; }

        [PublicAPI]
        public MergeRequest Build()
        {
            if (AsyncTasks.Any()) throw new InvalidOperationException(CallBuildAsyncErrorMessage);
            if (Request.Count == 0) throw new InvalidOperationException("There are no items to merge");
            return Request;
        }
        
        [PublicAPI]
        public async Task<MergeRequest> BuildAsync()
        {
            if (AsyncTasks.Any())
            {
                await Task.WhenAll(AsyncTasks).ConfigureAwait(false);
            }

            return Request;
        }
    }
}