using System.Net.Http.Headers;
using Gotenberg.Sharp.API.Client.Domain.Embed;
using Gotenberg.Sharp.API.Client.Extensions;
using Gotenberg.Sharp.API.Client.Infrastructure;
using Newtonsoft.Json.Linq;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    /// <summary>
    /// Embeds files into PDFs, for standards such as ZUGFeRD / Factur-X that require an XML invoice
    /// or other attachment to live inside the PDF.
    /// </summary>
    [MinimumGotenbergVersion(GotenbergVersions.Embed, Feature = "Embedding files in PDFs")]
    public sealed class EmbedRequest : PdfEngineRequest
    {
        /// <inheritdoc />
        protected override string ApiPath => Constants.Gotenberg.PdfEngines.ApiPaths.Embed;

        public IDictionary<string, Entry>? EmbedsData { get; set; }

        /// <inheritdoc />
        protected override void Validate()
        {
            if (this.EmbedsData == null || !this.EmbedsData.Any())
                throw new InvalidOperationException($"{nameof(EmbedsData)} is required and cannot be empty");

            base.Validate();
        }

        /// <inheritdoc />
        protected override IEnumerable<HttpContent> ToHttpContent()
        {
            var metadataObject = new JObject();
            foreach (var kvp in EmbedsData!)
            {
                metadataObject.Add(kvp.Key, JObject.FromObject(new
                {
                    kvp.Value.MimeType,
                    kvp.Value.Relationship
                }));
            }

            var metadataContent = new StringContent(metadataObject.ToString());
            metadataContent.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContent.MediaTypes.ApplicationJson);
            metadataContent.Headers.ContentDisposition = new ContentDispositionHeaderValue(Constants.HttpContent.Disposition.Types.FormData)
            {
                Name = "embedsMetadata",
            };
            metadataContent.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContent.MediaTypes.ApplicationJson);
            
            yield return metadataContent;

            foreach (var kvp in EmbedsData)
            {
                var contentItem = kvp.Value.Content.ToHttpContentItem();
                
                contentItem.Headers.ContentDisposition = new ContentDispositionHeaderValue(Constants.HttpContent.Disposition.Types.FormData)
                {
                    Name = "embeds",
                    FileName = kvp.Key,
                };
                
                if (!string.IsNullOrEmpty(kvp.Value.MimeType))
                {
                    contentItem.Headers.ContentType = new MediaTypeHeaderValue(kvp.Value.MimeType);
                }

                yield return contentItem;
            }

            foreach (var content in base.ToHttpContent())
            {
                yield return content;
            }
        }
    }
}