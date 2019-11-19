using System.IO;
using System.Net.Http;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Requests.Documents
{
    [UsedImplicitly]
    internal class DocumentStreamRequest : DocumentRequest<Stream>
    {
        internal DocumentStreamRequest(Stream bodyHtml) : base(value => new StreamContent(value), bodyHtml)
        {
        }
    }
}