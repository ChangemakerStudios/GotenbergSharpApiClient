using System.Net.Http;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    [UsedImplicitly]
    public sealed class MergeBytesRequest : MergeBaseRequest<byte[]>
    {
        public MergeBytesRequest() : base(value => new ByteArrayContent(value))
        {
            this.Assets = new AssetBytesRequest();
        }
    }
}