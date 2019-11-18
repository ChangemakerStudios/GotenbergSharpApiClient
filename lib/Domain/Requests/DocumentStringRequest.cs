using System.Net.Http;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public sealed class DocumentStringRequest : DocumentBaseRequest<string>
    {
        public DocumentStringRequest(string bodyHtml) : base(value => new StringContent(value), bodyHtml)
        {
        }
    }
}