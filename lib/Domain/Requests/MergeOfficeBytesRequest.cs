using System.Net.Http;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    [UsedImplicitly]
    public sealed class MergeOfficeBytesRequest: MergeOfficeRequest<byte[]>
    {
        public MergeOfficeBytesRequest() : base(value => new ByteArrayContent(value))
        {
            this.Assets = new AssetBytesRequest();
        }

        protected internal override MergeOfficeRequest<byte[]> FilterByExtension()
            => this.FilterByExtension<MergeOfficeBytesRequest>();

    }
}