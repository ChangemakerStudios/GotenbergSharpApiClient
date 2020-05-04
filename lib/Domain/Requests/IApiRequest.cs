namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public interface IApiRequest : IConvertToHttpContent
    {
        string ApiPath { get; }
    }
}