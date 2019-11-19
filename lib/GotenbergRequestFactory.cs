using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Requests.Documents;
using Gotenberg.Sharp.API.Client.Domain.Requests.Merge;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client
{
    public static class GotenbergRequestFactory
    {
        [UsedImplicitly]
        public static class Pdf
        {
            [UsedImplicitly]
            public static PdfRequest<Stream> FromHtml([NotNull] Stream body, Stream header = null, Stream footer = null)
            {
                var content = new DocumentStreamRequest(body)
                              {
                                  HeaderHtml = header,
                                  FooterHtml = footer
                              };

                return new PdfRequest<Stream>(content);
            }

            [UsedImplicitly]
            public static PdfRequest<byte[]> FromHtml([NotNull] byte[] body, byte[] header = null, byte[] footer = null)
            {
                var content = new DocumentBytesRequest(body)
                              {
                                  HeaderHtml = header,
                                  FooterHtml = footer
                              };

                return new PdfRequest<byte[]>(content);
            }
            
            [UsedImplicitly]
            public static PdfRequest<string> FromHtml([NotNull] string body, string header = null, string footer = null)
            {
                var content = new DocumentStringRequest(body)
                              {
                                  HeaderHtml = header,
                                  FooterHtml = footer
                              };

                return new PdfRequest<string>(content);
            }
            
        }

        [UsedImplicitly]
        public static class Merge
        {
            [UsedImplicitly]
            public static MergeRequest<Stream> FromStreams([CanBeNull]Dictionary<string, Stream> items)
            {
                var request = new MergeStreamRequest();
                request.Items.AddRange(items ?? Enumerable.Empty<KeyValuePair<string, Stream>>());
                return request;
            }
        
            [UsedImplicitly]
            public static MergeRequest<byte[]> FromBytes([CanBeNull]Dictionary<string, byte[]> items)
            {
                var request = new MergeBytesRequest();
                request.Items.AddRange(items ?? Enumerable.Empty<KeyValuePair<string, byte[]>>());
                return request;
            }         
        }
    }
}
