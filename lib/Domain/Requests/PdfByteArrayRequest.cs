
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    [UsedImplicitly]
    public sealed class PdfByteArrayRequest<TAsset> : PdfRequest<byte[], TAsset> where TAsset : class
    {
        public PdfByteArrayRequest(DocumentBytesRequest content, DocumentDimensions dimensions = null) : base(content, dimensions)
        {
        }
    }
}