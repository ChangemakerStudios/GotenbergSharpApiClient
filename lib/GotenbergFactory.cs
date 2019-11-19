using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Requests.Assets;
using Gotenberg.Sharp.API.Client.Domain.Requests.Documents;
using Gotenberg.Sharp.API.Client.Domain.Requests.Merge;

namespace Gotenberg.Sharp.API.Client
{
    public static class GotenbergFactory
    {
        #region Pdf requests
        
        public static PdfRequest<Stream> FromStreams(Stream body, Stream header = null, Stream footer = null)
        {
            var content = new DocumentStreamRequest(body)
            {
                HeaderHtml = header,
                FooterHtml = footer
            };

            return new PdfRequest<Stream>(content);
        }

        public static PdfRequest<string> FromStrings(string body, string header = null, string footer = null)
        {
            var content = new DocumentStringRequest(body)
            {
                HeaderHtml = header,
                FooterHtml = footer
            };

            return new PdfRequest<string>(content);
        }

        public static PdfRequest<byte[]> FromBytes(byte[] body, byte[] header = null, byte[] footer = null)
        {
            var content = new DocumentBytesRequest(body)
            {
                HeaderHtml = header,
                FooterHtml = footer
            };

            return new PdfRequest<byte[]>(content);
        }
        
        #endregion

        #region Merges

        public static MergeRequest<Stream> CreateStreamMerge(IEnumerable<KeyValuePair<string, Stream>> items)
        {
            var request = new MergeStreamRequest();
            request.Assets.AddRange(items ?? Enumerable.Empty<KeyValuePair<string, Stream>>());
            return request;
        }
        
        public static MergeRequest<byte[]> CreateByteMerge(IEnumerable<KeyValuePair<string, byte[]>> items)
        {
            var request = new MergeBytesRequest();
            request.Assets.AddRange(items ?? Enumerable.Empty<KeyValuePair<string, byte[]>>());
            return request;
        }
        
        #endregion
    }
}
