// Gotenberg.Sharp.Api.Client - Copyright (c) 2019 CaptiveAire

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public interface IConversionRequest: IConfigureRequests, IConvertToHttpContent
    {
        [UsedImplicitly]
        DocumentDimensions Dimensions { get; set; }

        [UsedImplicitly]
        void AddAssets(AssetRequest assets);

        [UsedImplicitly]
        void AddAsset(string name, ContentItem value);
    }

}