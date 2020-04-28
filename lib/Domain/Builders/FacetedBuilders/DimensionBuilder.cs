using System;
using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Requests.Content;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Builders.FacetedBuilders
{
    public sealed class DimensionBuilder<TParent>: BaseBuilder<ChromeRequest>
    {
        public DimensionBuilder(ChromeRequest request, TParent parent)
        {
            this.Parent = parent;
            this.Request = request ?? throw new ArgumentNullException(nameof(request));
            Request.Dimensions ??= new Dimensions();
        }

        [PublicAPI]
        public TParent Parent { get; }

        [PublicAPI]
        public DimensionBuilder<TParent> SetScale(double scale)
        {
            this.Request.Dimensions.Scale = scale;
            return this;
        }
        
        [PublicAPI]
        public DimensionBuilder<TParent> PaperWidth(double width)
        {
            this.Request.Dimensions.PaperWidth = width;
            return this;
        }

        [PublicAPI]
        public DimensionBuilder<TParent> PaperHeight(double height)
        {
            this.Request.Dimensions.PaperHeight = height;
            return this;
        }

        [PublicAPI]
        public DimensionBuilder<TParent> MarginTop(double marginTop)
        {
            this.Request.Dimensions.MarginTop = marginTop;
            return this;
        }

        [PublicAPI]
        public DimensionBuilder<TParent> MarginBottom(double marginBottom)
        {
            this.Request.Dimensions.MarginBottom = marginBottom;
            return this;
        }
        
        [PublicAPI]
        public DimensionBuilder<TParent> MarginLeft(double marginLeft)
        {
            this.Request.Dimensions.MarginLeft = marginLeft;
            return this;
        }
        
        [PublicAPI]
        public DimensionBuilder<TParent> MarginRight(double marginRight)
        {
            this.Request.Dimensions.MarginRight = marginRight;
            return this;
        }

        [PublicAPI]
        public DimensionBuilder<TParent> LandScape(bool landscape = true)
        {
            this.Request.Dimensions.Landscape = landscape;
            return this;
        }

        #region dimension instance
        
        [PublicAPI]
        public DimensionBuilder<TParent> SetDimensions(Dimensions dims)
        {
            this.Request.Dimensions = dims;
            return this;
        }

        [PublicAPI]
        public DimensionBuilder<TParent> UseChromeDefaults() => SetDimensions(Dimensions.ToChromeDefaults());

        [PublicAPI]
        public DimensionBuilder<TParent> UseDeliverableDefaults() => SetDimensions(Dimensions.ToDeliverableDefault());

        #endregion
    }
}