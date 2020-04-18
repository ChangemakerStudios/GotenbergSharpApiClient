// CaptiveAire.Gotenberg.Sharp.API.Client - Copyright (c) 2019 CaptiveAire

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Gotenberg.Sharp.API.Client.Domain.Requests.Content;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    /// <summary>
    /// Represents a Gotenberg Api conversion request for HTML or Markdown to pdf
    /// </summary>
    /// <remarks>
    ///     For Markdown conversions your Content.Body must contain HTML that references a markdown file 
    ///     using the Go template function toHTML within the body element. This function will convert a given markdown file to HTML.
    ///     See example here: https://thecodingmachine.github.io/gotenberg/#markdown.basic
    /// </remarks>
    public class ContentRequest : ResourceRequest
    {
        public ContentRequest(bool forMarkdown = false) => this.ContainsMarkdown = forMarkdown;

        public bool ContainsMarkdown { get; }

        public Document Content { get; set; }

        protected AssetRequest Assets { get; set; }

        public void AddAssets(AssetRequest assets)
        {
            this.Assets ??= new AssetRequest();
            this.Assets.AddRange(assets);
        }

        public void AddAsset(string name, ContentItem value)
        {
            this.Assets ??= new AssetRequest();
            this.Assets.Add(name, value);
        }

        /// <summary>
        /// Transforms the instance to a list of HttpContent items
        /// </summary>
        /// <returns></returns>
        /// <remarks>Useful for looking at the headers created via linq-pad.dump</remarks>
        public override IEnumerable<HttpContent> ToHttpContent()
        {
            if (Content?.Body == null) throw new ArgumentNullException(nameof(Content), "You need to Add at least a body");

            return Content.ToHttpContent()
                          .Concat(base.ToHttpContent())
                          .Concat(Assets?.ToHttpContent() ?? Enumerable.Empty<HttpContent>());
        }
      
    }

}