// ReSharper disable All CA1034
using Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;


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
                /// <summary>
                /// https://gotenberg.dev/docs/modules/chromium
                /// </summary>
                public const string Chromium = "forms/chromium";

                /// <summary>
                /// https://gotenberg.dev/docs/modules/libreoffice
                /// </summary>
                public const string LibreOffice = "forms/libreoffice";

                /// <summary>
                /// https://gotenberg.dev/docs/modules/pdf-engines
                /// </summary>
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

                public const string OutputFileName = "Gotenberg-Output-Filename";

                public const string RemoteUrl = "url";

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
                    public const string WaitDelay = "waitDelay";
                    public const string WaitForExpression = "waitForExpression";
                    public const string UserAgent = "userAgent";
                    public const string ExtraHttpHeaders = "extraHttpHeaders"; //all requests
                    public const string FailOnConsoleExceptions = "failOnConsoleExceptions";
                    public const string EmulatedMediaType = "emulatedMediaType";
                    public const string PdfFormat = "pdfFormat";
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
                    /// <summary>
                    /// Values reside in <see cref="PdfFormats"/>
                    /// </summary>
                    public const string PdfFormat = "pdfFormat";
                }
            }
        }
    }
}