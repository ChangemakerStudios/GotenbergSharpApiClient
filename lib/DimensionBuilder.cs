// Gotenberg.Sharp.Api.Client - Copyright (c) 2019 CaptiveAire

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client
{
    public class DimensionBuilder
    {
        public DimensionBuilder(HtmlConversionBuilder parent) 
            =>  this.Builder = parent;
        
        [UsedImplicitly]
        public HtmlConversionBuilder Builder { get; }
        
        [UsedImplicitly]
        public DimensionBuilder PaperWidth(double width)
        {
            this.Builder.DimensionInstance.PaperWidth = width;
            return this;
        }

        [UsedImplicitly]
        public DimensionBuilder PaperHeight(double height)
        {
            this.Builder.DimensionInstance.PaperHeight = height;
            return this;
        }

        [UsedImplicitly]
        public DimensionBuilder MarginTop(double marginTop)
        {
            this.Builder.DimensionInstance.MarginTop = marginTop;
            return this;
        }

        [UsedImplicitly]
        public DimensionBuilder MarginBottom(double marginBottom)
        {
            this.Builder.DimensionInstance.MarginBottom = marginBottom;
            return this;
        }
        
        [UsedImplicitly]
        public DimensionBuilder MarginLeft(double marginLeft)
        {
            this.Builder.DimensionInstance.MarginLeft = marginLeft;
            return this;
        }
        
        [UsedImplicitly]
        public DimensionBuilder MarginRight(double marginRight)
        {
            this.Builder.DimensionInstance.MarginRight = marginRight;
            return this;
        }

        [UsedImplicitly]
        public DimensionBuilder LandScape(bool landscape)
        {
            this.Builder.DimensionInstance.Landscape = landscape;
            return this;
        }
    }
}