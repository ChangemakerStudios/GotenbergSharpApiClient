using System.Net.Http;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    [UsedImplicitly]
    public sealed class DocumentStringRequest : DocumentBaseRequest<string>
    {
        public DocumentStringRequest(string bodyHtml) : base(value => new StringContent(value), bodyHtml)
        {
        }
    }
}