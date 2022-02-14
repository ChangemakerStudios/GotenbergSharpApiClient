using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

using Gotenberg.Sharp.API.Client.Domain.ContentTypes;
using Gotenberg.Sharp.API.Client.Extensions;
using Gotenberg.Sharp.API.Client.Infrastructure;
using Gotenberg.Sharp.API.Client.Infrastructure.ContentTypes;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    //Libre office has a convert route which can perform merges
    public class MergeOfficeRequest : RequestBase
    {
        readonly IResolveContentType _resolver = new ResolveContentTypeImplementation();

        public override string ApiPath 
            => Constants.Gotenberg.LibreOffice.ApiPaths.MergeOffice;

        public int Count => this.Assets.IfNullEmpty().Count;

        public bool PrintAsLandscape { get; set; }

        public string PageRanges { get; set; }

        /// <summary>
        /// Tells gotenberg to perform the conversion with unoconv.
        /// If you specify this with a Format the API has unoconv convert it to that. 
        /// Note: the documentation says you can't use both together but that regards request headers.
        /// When true and Format is not set, the client falls back to PDF/A-1a.
        /// </summary>
        public bool UseNativePdfFormat { get; set; }

        public override IEnumerable<HttpContent> ToHttpContent()
        {
            var validItems = new Lazy<List<ValidOfficeMergeItem>>(this.Assets.FindValidOfficeMergeItems(_resolver).ToList);

            foreach (var isValid in new[] { Count > 1, validItems.Value.Count > 1 })
                if (!isValid) yield break;

            yield return CreateFormDataItem("true", Constants.Gotenberg.LibreOffice.Routes.Convert.Merge);

            foreach (var item in validItems.Value.ToHttpContent())
                yield return item;

            foreach (var item in Config.IfNullEmptyContent())
                yield return item;

            foreach (var item in this.PropertiesToHttpContent()) 
                yield return item;
        }

    }
}