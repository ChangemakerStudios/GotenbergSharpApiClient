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
                public const string MergePdf = "merge";
                public const string MergeOffice = "convert/office";
                public const string ConvertHtml = "convert/html";
                public const string UrlConvert = "convert/url";
                public const string MarkdownConvert = "convert/markdown";
            }
            
            public static class FileNames
            {
                public const string Header = "header.html";
                public const string Index  = "index.html";
                public const string Footer = "footer.html";
            }

            public static class FormFieldNames
            {
                public const string Files          =   "files";
                public const string WaitTimeout    =   "waitTimeout";
                public const string ResultFilename =   "resultFilename";
                public const string RemoteUrl      =   "remoteURL";
                
                public const string PageRanges     =   "pageRanges";   
                public const string WebhookUrl     =   "webhookURL";
                public const string WebhookTimeout =   "webhookURLTimeout";
                public const string ChromeRpccBufferSize = "googleChromeRpccBufferSize";
                
                public static class Dims
                {
                    public const string Scale        =  "scale";
                    public const string PaperWidth   =  "paperWidth";
                    public const string PaperHeight  =  "paperHeight";
                    public const string MarginTop    =  "marginTop";
                    public const string MarginBottom =  "marginBottom";
                    public const string MarginLeft   =  "marginLeft";
                    public const string MarginRight  =  "marginRight";
                    public const string Landscape    =  "landscape";
                }
            }

            //See https://github.com/thecodingmachine/gotenberg-go-client/releases/tag/v7.0.0
            public static class CustomRemoteHeaders
            {
                public const string RemoteUrlKeyPrefix = "Gotenberg-Remoteurl-";
                public const string WebhookHeaderKeyPrefix = "Gotenberg-Webhookurl-";
            }
        }

    }
}