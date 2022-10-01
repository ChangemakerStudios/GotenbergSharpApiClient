//  Copyright 2019-2022 Chris Mohan, Jaben Cargman
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

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

using Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;
using Gotenberg.Sharp.API.Client.Domain.Requests.Facets;
using Gotenberg.Sharp.API.Client.Extensions;
using Gotenberg.Sharp.API.Client.Infrastructure;

using JetBrains.Annotations;

namespace Gotenberg.Sharp.API.Client.Domain.Requests;

public abstract class BuildRequestBase
{
    private const string DispositionType = Constants.HttpContent.Disposition.Types.FormData;

    internal RequestConfig? Config { get; set; }

    internal AssetDictionary? Assets { get; set; }

    internal PdfFormats Format { [UsedImplicitly] get; set; }

    protected abstract string ApiPath { get; }

    protected abstract IEnumerable<HttpContent> ToHttpContent();

    internal static StringContent CreateFormDataItem<T>(T value, string fieldName)
    {
        var item = new StringContent(value!.ToString()!);

        item.Headers.ContentDisposition = new ContentDispositionHeaderValue(DispositionType)
            { Name = fieldName };

        return item;
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

        var headers = (this.Config?.GetHeaders()).IfNullEmpty()
            .ToLookup(s => s.Name, s => s.Value);

        return new ApiRequestImplementation(this.ToHttpContent, this.ApiPath, headers, isWebHook);
    }
}