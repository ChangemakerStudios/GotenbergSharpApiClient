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

        public static class Gotenberg
        {
            public static class ApiPaths
            {
                public const string MergePdf = "forms/pdfengines/merge";

                //Merge format type can be speicified by
                //--form 'pdfFormat="PDF/A-1a"'
                //

                public const string MergeOffice = "forms/libreoffice/convert";
                public const string ConvertHtml = "forms/chromium/convert/html";
                public const string UrlConvert = "forms/chromium/convert/url";
                public const string MarkdownConvert = "forms/chromium/convert/markdown";

                //new https://gotenberg.dev/docs/modules/pdf-engines#convert
                public const string ConvertPdf = "forms/pdfengines/convert";
                //needs form fields for each conversion type
                //Unoconv: --form 'pdfFormat="PDF/A-1a"' 
                //


            }

            //https://gotenberg.dev/docs/modules/pdf-engines#convert
            public static class PdfConversionFormats
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

            public static class FileNames
            {
                public const string Header = "header.html";
                public const string Index = "index.html";
                public const string Footer = "footer.html";
            }

            public static class FormFieldNames
            {
                public const string Files = "files";
                public const string WaitTimeout = "waitTimeout";
                public const string ResultFilename = "resultFilename";
                public const string RemoteUrl = "url";

                public const string ApiReadTimeout = "api-read-timeout";
                public const string PageRanges = "nativePageRanges";
               
                public static class Webhook
                {
                    public const string Url = "Gotenberg-Webhook-Url"; 
                    public const string ErrorUrl = "Gotenberg-Webhook-Error-Url";
                    public const string HttpMethod = "Gotenberg-Webhook-Method";
                    public const string ErrorHttpMethod = "Gotenberg-Webhook-Error-Method";
                    public const string ExtraHeaders = "Gotenberg-Webhook-Extra-Http-Headers";
                }

                //Works
                public static class Dims
                {
                    public const string Scale = "scale"; 
                    public const string PaperWidth = "paperWidth";
                    public const string PaperHeight = "paperHeight";
                    public const string MarginTop = "marginTop";
                    public const string MarginBottom = "marginBottom";
                    public const string MarginLeft = "marginLeft";
                    public const string MarginRight = "marginRight";
                    public const string Landscape = "landscape";

                    //new
                    public const string PreferCssPageSize = "preferCssPageSize";
                    public const string PrintBackground = "printBackground";
                }
            }

            public static class CustomRemoteHeaders
            { 
                public const string ExtraHttpHeaders = "extraHttpHeaders"; //all requests
            }
        }
    }
}