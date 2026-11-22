using System.Net.Http.Headers;
using Gotenberg.Sharp.API.Client.Domain.Embed;
using Gotenberg.Sharp.API.Client.Domain.Overlays;
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

        internal override string NoStagesMessage =>
            $"{nameof(this.EmbedsData)} holds no file to embed: Gotenberg 9 answers 400 (stages is required) to a request with no stage. Add at least one file.";

        // Gotenberg 8 wrote the relationship of an embedded file as given; the attach stage of Gotenberg 9
        // takes five and answers 400 to any other.
        internal override GotenbergVersionNotSupportedException? FindRouteRemoved(
            Func<string, string, GotenbergVersionNotSupportedException> removed)
        {
            foreach (var kvp in this.EmbedsData.IfNullEmpty())
            {
                var relationship = kvp.Value.Relationship;

                if (!relationship.IsSet() || StageList.CanonicalRelationship(relationship) != null) continue;

                return removed(
                    $"The embedded file relationship '{relationship}' of '{kvp.Key}' (Entry.Relationship)",
                    $"Gotenberg 9 takes only {string.Join(", ", StageList.AttachRelationships)} (Constants.Gotenberg.PdfEngines.EmbedRelation), in any case: use one of them.");
            }

            return null;
        }

        public IDictionary<string, Entry>? EmbedsData { get; set; }

        // On Gotenberg 9 each embedded file is uploaded under 'files', where its attach stage names it.
        internal override IEnumerable<(string FileName, string ParamName)> StageFiles() =>
            base.StageFiles().Concat(this.EmbedsData.IfNullEmpty().Select(kvp => (kvp.Key, nameof(this.EmbedsData))));

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

            // The attach stage of Gotenberg 9 finds the file by its name, under 'files'.
            contentItem.Headers.ContentDisposition = OverlayFile.ToDisposition(
                fieldName,
                entry.Key,
                exactName: fieldName == Constants.Gotenberg.SharedFormFieldNames.Files);

            if (!string.IsNullOrEmpty(entry.Value.MimeType))
            {
                contentItem.Headers.ContentType = new MediaTypeHeaderValue(entry.Value.MimeType);
            }

            return contentItem;
        }
    }
}