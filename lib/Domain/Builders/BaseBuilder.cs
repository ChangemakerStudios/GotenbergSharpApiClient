using Gotenberg.Sharp.API.Client.Domain.Requests;

namespace Gotenberg.Sharp.API.Client.Domain.Builders
{
    public abstract class BaseBuilder<TRequest> where TRequest: RequestBase
    {
        protected virtual TRequest Request { get; set; }

        protected string CallBuildAsyncErrorMessage = "Call BuildAsync, your request contains asynchronous items";
    }

}
