using System;

using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;
using Gotenberg.Sharp.API.Client.Extensions;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Builders.Facets
{
    public sealed class DimensionBuilder : BaseBuilder<ChromeRequest>
    {
        public DimensionBuilder(ChromeRequest request)
        {
            this.Request = request ?? throw new ArgumentNullException(nameof(request));
            Request.Dimensions ??= new Dimensions();
        }


        [PublicAPI]
        public DimensionBuilder SetMargins(Margins margins)
        {
            var selected = margins.ToSelectedMargins();
            this.Request.Dimensions.MarginLeft = selected.Left;
            this.Request.Dimensions.MarginRight = selected.Right;
            this.Request.Dimensions.MarginTop = selected.Top;
            this.Request.Dimensions.MarginBottom = selected.Bottom;

            return this;
        }

        [PublicAPI]
        public DimensionBuilder SetPaperSize(PaperSizes sizes)
        {
            var selected = sizes.ToSelectedSize();
            this.Request.Dimensions.PaperWidth = selected.Width;
            this.Request.Dimensions.PaperHeight = selected.Height;

            return this;
        }

        /// <remarks>Gotenberg allows up to 200%, (2.0)</remarks>
        [PublicAPI]
        public DimensionBuilder SetScale(double scale)
        {
            this.Request.Dimensions.Scale = scale;

            return this;
        }

        [PublicAPI]
        public DimensionBuilder PaperWidth(double width)
        {
            this.Request.Dimensions.PaperWidth = width;
            return this;
        }

        [PublicAPI]
        public DimensionBuilder PaperHeight(double height)
        {
            this.Request.Dimensions.PaperHeight = height;
            return this;
        }

        [PublicAPI]
        public DimensionBuilder MarginTop(double marginTop)
        {
            this.Request.Dimensions.MarginTop = marginTop;
            return this;
        }

        [PublicAPI]
        public DimensionBuilder MarginBottom(double marginBottom)
        {
            this.Request.Dimensions.MarginBottom = marginBottom;
            return this;
        }

        [PublicAPI]
        public DimensionBuilder MarginLeft(double marginLeft)
        {
            this.Request.Dimensions.MarginLeft = marginLeft;
            return this;
        }

        [PublicAPI]
        public DimensionBuilder MarginRight(double marginRight)
        {
            this.Request.Dimensions.MarginRight = marginRight;
            return this;
        }

        [PublicAPI]
        public DimensionBuilder LandScape(bool landscape = true)
        {
            this.Request.Dimensions.Landscape = landscape;
            return this;
        }

        #region dimension instance

        [PublicAPI]
        public DimensionBuilder SetDimensions(Dimensions dims)
        {
            this.Request.Dimensions = dims ?? throw new ArgumentNullException(nameof(dims));
            return this;
        }

        [PublicAPI]
        public DimensionBuilder UseChromeDefaults() => SetDimensions(Dimensions.ToChromeDefaults());

        [PublicAPI]
        public DimensionBuilder UseDeliverableDefaults() => SetDimensions(Dimensions.ToDeliverableDefault());

        #endregion
    }
}