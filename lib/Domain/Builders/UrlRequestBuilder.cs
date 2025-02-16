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

using Gotenberg.Sharp.API.Client.Domain.Requests.Facets.UrlExtras;



namespace Gotenberg.Sharp.API.Client.Domain.Builders;

public sealed class UrlRequestBuilder()
    : BaseChromiumBuilder<UrlRequest, UrlRequestBuilder>(new UrlRequest())
{
    public UrlRequestBuilder SetUrl(string url)
    {
        if (url.IsNotSet()) throw new ArgumentException("url is either null or empty");
        return this.SetUrl(new Uri(url));
    }

    
    public UrlRequestBuilder SetUrl(Uri url)
    {
        this.Request.Url = url ?? throw new ArgumentNullException(nameof(url));
        if (!url.IsAbsoluteUri) throw new InvalidOperationException("url is not absolute");
        return this;
    }

    
    public UrlRequestBuilder AddHeaderFooter(Action<UrlHeaderFooterBuilder> action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));

        action(new UrlHeaderFooterBuilder(this.Request.Content ??= new HeaderFooterDocument()));
        return this;
    }

    
    public UrlRequestBuilder AddAsyncHeaderFooter(Func<UrlHeaderFooterBuilder, Task> asyncAction)
    {
        if (asyncAction == null) throw new ArgumentNullException(nameof(asyncAction));

        this.BuildTasks.Add(asyncAction(new UrlHeaderFooterBuilder(this.Request.Content ??= new HeaderFooterDocument())));
        return this;
    }

    
    public UrlRequestBuilder AddExtraResources(Action<UrlExtraResourcesBuilder> action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        action(new UrlExtraResourcesBuilder(this.Request.ExtraResources ??= new ExtraUrlResources()));
        return this;
    }
}