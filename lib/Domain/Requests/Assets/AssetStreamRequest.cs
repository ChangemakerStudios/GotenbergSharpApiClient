using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Requests.Assets
{
    [UsedImplicitly]
    public sealed class AssetStreamRequest : AssetRequest<Stream>
    {
        public AssetStreamRequest() 
            : base(value=> new StreamContent(value))
        {
        }
    }
}