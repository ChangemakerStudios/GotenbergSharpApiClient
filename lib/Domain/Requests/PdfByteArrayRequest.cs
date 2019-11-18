
namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public sealed class PdfByteArrayRequest<TAsset> : PdfBaseRequest<byte[], TAsset> where TAsset : class
    {
        public PdfByteArrayRequest(DocumentBytesRequest content, DocumentDimensions dimensions = null) : base(content, dimensions)
        {
        }
    }
}