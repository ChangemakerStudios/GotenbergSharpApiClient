
using System;
using Gotenberg.Sharp.API.Client.Domain.Builders.FacetedBuilders;
using Gotenberg.Sharp.API.Client.Domain.Requests;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Builders
{
    [PublicAPI]
    public sealed class MergeBuilder: BaseBuilder<MergeRequest>
    {
        public MergeBuilder() => Init();
    
        protected override MergeRequest Request { get; set; }

        [PublicAPI]
        public AssetBuilder Assets => new AssetBuilder(this.Request);

        public MergeBuilder ConfigureRequest(Action<ConfigBuilder> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            action(new ConfigBuilder(this.Request));
            return this;
        }

        [PublicAPI]
        public MergeRequest Build() => this.Request;

        void Init() => this.Request = new MergeRequest();
       
    }

}