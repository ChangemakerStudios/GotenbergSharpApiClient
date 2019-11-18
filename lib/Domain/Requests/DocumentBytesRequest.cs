using System.Net.Http;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public sealed class DocumentBytesRequest : DocumentBaseRequest<byte[]>
    {
        public DocumentBytesRequest(byte[] bodyHtml) : base(value => new ByteArrayContent(value), bodyHtml)
        {
        }
    }
}