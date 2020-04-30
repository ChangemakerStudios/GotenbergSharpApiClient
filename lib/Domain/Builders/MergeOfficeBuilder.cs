using Gotenberg.Sharp.API.Client.Domain.Builders.FacetedBuilders;
using Gotenberg.Sharp.API.Client.Domain.Requests;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Builders
{
    /// <remarks>
    ///     Any non office files sent in are just ignored.
    ///     A nice surprise: Gotenberg/Chrome will merge in all sheets within a multi-sheet excel workbook.
    ///     If you send in a csv file but with an xlsx extension, it will merge it in as text.
    /// </remarks>
    public sealed class MergeOfficeBuilder: BaseBuilder<MergeOfficeRequest>
    {
        public MergeOfficeBuilder() => Init();

        protected override MergeOfficeRequest Request { get; set; }

        [PublicAPI]
        public AssetBuilder<MergeOfficeBuilder> Assets => new AssetBuilder<MergeOfficeBuilder>(this.Request, this);

        [PublicAPI]
        public ConfigBuilder<MergeOfficeBuilder> ConfigureRequest => new ConfigBuilder<MergeOfficeBuilder>(this.Request, this);

        [PublicAPI]
        public MergeOfficeRequest Build() => this.Request;

        void Init() => this.Request = new MergeOfficeRequest();
    }
}