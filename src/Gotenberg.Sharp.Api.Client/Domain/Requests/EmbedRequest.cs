using System.Net.Http.Headers;
using Gotenberg.Sharp.API.Client.Domain.Embed;
using Gotenberg.Sharp.API.Client.Domain.Stages;
using Gotenberg.Sharp.API.Client.Extensions;
using Gotenberg.Sharp.API.Client.Infrastructure;
using Newtonsoft.Json.Linq;
using Gotenberg.Sharp.API.Client.Domain.Shared;

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

        /// <inheritdoc />
        protected override string GetApiPath(GotenbergApiVersion api) =>
            api == GotenbergApiVersion.V9 ? Constants.Gotenberg.PdfEngines.Gotenberg9.Pipe : this.ApiPath;

        // Gotenberg 9 attaches each file with a stage that names the uploaded file.
        internal override void AddRouteStages(StageList stages)
        {
            foreach (var kvp in this.EmbedsData.IfNullEmpty())
                stages.Attach(kvp.Key, kvp.Value.MimeType, kvp.Value.Relationship);
        }

        public IDictionary<string, Entry>? EmbedsData { get; set; }

        /// <inheritdoc />
        protected override void Validate()
        {
            if (this.EmbedsData == null || !this.EmbedsData.Any())
                throw new InvalidOperationException($"{nameof(EmbedsData)} is required and cannot be empty");

            base.Validate();
        }

        /// <inheritdoc />
        protected override IEnumerable<HttpContent> ToHttpContent(GotenbergApiVersion api)
        {
            if (api == GotenbergApiVersion.V9)
            {
                // Gotenberg 9 reads every file from 'files' and stores it under its name, so an embedded
                // file named like a PDF of the request would replace it.
                var clash = this.EmbedsData!.Keys.FirstOrDefault(name => this.Assets.IfNullEmpty().Any(pdf => pdf.Key == name));

                if (clash != null)
                    throw new ArgumentException(
                        $"The embedded file '{clash}' has the name of a PDF of the request. On Gotenberg 9 both are uploaded as files under their names, so give the embedded file another name.",
                        nameof(this.EmbedsData));

                foreach (var kvp in this.EmbedsData!)
                    yield return this.ToFilePart(kvp, Constants.Gotenberg.SharedFormFieldNames.Files);

                foreach (var content in base.ToHttpContent(api))
                    yield return content;

                yield break;
            }

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
                Name = Constants.Gotenberg.PdfEngines.FormFieldNames.EmbedsMetadata,
            };
            metadataContent.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContent.MediaTypes.ApplicationJson);
            
            yield return metadataContent;

            foreach (var kvp in EmbedsData)
                yield return this.ToFilePart(kvp, Constants.Gotenberg.PdfEngines.FormFieldNames.Embeds);

            foreach (var content in base.ToHttpContent(api))
            {
                yield return content;
            }
        }

        private HttpContent ToFilePart(KeyValuePair<string, Entry> entry, string fieldName)
        {
            var contentItem = entry.Value.Content.ToHttpContentItem();

            contentItem.Headers.ContentDisposition = new ContentDispositionHeaderValue(Constants.HttpContent.Disposition.Types.FormData)
            {
                Name = fieldName,
                FileName = entry.Key,
            };

            if (!string.IsNullOrEmpty(entry.Value.MimeType))
            {
                contentItem.Headers.ContentType = new MediaTypeHeaderValue(entry.Value.MimeType);
            }

            return contentItem;
        }
    }
}