
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
        public AssetBuilder<MergeBuilder> Assets => new AssetBuilder<MergeBuilder>(this.Request, this);

        [PublicAPI]
        public ConfigBuilder<MergeBuilder> ConfigureRequest => new ConfigBuilder<MergeBuilder>(this.Request, this);
 
        [PublicAPI]
        public MergeRequest Build() => this.Request;

        void Init() => this.Request = new MergeRequest();
       
    }

}