using System.Net.Http;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public sealed class MergeBytesRequest : MergeBaseRequest<byte[]>
    {
        public MergeBytesRequest() : base(value => new ByteArrayContent(value))
        {
            this.Assets = new AssetBytesRequest();
        }
    }
}