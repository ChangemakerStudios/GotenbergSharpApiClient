using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

using Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;
using Gotenberg.Sharp.API.Client.Domain.ContentTypes;
using Gotenberg.Sharp.API.Client.Extensions;
using Gotenberg.Sharp.API.Client.Infrastructure;
using Gotenberg.Sharp.API.Client.Infrastructure.ContentTypes;

namespace Gotenberg.Sharp.API.Client.Domain.Requests
{
    public class MergeOfficeRequest : RequestBase
    {
        readonly IResolveContentType _resolver = new ResolveContentTypeImplementation();

        public override string ApiPath 
            => Constants.Gotenberg.ApiPaths.MergeOffice;

        public int Count => this.Assets.IfNullEmpty().Count;

        public bool PrintAsLandscape { get; set; }

        /// <summary>
        /// When used without setting <see cref="UseNativePdfFormat"/> to true
        /// Gotenberg has LibreOffice perform the conversion.
        /// When this and UseNativePdfFormat are set, gotenberg has unoconv do the work.
        /// </summary>
        public PdfFormats Format { get; set; }

        /// <summary>
        /// This tells gotenberg to use unoconv to perform the conversion.
        /// If you specify this with a <see cref="Format"/> it'll have unoconv convert it to that. 
        /// Note: the documentation says you can't use both together but I believe that regards the headers sent in.
        /// Using Format alone tells Gotenberg to have LibreOffice do the conversion.
        /// If you this prop to true and do not set <see cref="Format"/>, it'll default to PDF/A-1a
        /// </summary>
        public bool UseNativePdfFormat { get; set; }

        public override IEnumerable<HttpContent> ToHttpContent()
        {
            var lazyValidityCheck = new Lazy<List<ValidOfficeMergeItem>>(this.Assets.FindValidOfficeMergeItems(_resolver).ToList);

            foreach (var item in new[] { Count > 1, lazyValidityCheck.Value.Count() > 1 })
                if (!item) yield break;

            yield return CreateFormDataItem("true", Constants.Gotenberg.FormFieldNames.OfficeLibre.Merge);

            foreach (var item in lazyValidityCheck.Value.ToHttpContent())
                yield return item;

            foreach (var item in this.PropertiesToHttpContent()) 
                yield return item;
        }

    }
}