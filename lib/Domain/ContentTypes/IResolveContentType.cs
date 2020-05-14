// Gotenberg.Sharp.Api.Client - Copyright (c) 2020 CaptiveAire

namespace Gotenberg.Sharp.API.Client.Domain.ContentTypes
{
    public interface IResolveContentType
    {
        string GetContentType(string fileName, string defaultContentType = "application/octet-stream");
    }
}