//  Copyright 2019-2025 Chris Mohan, Jaben Cargman
//  and GotenbergSharpApiClient Contributors
// 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.

namespace Gotenberg.Sharp.API.Client.Infrastructure;

public static class Constants
{
    //Collapse all: ctrl M, A
    //Toggle with: ctrl M, L

    public static class HttpContent
    {
        public static class Headers
        {
            public const string UserAgent = "User-Agent";
        }

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
        public static class All
        {
            public const string Trace = "Gotenberg-Trace";

            public static class ApiPaths
            {
                public const string Version = "version";

                public const string Debug = "debug";

                public const string Health = "health";
            }
        }

        public static class SharedFormFieldNames
        {
            public const string Files = "files";

            public const string OutputFileName = "Gotenberg-Output-Filename";
            //above currently only used by web-hooks. The docs put it in the API section however.
        }

        private static class CrossCutting
        {
            internal const string Landscape = "landscape";

            internal const string PageRanges = "nativePageRanges";

            internal const string PdfFormat = "pdfa";

            internal const string PdfUa = "pdfua";

            internal static class FileNames
            {
                internal const string Index = "index.html";
            }
        }

        /// <summary>
        ///     Performs office doc conversions AND merges. It has one route however: convert
        ///     Merges happen when the header --form merge="true" is passed
        ///     Sending more than one document for conversion results in the api returning a zip file with results
        ///     https://gotenberg.dev/docs/modules/libreoffice
        /// </summary>
        public static class LibreOffice
        {
            public static class ApiPaths
            {
                private const string Root = "forms/libreoffice";

                public const string MergeOffice = $"{Root}/convert";
            }

            public static class Routes
            {
                public static class Convert
                {
                    public const string Landscape = CrossCutting.Landscape;

                    public const string PageRanges = CrossCutting.PageRanges;

                    public const string PdfFormat = CrossCutting.PdfFormat;

                    public const string PdfUa = CrossCutting.PdfUa;

                    public const string Merge = "merge";

                    public const string Flatten = "flatten";
                }
            }
        }

        /// <summary>
        ///     https://gotenberg.dev/docs/modules/pdf-engines#convert
        /// </summary>
        public static class PdfEngines
        {
            public static class ApiPaths
            {
                private const string Root = "forms/pdfengines";

                public const string MergePdf = $"{Root}/merge";

                public const string ConvertPdf = $"{Root}/convert";
            }

            public static class Routes
            {
                public static class Merge
                {
                    public const string PdfFormat = CrossCutting.PdfFormat;
                }

                public static class Convert
                {
                    public const string PdfFormat = CrossCutting.PdfFormat;
                }
            }
        }

        /// <summary>
        ///     Source: https://gotenberg.dev/docs/modules/webhook#middleware
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

        /// <summary>
        ///     https://gotenberg.dev/docs/modules/chromium
        /// </summary>
        public static class Chromium
        {
            public static class ApiPaths
            {
                private const string Root = "forms/chromium";

                public const string ConvertUrl = $"{Root}/convert/url";

                public const string ConvertHtml = $"{Root}/convert/html";

                public const string ConvertMarkdown = $"{Root}/convert/markdown";
            }

            public static class Routes
            {
                public static class Url
                {
                    public const string RemoteUrl = "url";

                    public const string ExtraLinkTags = "extraLinkTags";

                    public const string ExtraScriptTags = "extraScriptTags";
                }

                public static class Html
                {
                    public const string IndexFile = CrossCutting.FileNames.Index;
                }

                /*public static class Markdown
                {
                    //Unused because MD requests are done with through HTML request
                    //public const string IndexFile = CrossCutting.FileNames.Index;
                }*/
            }

            public static class Shared
            {
                /// <summary>
                ///     From the header & footer tabs
                /// </summary>
                public static class FileNames
                {
                    public const string Header = "header.html";

                    public const string Footer = "footer.html";
                }

                /// <summary>
                ///     Page properties
                /// </summary>
                public static class PageProperties
                {
                    public const string PaperWidth = "paperWidth";

                    public const string PaperHeight = "paperHeight";

                    public const string MarginTop = "marginTop";

                    public const string MarginBottom = "marginBottom";

                    public const string MarginLeft = "marginLeft";

                    public const string MarginRight = "marginRight";

                    public const string PreferCssPageSize = "preferCssPageSize";

                    public const string PrintBackground = "printBackground";

                    public const string OmitBackground = "omitBackground";

                    public const string GenerateDocumentOutline = "generateDocumentOutline";

                    public const string Landscape = CrossCutting.Landscape;

                    public const string Scale = "scale";

                    public const string PageRanges = CrossCutting.PageRanges;
                    
                    public const string SinglePage = "singlePage";
                }

                public static class HtmlConvert
                {
                    //wait
                    public const string WaitDelay = "waitDelay";

                    public const string WaitForExpression = "waitForExpression";

                    //http headers
                    public const string UserAgent = "userAgent";

                    public const string ExtraHttpHeaders = "extraHttpHeaders";

                    public const string MetaData = "metadata";

                    //javascript
                    public const string FailOnConsoleExceptions = "failOnConsoleExceptions";

                    //css
                    public const string EmulatedMediaType = "emulatedMediaType";

                    public const string SkipNetworkIdleEvent = "skipNetworkIdleEvent";

                    //pdf format
                    public const string PdfFormat = CrossCutting.PdfFormat;

                    public const string PdfUa = CrossCutting.PdfUa;
                }

                public static class UrlConvert
                {
                    public const string PdfFormat = CrossCutting.PdfFormat;

                    public const string PdfUa = CrossCutting.PdfUa;
                }
            }
        }
    }
}