using System.IO;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public sealed class PdfStreamRequest<TAsset> : PdfBaseRequest<Stream, TAsset> where TAsset : class
    {
        public PdfStreamRequest(DocumentStreamRequest content, DocumentDimensions dimensions) : base(content, dimensions)
        {
        }
    }
}