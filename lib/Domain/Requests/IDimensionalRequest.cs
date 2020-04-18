// Gotenberg.Sharp.Api.Client - Copyright (c) 2019 CaptiveAire

using Gotenberg.Sharp.API.Client.Domain.Requests.Content;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public interface IDimensionalRequest
    {
        Dimensions Dimensions { get; set; }
    }

}