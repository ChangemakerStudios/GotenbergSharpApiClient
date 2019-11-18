using System.IO;
using System.Net.Http;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    [UsedImplicitly]
    public sealed class DocumentStreamRequest : DocumentRequest<Stream>
    {
        public DocumentStreamRequest(Stream bodyHtml) : base(value => new StreamContent(value), bodyHtml)
        {
        }
    }
}