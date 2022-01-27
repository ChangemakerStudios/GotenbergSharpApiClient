using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;
using Gotenberg.Sharp.API.Client.Infrastructure;

using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public abstract class RequestBase : IApiRequest
    {
        const string DispositionType = Constants.HttpContent.Disposition.Types.FormData;

        public abstract string ApiPath { get; }

        public bool IsWebhookRequest => Config?.Webhook?.TargetUrl != null;

        public RequestConfig Config { get; set; }

        public AssetDictionary Assets { get; set; }

        public CustomHttpHeaders CustomHeaders { get; } = new CustomHttpHeaders();

        public abstract IEnumerable<HttpContent> ToHttpContent();

        public static StringContent CreateFormDataItem<T>(T value, string fieldName)
        {
            var item = new StringContent(value.ToString());
            
            item.Headers.ContentDisposition = 
                new ContentDispositionHeaderValue(DispositionType) { Name = fieldName };
        
            return item;
        }
    }
}