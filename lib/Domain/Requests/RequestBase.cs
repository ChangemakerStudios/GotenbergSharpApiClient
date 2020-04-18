// Gotenberg.Sharp.Api.Client - Copyright (c) 2020 CaptiveAire

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public abstract class RequestBase : IConfigureRequests
    {
        public RequestConfig Config { get; set; }
    }

}