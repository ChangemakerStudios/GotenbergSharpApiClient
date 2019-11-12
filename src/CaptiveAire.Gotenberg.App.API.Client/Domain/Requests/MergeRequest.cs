// Gotenberg.App.API.Sharp.Client - Copyright (c) 2019 CaptiveAire

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace CaptiveAire.Gotenberg.App.API.Sharp.Client.Domain.Requests
{
    /// <summary>
    /// A request to merge the specified items into one pdf file
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class MergeRequest
    {
        /// <summary>
        /// Gets the request configuration containing fields that all Gotenberg endpoints accept
        /// </summary>
        // ReSharper disable once MemberCanBeProtected.Global
        public RequestConfig Config { get; set; } = new RequestConfig();

        /// <summary>
        /// Key = file name; value = the pdf bytes
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public Dictionary<string, byte[]> Items { get; set; } = new Dictionary<string, byte[]>();

        /// <summary>
        /// Transforms the merge items to http content items
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<HttpContent> ToHttpContent()
        {
            return this.Items.Where(_ => _.Value != null)
                .Select(_ =>
                {
                    var item = new ByteArrayContent(_.Value);
                    item.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data") { Name = "files", FileName = _.Key };
                    item.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

                    return item;
                    
                }).Concat(Config.ToHttpContent());
        }
    }
}