// Gotenberg.App.API.Sharp.Client - Copyright (c) 2019 CaptiveAire

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using CaptiveAire.Gotenberg.App.API.Sharp.Client.Infrastructure;
using JetBrains.Annotations;

namespace CaptiveAire.Gotenberg.App.API.Sharp.Client.Domain.Requests
{
    /// <summary>
    /// A request to merge the specified items into one pdf file
    /// </summary>
    public class MergeRequest<TValue> where TValue: class
    {
        /// <summary>
        /// Gets the request configuration containing fields that all Gotenberg endpoints accept
        /// </summary>
        // ReSharper disable once MemberCanBeProtected.Global
        public RequestConfig Config { get; set; } = new RequestConfig();

        /// <summary>
        /// Key = file name; value = the pdf bytes
        /// </summary>
        public Dictionary<string, TValue> Items { get; [UsedImplicitly] set; } = new Dictionary<string, TValue>();

        /// <summary>
        /// Transforms the merge items to http content items
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<HttpContent> ToHttpContent(Func<TValue,HttpContent> converter)
        {
            return this.Items.Where(_ => _.Value != null)
                .Select(_ =>
                {
                    var item = converter(_.Value);
                    
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