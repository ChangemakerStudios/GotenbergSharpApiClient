// Gotenberg.Sharp.API.Client - Copyright (c) 2019 CaptiveAire

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Gotenberg.Sharp.API.Client.Infrastructure;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    /// <summary>
    /// A request to merge the specified items into one pdf file
    /// </summary>
    public class MergeRequest : IMergeRequest
    {
        
        /// <summary>
        /// Gets the request configuration containing fields that all Gotenberg endpoints accept
        /// </summary>
        // ReSharper disable once MemberCanBeProtected.Global
        [UsedImplicitly]
        public HttpMessageConfig Config { get; set; } = new HttpMessageConfig();

        /// <summary>
        /// Key = file name; value = the document content
        /// </summary>
        [UsedImplicitly]
        public Dictionary<string, ContentItem> Items { get; set; }

        /// <summary>
        /// Gets the count of items
        /// </summary>
        [UsedImplicitly]
        public int Count => this.Items?.Count ?? 0;

        /// <summary>
        /// Transforms the merge items to http content items
        /// </summary>
        /// <returns></returns>
        public IEnumerable<HttpContent> ToHttpContent()
        {
            return this.Items.Where(_ => _.Value != null)
                .Select(_ =>
                {
                    var item = _.Value.ToHttpContentItem();
                    
                    item.Headers.ContentDisposition = new ContentDispositionHeaderValue(Constants.Http.Disposition.Types.FormData) {
                        Name = Constants.Gotenberg.FormFieldNames.Files,
                        FileName = _.Key
                    };

                    item.Headers.ContentType = new MediaTypeHeaderValue(Constants.Http.MediaTypes.ApplicationPdf);

                    return item;
                    
                }).Concat(Config.ToHttpContent());
        }
    }
}