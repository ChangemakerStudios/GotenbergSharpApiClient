using System.IO;
using System.Net.Http;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    [UsedImplicitly]
    public sealed class MergeOfficeStreamRequest : MergeOfficeRequest<Stream>
    {
        public MergeOfficeStreamRequest() : base(value => new StreamContent(value))
        {
            this.Assets = new AssetStreamRequest();
        }

        protected internal override MergeOfficeRequest<Stream> FilterByExtension()
            => this.FilterByExtension<MergeOfficeStreamRequest>();

    }
}