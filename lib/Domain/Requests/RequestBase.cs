using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Headers;

using Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;
using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;
using Gotenberg.Sharp.API.Client.Infrastructure;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public abstract class RequestBase : IApiRequest
    {
        const string DispositionType = Constants.HttpContent.Disposition.Types.FormData;

        /// <summary>
        /// Only meant for internal use. Scoped as public b/c the interface defines it.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public abstract string ApiPath { get; }

        public bool IsWebhookRequest => Config?.Webhook?.TargetUrl != default;

        public RequestConfig Config { get; set; }

        public AssetDictionary Assets { get; set; }

        public PdfFormats Format { [UsedImplicitly] get; set; }

        public CustomHttpHeaders CustomHeaders { get; } = new CustomHttpHeaders();

        public abstract IEnumerable<HttpContent> ToHttpContent();

        internal static StringContent CreateFormDataItem<T>(T value, string fieldName)
        {
            var item = new StringContent(value.ToString());

            item.Headers.ContentDisposition =
                new ContentDispositionHeaderValue(DispositionType) { Name = fieldName };
            return item;
        }
    }
}