﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;
using Gotenberg.Sharp.API.Client.Domain.Requests;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Builders
{
    public sealed class MergeBuilder : BaseBuilder<MergeRequest>
    {
        readonly List<Task> _asyncTasks = new List<Task>();

        public MergeBuilder() => this.Request = new MergeRequest();

        protected override MergeRequest Request { get; set; }

        [PublicAPI]
        public MergeBuilder WithAssets(Action<AssetBuilder> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            action(new AssetBuilder(this.Request));
            return this;
        }

        [PublicAPI]
        public MergeBuilder WithAsyncAssets(Func<AssetBuilder, Task> asyncAction)
        {
            if (asyncAction == null) throw new ArgumentNullException(nameof(asyncAction));
            this._asyncTasks.Add(asyncAction(new AssetBuilder(this.Request)));
            return this;
        }

        /// <summary>
        /// This tells gotenberg to have OfficeLibre perform the conversion.
        /// If you set <see cref="MergeOfficeRequest.UseNativePdfFormat"/> to true
        /// then gotenberg will hand the work off to unoconv to do the work
        /// </summary>
        [PublicAPI]
        public MergeBuilder SetPdfFormat(PdfFormats format)
        {
            this.Request.Format = format;
            return this;
        }

        [PublicAPI]
        public MergeBuilder ConfigureRequest(Action<ConfigBuilder> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            action(new ConfigBuilder(this.Request));
            return this;
        }


        [PublicAPI]
        public MergeRequest Build()
        {
            if (_asyncTasks.Any()) throw new InvalidOperationException(CallBuildAsyncErrorMessage);
            if (Request.Count == 0) throw new InvalidOperationException("There are no items to merge");
            return Request;
        }


        [PublicAPI]
        public async Task<MergeRequest> BuildAsync()
        {
            if (_asyncTasks.Any())
            {
                await Task.WhenAll(_asyncTasks).ConfigureAwait(false);
            }

            return Request;
        }
    }
}