using System.Net.Http;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Requests.Documents
{
    [UsedImplicitly]
    internal class DocumentBytesRequest : DocumentRequest<byte[]>
    {
        internal DocumentBytesRequest(byte[] bodyHtml) : base(value => new ByteArrayContent(value), bodyHtml)
        {
        }
    }
}