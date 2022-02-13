using Gotenberg.Sharp.API.Client.Infrastructure;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Requests.Facets
{
    /// <summary>
    /// Represents the elements of a document
    /// </summary>
    /// <remarks>The file names are a Gotenberg Api convention</remarks>
    public sealed class FullDocument : HeaderFooterDocument
    {
        [MultiFormHeader(fileName: Constants.Gotenberg.Chromium.Routes.Html.IndexFile)]
        public ContentItem Body { [UsedImplicitly] get; set; }
    }
}