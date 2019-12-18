// Gotenberg.Sharp.Api.Client - Copyright (c) 2019 CaptiveAire

using Gotenberg.Sharp.API.Client.Domain.Requests;
using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client
{
    public class DimensionBuilder : ConversionBuilderFacade
    {
        public DimensionBuilder(PdfRequest request)
        {
            this.Request = request;
            this.Request.Dimensions ??= new DocumentDimensions();
        }

        [UsedImplicitly]
        public DimensionBuilder PaperWidth(double width)
        {
            this.Request.Dimensions.PaperWidth = width;
            return this;
        }

        [UsedImplicitly]
        public DimensionBuilder PaperHeight(double height)
        {
            this.Request.Dimensions.PaperHeight = height;
            return this;
        }

        [UsedImplicitly]
        public DimensionBuilder MarginTop(double marginTop)
        {
            this.Request.Dimensions.MarginTop = marginTop;
            return this;
        }

        [UsedImplicitly]
        public DimensionBuilder MarginBottom(double marginBottom)
        {
            this.Request.Dimensions.MarginBottom = marginBottom;
            return this;
        }
        
        [UsedImplicitly]
        public DimensionBuilder MarginLeft(double marginLeft)
        {
            this.Request.Dimensions.MarginLeft = marginLeft;
            return this;
        }
        
        [UsedImplicitly]
        public DimensionBuilder MarginRight(double marginRight)
        {
            this.Request.Dimensions.MarginRight = marginRight;
            return this;
        }

        [UsedImplicitly]
        public DimensionBuilder LandScape(bool landscape)
        {
            this.Request.Dimensions.Landscape = landscape;
            return this;
        }
    }
}