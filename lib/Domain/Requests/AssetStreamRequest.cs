using System.IO;
using System.Net.Http;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    [UsedImplicitly]
    public sealed class AssetStreamRequest : AssetBaseRequest<Stream>
    {
        public AssetStreamRequest() : base(value=> new StreamContent(value))
        {
        }
    }
}