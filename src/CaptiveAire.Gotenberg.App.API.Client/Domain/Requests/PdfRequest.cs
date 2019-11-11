// CaptiveAire.Gotenberg.App.API.Sharp.Client - Copyright (c) 2019 CaptiveAire

// ReSharper disable MemberCanBePrivate.Global

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using CaptiveAire.Gotenberg.App.API.Sharp.Client.Extensions;
using Microsoft.AspNetCore.StaticFiles;

namespace CaptiveAire.Gotenberg.App.API.Sharp.Client.Domain.Requests
{
    /// <summary>
    /// 
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class PdfRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfRequest"/> class.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="dimensions">The dimensions.</param>
        /// <param name="config">Configuration for the request</param>
        public PdfRequest(DocumentContent content, DocumentDimensions dimensions, RequestConfiguration config = null)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
            Dimensions = dimensions ?? throw new ArgumentNullException(nameof(dimensions));
            Config = config;

            if (content.HeaderHtml.IsSet() && dimensions.MarginTop <= 0) dimensions.MarginTop = .38;
            if (content.FooterHtml.IsSet() && dimensions.MarginBottom <= 0) dimensions.MarginBottom = .38;
            //.38 is tHe smallest value that still shows up
        }

        /// <summary>
        /// Gets the request configuration containing fields that all Gotenberg endpoints accept
        /// </summary>
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public RequestConfiguration Config { get; set; }

        /// <summary>
        /// Gets the content.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        public DocumentContent Content { get; }

        /// <summary>
        /// Gets the assets.
        /// </summary>
        /// <value>
        /// The assets.
        /// </value>
        public Dictionary<string, byte[]> Assets { get; private set; } = new Dictionary<string, byte[]>();

        /// <summary>
        /// Gets the dimensions.
        /// </summary>
        /// <value>
        /// The dimensions.
        /// </value>
        public DocumentDimensions Dimensions { get; } 


        /// <summary>
        /// Adds the assets.
        /// </summary>
        /// <param name="assets">The assets.</param>
        /// <exception cref="ArgumentNullException">assets</exception>
        // ReSharper disable once UnusedMember.Global
        public void AddAssets(Dictionary<string, byte[]> assets)
        {
            Assets = assets ?? throw new ArgumentNullException(nameof(assets));
        }

        /// <summary>
        /// Transforms the instance to a list of HttpContent items
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<HttpContent> ToHttpContent()
        {
            var result = Content.ToStringContent()
                .Concat(Dimensions.ToStringContent())
                .Concat(AddAssets());

            if (Config != null)
            {
                result = result.Concat(Config.ToStringContent());
            }

            return result;
        }

        IEnumerable<ByteArrayContent> AddAssets()
        {
            var contentTypeProvider = new FileExtensionContentTypeProvider();

            return this.Assets
                .Select(item =>
                {
                    contentTypeProvider.TryGetContentType(item.Key, out var contentType);

                    return new { Asset = item, ContentType= contentType };
                })
                .Where(_ => _.ContentType.IsSet())
                .Select(item =>
                {
                    var assetItem = new ByteArrayContent(item.Asset.Value);

                    assetItem.Headers.ContentDisposition =
                        new ContentDispositionHeaderValue("form-data") {Name = "files", FileName = item.Asset.Key};

                    assetItem.Headers.ContentType = new MediaTypeHeaderValue(item.ContentType);

                    return assetItem;
                });
        }
    }
}