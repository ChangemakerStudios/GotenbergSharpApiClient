using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public interface IApiRequest : IConvertToHttpContent
    {
        string ApiPath { get; }
        bool IsWebhookRequest { get; }
        CustomHttpHeaders CustomHeaders { get; }
    }
}