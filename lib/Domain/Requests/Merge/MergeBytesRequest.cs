using System.Collections.Generic;
using System.Net.Http;
using Gotenberg.Sharp.API.Client.Domain.Requests.Assets;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Requests.Merge
{
    [UsedImplicitly]
    public sealed class MergeBytesRequest : MergeRequest<byte[]>
    {
        public MergeBytesRequest() : base(value => new ByteArrayContent(value)) 
            => this.Assets = new AssetBytesRequest();
    }
}