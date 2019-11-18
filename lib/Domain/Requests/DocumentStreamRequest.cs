using System.IO;
using System.Net.Http;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public sealed class DocumentStreamRequest : DocumentBaseRequest<Stream>
    {
        public DocumentStreamRequest(Stream bodyHtml) : base(value => new StreamContent(value), bodyHtml)
        {
        }
    }
}