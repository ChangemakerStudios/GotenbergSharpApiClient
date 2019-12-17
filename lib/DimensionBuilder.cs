// Gotenberg.Sharp.Api.Client - Copyright (c) 2019 CaptiveAire

using Gotenberg.Sharp.API.Client.Domain.Requests;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client
{
    public class DimensionBuilder : ConversionBuilderFacade
    {
        public DimensionBuilder(DocumentDimensions dimensions) => this.Dims = dimensions;
        
        [UsedImplicitly]
        public DimensionBuilder PaperWidth(double width)
        {
           this.Dims.PaperWidth = width;
            return this;
        }

        [UsedImplicitly]
        public DimensionBuilder PaperHeight(double height)
        {
            this.Dims.PaperHeight = height;
            return this;
        }

        [UsedImplicitly]
        public DimensionBuilder MarginTop(double marginTop)
        {
            this.Dims.MarginTop = marginTop;
            return this;
        }

        [UsedImplicitly]
        public DimensionBuilder MarginBottom(double marginBottom)
        {
            this.Dims.MarginBottom = marginBottom;
            return this;
        }
        
        [UsedImplicitly]
        public DimensionBuilder MarginLeft(double marginLeft)
        {
            this.Dims.MarginLeft = marginLeft;
            return this;
        }
        
        [UsedImplicitly]
        public DimensionBuilder MarginRight(double marginRight)
        {
            this.Dims.MarginRight = marginRight;
            return this;
        }

        [UsedImplicitly]
        public DimensionBuilder LandScape(bool landscape)
        {
            this.Dims.Landscape = landscape;
            return this;
        }
    }
}