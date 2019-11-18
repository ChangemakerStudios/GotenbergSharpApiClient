using System.IO;
using System.Net.Http;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public sealed class MergeStreamRequest : MergeBaseRequest<Stream>
    {
        public MergeStreamRequest() : base(value => new StreamContent(value)) 
            => this.Assets = new AssetStreamRequest();
    }
}