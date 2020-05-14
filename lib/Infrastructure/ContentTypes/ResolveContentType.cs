// Gotenberg.Sharp.Api.Client - Copyright (c) 2020 CaptiveAire

using Gotenberg.Sharp.API.Client.Domain.ContentTypes;

using Microsoft.AspNetCore.StaticFiles;

namespace Gotenberg.Sharp.API.Client.Infrastructure.ContentTypes
{
    public class ResolveContentTypeImplementation : IResolveContentType
    {
        static readonly FileExtensionContentTypeProvider _contentTypeProvider = new FileExtensionContentTypeProvider();

        public string GetContentType(string fileName, string defaultContentType = "application/octet-stream")
        {
            return _contentTypeProvider.TryGetContentType(fileName, out string contentType) ? contentType : defaultContentType;
        }
    }
}