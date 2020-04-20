using Gotenberg.Sharp.API.Client.Domain.Requests;

namespace Gotenberg.Sharp.API.Client.Domain.Builders
{
    public abstract class BaseRequestBuilder<TRequest> where TRequest: ResourceRequest, new()
    {
        protected virtual TRequest Request { get; set; }
    }
}
