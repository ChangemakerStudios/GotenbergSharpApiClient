// Gotenberg.Sharp.Api.Client - Copyright (c) 2020 CaptiveAire

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public interface IMergeOfficeRequest: IMergeRequest
    {
        IMergeOfficeRequest FilterByExtension();
    }
}