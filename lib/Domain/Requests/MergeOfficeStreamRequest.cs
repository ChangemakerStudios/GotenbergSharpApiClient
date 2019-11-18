using System.IO;
using System.Net.Http;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public sealed class MergeOfficeStreamRequest : MergeOfficeRequestBase<Stream>
    {
        public MergeOfficeStreamRequest() : base(value => new StreamContent(value)) { }

        protected internal override MergeOfficeRequestBase<Stream> FilterByExtension() =>
            this.FilterByExtension<MergeOfficeStreamRequest>();
    }
}