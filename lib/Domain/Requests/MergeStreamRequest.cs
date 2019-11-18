using System.IO;
using System.Net.Http;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    [UsedImplicitly]
    public sealed class MergeStreamRequest : MergeBaseRequest<Stream>
    {
        public MergeStreamRequest() : base(value => new StreamContent(value)) 
            => this.Assets = new AssetStreamRequest();
    }
}