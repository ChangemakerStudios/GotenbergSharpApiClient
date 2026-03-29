// Copyright 2019-2025 Chris Mohan, Jaben Cargman
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

using Gotenberg.Sharp.API.Client.Domain.Overlays;
using Gotenberg.Sharp.API.Client.Domain.PdfOutput;
using Gotenberg.Sharp.API.Client.Domain.Requests.ApiRequests;
using Gotenberg.Sharp.API.Client.Domain.Rotation;
using Gotenberg.Sharp.API.Client.Domain.Split;

namespace Gotenberg.Sharp.API.Client.Domain.Requests;

public abstract class BuildRequestBase
{
    internal RequestConfig? Config { get; set; }

    internal AssetDictionary? Assets { get; set; }

    /// <summary>
    /// PDF output options shared across all request types (PDF/A, PDF/UA, flatten, tagged PDF, metadata).
    /// </summary>
    public PdfOutputOptions? PdfOutputOptions { get; set; }

    /// <summary>
    /// Cross-cutting rotation options (angle and page ranges).
    /// </summary>
    public RotationOptions? RotationOptions { get; set; }

    /// <summary>
    /// Cross-cutting split options (mode, span, and unify).
    /// </summary>
    public SplitOptions? SplitOptions { get; set; }

    /// <summary>
    /// Cross-cutting watermark options (background overlay).
    /// </summary>
    public WatermarkOptions? WatermarkOptions { get; set; }

    /// <summary>
    /// Cross-cutting stamp options (foreground overlay).
    /// </summary>
    public StampOptions? StampOptions { get; set; }

    protected abstract string ApiPath { get; }

    private const string _dispositionType = Constants.HttpContent.Disposition.Types.FormData;

    internal static StringContent CreateFormDataItem<T>(T value, string fieldName)
    {
        var item = new StringContent(value!.ToString()!);

        item.Headers.ContentDisposition = new ContentDispositionHeaderValue(_dispositionType) { Name = fieldName };

        return item;
    }

    protected virtual IEnumerable<HttpContent> ToHttpContent()
    {
        return this.PdfOutputOptions.IfNullEmptyContent()
            .Concat(this.RotationOptions.IfNullEmptyContent())
            .Concat(this.SplitOptions.IfNullEmptyContent())
            .Concat(this.WatermarkOptions.IfNullEmptyContent())
            .Concat(this.StampOptions.IfNullEmptyContent());
    }

    protected virtual void Validate()
    {
        this.Config?.Validate();
        this.Assets?.Validate();
    }

    public virtual IApiRequest CreateApiRequest()
    {
        this.Validate();

        var isWebHook = this.Config?.Webhook?.IsConfigured() ?? false;

        var headers = (this.Config?.GetHeaders()).IfNullEmpty().ToLookup(s => s.Name, s => s.Value);

        return new PostApiRequestImpl(this.ToHttpContent, this.ApiPath, headers, isWebHook);
    }
}