// Gotenberg.Sharp.Api.Client - Copyright (c) 2019 CaptiveAire

using System.ComponentModel;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public interface IConversionRequest: IConfigureRequests, IConvertToHttpContent
    {
        DocumentDimensions Dimensions { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        void AddAssets(IConvertToHttpContent assets);
    }

}