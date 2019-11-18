using System.Net.Http;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    [UsedImplicitly]
    public sealed class AssetBytesRequest: AssetRequest<byte[]>
    {
        public AssetBytesRequest() : base(value => new ByteArrayContent(value))
        {
        }
    }
}