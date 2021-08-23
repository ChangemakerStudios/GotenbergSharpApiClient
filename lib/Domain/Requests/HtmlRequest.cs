using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;
using Gotenberg.Sharp.API.Client.Extensions;
using Gotenberg.Sharp.API.Client.Infrastructure;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    /// <summary>
    /// Represents a Gotenberg Api conversion request for HTML or Markdown to pdf
    /// </summary>
    /// <remarks>
    ///     For Markdown conversions your Content.Body must contain HTML that references one or more markdown files
    ///     using the Go template function 'toHTML' within the body element. Chrome uses the function to convert the contents of a given markdown file to HTML.
    ///     See example here: https://thecodingmachine.github.io/gotenberg/#markdown.basic
    /// </remarks>
    public sealed class HtmlRequest : ChromeRequest
    {
        public override string ApiPath =>
            this.ContainsMarkdown
                ? Constants.Gotenberg.ApiPaths.MarkdownConvert
                : Constants.Gotenberg.ApiPaths.ConvertHtml;

        [PublicAPI]
        public HtmlRequest()
        {
        }

        [PublicAPI]
        public HtmlRequest(bool containsMarkdown) => this.ContainsMarkdown = containsMarkdown;

        public bool ContainsMarkdown { get; internal set; }

        public FullDocument Content { get; set; }

        /// <summary>
        /// Transforms the instance to a list of HttpContent items
        /// </summary>
        /// <returns></returns>
        /// <remarks>Useful for looking at the headers created via linq-pad.dump</remarks>
        public override IEnumerable<HttpContent> ToHttpContent()
        {
            if (Content?.Body == null) throw new InvalidOperationException("You need to Add at least a body");

            return 
                Content.IfNullEmptyContent()
                .Concat(Assets.IfNullEmptyContent())
                .Concat(Config.IfNullEmptyContent())
                .Concat(Dimensions.IfNullEmptyContent())
                .Concat(GetExtraHeaderHttpContent().IfNullEmpty());
        }
    }
}