// Gotenberg.App.API.Sharp.Client - Copyright (c) 2019 CaptiveAire

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace CaptiveAire.Gotenberg.App.API.Sharp.Client.Extensions
{
    /// <summary>
    /// Extends <see cref="MergeRequest"/>
    /// </summary>
    public static class MergeRequestExtensions
    {
        /// <summary>
        ///  Transforms the specified request into an http content collection
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static IReadOnlyList<ByteArrayContent> ToHttpContentCollection(this MergeRequest request)
        {
            return request.Items.Where(_=> _.Value != null ).Select(_ =>
                                        {
                                            var item = new ByteArrayContent(_.Value);
                                            item.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data") { Name = "files", FileName = _.Key };
                                            item.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

                                            return item;
                                        }).ToList();

        }
    }
}