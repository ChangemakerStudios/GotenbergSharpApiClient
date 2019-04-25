// CaptiveAire.Gotenberg.App.API.Sharp.Client - Copyright (c) 2019 CaptiveAire

// ReSharper disable UnusedMember.Global


namespace CaptiveAire.Gotenberg.App.API.Sharp.Client
{
    /// <summary>
    ///  Represents the dimensions of the pdf document
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class DocumentDimensions
    {
        public double PaperWidth { get; set; }
        public double PaperHeight { get; set; }
        public double MarginTop { get; set; }
        public double MarginBottom { get; set; }
        public double MarginLeft { get; set; }
        public double MarginRight { get; set; }
        public bool Landscape { get;set; }


    }
}