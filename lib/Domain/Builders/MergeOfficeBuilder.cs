using System;
using System.Linq;
using System.Threading.Tasks;

using Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;
using Gotenberg.Sharp.API.Client.Domain.Requests;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Builders
{
    /// <summary>
    ///     Any non office files sent in are just ignored.
    ///     A nice surprise: Gotenberg/Chrome will merge in all sheets within a multi-sheet excel workbook.
    ///     If you send in a csv file but with an xlsx extension, it will merge it in as text.
    /// </summary>
    public sealed class MergeOfficeBuilder : BaseMergeBuilder<MergeOfficeRequest, MergeOfficeBuilder>
    {
        public MergeOfficeBuilder() => this.Request = new MergeOfficeRequest();

        protected override MergeOfficeRequest Request { get; set; }

        [PublicAPI]
        public MergeOfficeBuilder PrintAsLandscape()
        {
            this.Request.PrintAsLandscape = true;
            return this;
        }

        [PublicAPI]
        public MergeOfficeBuilder SetPageRanges(string pageRanges)
        {
            this.Request.PageRanges = pageRanges;
            return this;
        }

        /// <summary>
        /// This tells gotenberg to use unoconv to perform the conversion.
        /// When <see cref="MergeOfficeRequest.Format"/> is not set it defaults to using PDF/A-1a
        /// </summary>
        [PublicAPI]
        public MergeOfficeBuilder UseNativePdfFormat()
        {
            this.Request.UseNativePdfFormat = true;
            return this;
        }

        /// <summary>
        /// This tells gotenberg to use unoconv to perform the conversion in the specified format.
        /// </summary>
        [PublicAPI]
        public MergeOfficeBuilder UseNativePdfFormat(PdfFormats format)
        {
            this.Request.UseNativePdfFormat = true;
            this.Request.Format = format;

            return this;
        }

        [PublicAPI]
        public MergeOfficeRequest Build()
        {
            if (AsyncTasks.Any()) throw new InvalidOperationException(CallBuildAsyncErrorMessage);
            if (Request.Count == 0) throw new InvalidOperationException("There are no items to merge");
            return Request;
        }

        [PublicAPI]
        public async Task<MergeOfficeRequest> BuildAsync()
        {
            if (AsyncTasks.Any())
            {
                await Task.WhenAll(AsyncTasks).ConfigureAwait(false);
            }

            return Request;
        }
    }
}