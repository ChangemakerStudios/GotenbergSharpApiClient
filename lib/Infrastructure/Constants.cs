namespace Gotenberg.Sharp.API.Client.Infrastructure
{
    public static class Constants
    {
        public static class Http
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
                public const string RemoteURL      =   "remoteURL";
                
                public const string WebhookURL     =   "webhookURL";
                public const string WebhookTimeout =   "webhookURLTimeout";
                public const string ChromeRpccBufferSize = "googleChromeRpccBufferSize";
                
                public static class Dims
                {
                    public const string PaperWidth   =  "paperWidth";
                    public const string PaperHeight  =  "paperHeight";
                    public const string MarginTop    =  "marginTop";
                    public const string MarginBottom =  "marginBottom";
                    public const string MarginLeft   =  "marginLeft";
                    public const string MarginRight  =  "marginRight";
                    public const string Landscape    =  "landscape";
                }
                
            }
        }

    }
}