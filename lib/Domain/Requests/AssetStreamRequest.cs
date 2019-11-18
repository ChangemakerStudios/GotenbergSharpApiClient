using System.IO;
using System.Net.Http;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public sealed class AssetStreamRequest : AssetBaseRequest<Stream>
    {
        public AssetStreamRequest() : base(value=> new StreamContent(value))
        {
        }
    }
}