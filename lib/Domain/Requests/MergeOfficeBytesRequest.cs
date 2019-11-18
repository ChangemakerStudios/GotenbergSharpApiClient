using System.Net.Http;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public sealed class MergeOfficeBytesRequest: MergeOfficeRequestBase<byte[]>
    {
        public MergeOfficeBytesRequest() : base(value => new ByteArrayContent(value))
        {
        }

        protected internal override MergeOfficeRequestBase<byte[]> FilterByExtension() 
            => this.FilterByExtension<MergeOfficeBytesRequest>();
    }
}