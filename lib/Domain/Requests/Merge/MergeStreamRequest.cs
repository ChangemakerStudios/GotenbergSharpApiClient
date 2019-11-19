using System.IO;
using System.Net.Http;
using Gotenberg.Sharp.API.Client.Domain.Requests.Assets;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Requests.Merge
{
    [UsedImplicitly]
    public sealed class MergeStreamRequest : MergeRequest<Stream>
    {
        public MergeStreamRequest() : base(value => new StreamContent(value))
            => this.Items = new AssetStreamRequest();
    }
}