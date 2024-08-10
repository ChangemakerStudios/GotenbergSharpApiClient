//  Copyright 2019-2024 Chris Mohan, Jaben Cargman
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

namespace Gotenberg.Sharp.API.Client.Extensions;

internal static class MergeOfficeRequestExtensions
{
    internal static IEnumerable<HttpContent> PropertiesToHttpContent(this MergeOfficeRequest request)
    {
        if (request.PrintAsLandscape)
            yield return BuildRequestBase.CreateFormDataItem(
                "true",
                Constants.Gotenberg.LibreOffice.Routes.Convert.Landscape);

        if (request.PageRanges.IsSet())
            yield return BuildRequestBase.CreateFormDataItem(
                request.PageRanges,
                Constants.Gotenberg.LibreOffice.Routes.Convert.PageRanges);
        
        if (request.ExportFormFields.HasValue)
            yield return BuildRequestBase.CreateFormDataItem(
                request.ExportFormFields.Value,
                Constants.Gotenberg.LibreOffice.Routes.Convert.ExportFormFields);

        if (request.EnablePdfUa)
            yield return BuildRequestBase.CreateFormDataItem(
                "true",
                Constants.Gotenberg.LibreOffice.Routes.Convert.PdfUa);

        if (!request.UseNativePdfFormat && request.Format == default) yield break;

        if (!request.UseNativePdfFormat && request.Format != default)
            yield return BuildRequestBase.CreateFormDataItem(
                request.Format.ToFormDataValue(),
                Constants.Gotenberg.LibreOffice.Routes.Convert.PdfFormat);
        else
            yield return BuildRequestBase.CreateFormDataItem(
                request.Format.ToFormDataValue(),
                Constants.Gotenberg.LibreOffice.Routes.Convert.NativePdfFormat);
    }
}