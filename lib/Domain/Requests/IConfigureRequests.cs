// Gotenberg.Sharp.Api.Client - Copyright (c) 2019 CaptiveAire

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public interface IConfigureRequests
    {
        RequestConfig Config { [UsedImplicitly]get; set; }
    }
}