using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

using Gotenberg.Sharp.API.Client.Extensions;
using Gotenberg.Sharp.API.Client.Infrastructure;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    /// <summary>
    /// A request to merge the specified items into one pdf file.
    /// </summary>
    public sealed class MergeRequest: RequestBase, IMergeRequest
    {
        /// <summary>
        /// Gets the count of items
        /// </summary>
        public int Count => this.Assets.IfNullEmpty().Count;

        /// <summary>
        /// Transforms the merge items to http content items
        /// </summary>
        /// <returns></returns>
        public IEnumerable<HttpContent> ToHttpContent()
        {
            return this.Assets.ToAlphabeticalMergeOrderByIndex()
                .Where(item => item.Value != null)
                .Select(item =>
                {
                    var contentItem = item.Value.ToHttpContentItem();
                    
                    contentItem.Headers.ContentDisposition = new ContentDispositionHeaderValue(Constants.HttpContent.Disposition.Types.FormData) 
                    {
                        Name = Constants.Gotenberg.FormFieldNames.Files,
                        FileName = item.Key
                    };

                    contentItem.Headers.ContentType = new MediaTypeHeaderValue(Constants.HttpContent.MediaTypes.ApplicationPdf);

                    return contentItem;
                    
                }).Concat(Config.IfNullEmptyContent());
        }
    }
}