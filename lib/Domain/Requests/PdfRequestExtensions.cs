using System;
using Gotenberg.Sharp.API.Client.Domain.Requests.Assets;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public static class PdfRequestExtensions
    {
        public static PdfRequest<TContent> WithAssets<TContent>(this PdfRequest<TContent> request, AssetBytesRequest assets)
            where TContent : class
        {
            return request.AddAssets(assets ?? throw new ArgumentNullException());
        }

        public static PdfRequest<TContent> WithAssets<TContent>(this PdfRequest<TContent> request, AssetStreamRequest assets)
            where TContent : class
        {
            request.AddAssets(assets ?? new AssetStreamRequest());

            return request;
        }
    }
}
