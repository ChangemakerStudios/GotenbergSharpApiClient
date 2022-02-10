using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;
using Gotenberg.Sharp.API.Client.Domain.Requests;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Builders
{
    /// <remarks>
    ///     Any non office files sent in are just ignored.
    ///     A nice surprise: Gotenberg/Chrome will merge in all sheets within a multi-sheet excel workbook.
    ///     If you send in a csv file but with an xlsx extension, it will merge it in as text.
    /// </remarks>
    public sealed class MergeOfficeBuilder : BaseBuilder<MergeOfficeRequest>
    {
        readonly List<Task> _asyncTasks = new List<Task>();

        public MergeOfficeBuilder() => this.Request = new MergeOfficeRequest();

        protected override MergeOfficeRequest Request { get; set; }

        [PublicAPI]
        public MergeOfficeBuilder WithAssets(Action<AssetBuilder> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            action(new AssetBuilder(this.Request));
            return this;
        }

        [PublicAPI]
        public MergeOfficeBuilder WithAsyncAssets(Func<AssetBuilder, Task> asyncAction)
        {
            if (asyncAction == null) throw new ArgumentNullException(nameof(asyncAction));
            this._asyncTasks.Add(asyncAction(new AssetBuilder(this.Request)));
            return this;
        }

        [PublicAPI]
        public MergeOfficeBuilder ConfigureRequest(Action<ConfigBuilder> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            action(new ConfigBuilder(this.Request));
            return this;
        }

        [PublicAPI]
        public MergeOfficeBuilder PrintAsLandscape()
        {
            this.Request.PrintAsLandscape = true;
            return this;
        }

        /// <summary>
        /// This tells gotenberg to have OfficeLibre perform the conversion.
        /// If you set <see cref="MergeOfficeRequest.UseNativePdfFormat"/> to true
        /// then gotenberg will hand the work off to unoconv to do the work
        /// </summary>
        [PublicAPI]
        public MergeOfficeBuilder SetPdfFormat(PdfFormats format)
        {
            this.Request.Format = format;
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
            if (_asyncTasks.Any()) throw new InvalidOperationException(CallBuildAsyncErrorMessage);
            if (Request.Count == 0) throw new InvalidOperationException("There are no items to merge");
            return Request;
        }


        [PublicAPI]
        public async Task<MergeOfficeRequest> BuildAsync()
        {
            if (_asyncTasks.Any())
            {
                await Task.WhenAll(_asyncTasks).ConfigureAwait(false);
            }

            return Request;
        }
    }
}