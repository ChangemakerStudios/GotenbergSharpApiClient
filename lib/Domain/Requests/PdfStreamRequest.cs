using System.IO;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    [UsedImplicitly]
    public sealed class PdfStreamRequest<TAsset> : PdfRequest<Stream, TAsset> where TAsset : class
    {
        public PdfStreamRequest(DocumentStreamRequest content, DocumentDimensions dimensions = null) : base(content, dimensions)
        {
        }
    }
}