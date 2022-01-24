// ReSharper disable All CA1034

namespace Gotenberg.Sharp.API.Client.Infrastructure
{
    public static class Constants
    {
        public static class HttpContent
        {
            public static class MediaTypes
            {
                public const string TextHtml = "text/html";
                public const string ApplicationPdf = "application/pdf";
            }

            public static class MultipartData
            {
                public const string BoundaryPrefix = "--------------------------";
            }

            public static class Disposition
            {
                public static class Types
                {
                    public const string FormData = "form-data";
                }
            }
        }

        /// <summary>
        /// https://gotenberg.dev/docs/about
        /// </summary>
        public static class Gotenberg
        {
            static class Modules
            {
                public const string Chromium = "forms/chromium";
                public const string LibreOffice = "forms/libreoffice";
                public const string PdfEngines = "forms/pdfengines";
            }

            public static class ApiPaths
            {
                public const string ConvertHtml = $"{Modules.Chromium}/convert/html";
                public const string ConvertUrl = $"{Modules.Chromium}/convert/url";
                public const string ConvertMarkdown = $"{Modules.Chromium}/convert/markdown";

                //TODO: currently unused
                public const string ConvertPdf = $"{Modules.PdfEngines}/convert";
                public const string MergePdf = $"{Modules.PdfEngines}/merge"; 
                public const string MergeOffice = $"{Modules.LibreOffice}/convert";
            }

            public static class FileNames
            {
                public const string Header = "header.html";
                public const string Index = "index.html";
                public const string Footer = "footer.html";
            }

            public static class FormFieldNames
            {
                public const string Files = "files";
                public const string ResultFilename = "resultFilename";

                public const string RemoteUrl = "url";
                public const string WaitTimeout = "waitTimeout";
                public const string ApiReadTimeout = "api-read-timeout";

                public static class Dims
                {
                    public const string PaperWidth = "paperWidth";
                    public const string PaperHeight = "paperHeight";
                    public const string MarginTop = "marginTop";
                    public const string MarginBottom = "marginBottom";
                    public const string MarginLeft = "marginLeft";
                    public const string MarginRight = "marginRight";
                    public const string PreferCssPageSize = "preferCssPageSize";
                    public const string PrintBackground = "printBackground";
                    public const string Landscape = "landscape";
                    public const string Scale = "scale";
                    public const string PageRanges = "nativePageRanges";
                }

                /// <summary>
                /// https://gotenberg.dev/docs/modules/chromium
                /// </summary>
                public static class HtmlConvertBehaviors
                {                    
                    public const string ExtraHttpHeaders = "extraHttpHeaders"; //all requests
                    
                    //Duration to wait when loading an HTML document before converting it to PDF
                    public const string WaitDelay = "waitDelay";
                    
                    //The JavaScript expression to wait before converting an HTML document to PDF until it returns true
                    public const string WaitForExpression = "waitForExpression";

                    public const string FailOnConsoleExceptions = "failOnConsoleExceptions";

                    /// <summary>
                    ///   The media type to emulate, either "screen" or "print" - empty means "print"
                    /// </summary>
                    public const string EmulatedMediaType = "emulatedMediaType";
                }

                /// <summary>
                /// https://gotenberg.dev/docs/modules/chromium#url
                /// </summary>
                public static class UrlConvertBehaviors
                {
                    public const string ExtraLinkTags = "extraLinkTags";
                    public const string ExtraScriptTags = "extraScriptTags";
                }

                /// <summary>
                /// Source: https://gotenberg.dev/docs/modules/webhook#middleware
                /// </summary>
                /// <remarks>
                ///     PipeDream.com can be used to test the webhook feature.
                /// </remarks>
                public static class Webhook
                {
                    public const string Url = "Gotenberg-Webhook-Url";
                    public const string ErrorUrl = "Gotenberg-Webhook-Error-Url";
                    public const string HttpMethod = "Gotenberg-Webhook-Method";
                    public const string ErrorHttpMethod = "Gotenberg-Webhook-Error-Method";
                    public const string ExtraHeaders = "Gotenberg-Webhook-Extra-Http-Headers";
                }

                public static class OfficeLibre
                {
                    public const string Merge = "merge";
                    //TODO currently unused
                    public const string PdfA1aFormat = "nativePdfA1aFormat";
                    //just passes true as the value
                }

                /// <summary>
                /// https://gotenberg.dev/docs/modules/pdf-engines#convert
                /// </summary>
                public static class PdfEngines
                {
                    public const string PdfFormat = "pdfFormat";

                    /// <summary>
                    ///     Used in merges and conversions
                    ///     Source: https://github.com/gotenberg/gotenberg/blob/872263d25335445423f22f6043922964ec3d5eff/pkg/gotenberg/pdfengine.go#L19
                    /// </summary>
                    public static class PdfFormats
                    {
                        public const string A1a = "PDF/A-1a";
                        public const string A1b = "PDF/A-1b";
                        public const string A2a = "PDF/A-2a";
                        public const string A2b = "PDF/A-2b";
                        public const string A2u = "PDF/A-2u";
                        public const string A3a = "PDF/A-3a";
                        public const string A3b = "PDF/A-3b";
                        public const string A3u = "PDF/A-3u";
                    }                    
                }
            }
        }
    }
}