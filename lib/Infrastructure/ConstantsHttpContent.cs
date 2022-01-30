namespace Gotenberg.Sharp.API.Client.Infrastructure;

public static class ConstantsHttpContent
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