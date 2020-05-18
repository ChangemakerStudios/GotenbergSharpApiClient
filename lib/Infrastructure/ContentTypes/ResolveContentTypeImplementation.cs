using System;

using Gotenberg.Sharp.API.Client.Domain.ContentTypes;
using Gotenberg.Sharp.API.Client.Extensions;

using Microsoft.AspNetCore.StaticFiles;

namespace Gotenberg.Sharp.API.Client.Infrastructure.ContentTypes
{
    public class ResolveContentTypeImplementation : IResolveContentType
    {
        static readonly FileExtensionContentTypeProvider ContentTypeProvider = new FileExtensionContentTypeProvider();

        public string GetContentType(string fileName, string defaultContentType = "application/octet-stream")
        {
            if (fileName.IsNotSet()) throw new ArgumentException("file name is either null or empty");
            return ContentTypeProvider.TryGetContentType(fileName, out var contentType)
                ? contentType
                : defaultContentType;
        }
    }
}