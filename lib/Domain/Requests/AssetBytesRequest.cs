using System.Net.Http;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public sealed class AssetBytesRequest: AssetBaseRequest<byte[]>
    {
        public AssetBytesRequest() : base(value => new ByteArrayContent(value))
        {
        }
    }
}