// Gotenberg.Sharp.Api.Client - Copyright (c) 2019 CaptiveAire

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public interface IConversionRequest: IConfigureRequests, IConvertToHttpContent
    {
        DocumentDimensions Dimensions { get; set; }
        void AddAssets(AssetRequest assets);
        void AddAsset(string name, ContentItem value);
    }

}