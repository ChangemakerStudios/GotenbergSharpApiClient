using System.Net.Http;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    [UsedImplicitly]
    public sealed class MergeOfficeBytesRequest: MergeOfficeRequestBase<byte[]>
    {
        public MergeOfficeBytesRequest() : base(value => new ByteArrayContent(value))
        {
            this.Assets = new AssetBytesRequest();
        }

        protected internal override MergeOfficeRequestBase<byte[]> FilterByExtension()
            => this.FilterByExtension<MergeOfficeBytesRequest>();

    }
}