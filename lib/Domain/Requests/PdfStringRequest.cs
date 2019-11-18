
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    [UsedImplicitly]
    public sealed class PdfStringRequest<TAsset> : PdfRequest<string, TAsset> where TAsset : class
    {
        public PdfStringRequest(DocumentStringRequest content, DocumentDimensions dimensions = null) : base(content, dimensions)
        {
        }
    }
}