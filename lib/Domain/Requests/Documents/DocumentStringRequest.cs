using System.Net.Http;

namespace Gotenberg.Sharp.API.Client.Domain.Requests.Documents
{
    internal class DocumentStringRequest : DocumentRequest<string>
    {
        internal DocumentStringRequest(string bodyHtml) : base(value => new StringContent(value), bodyHtml)
        {
        }
    }
}