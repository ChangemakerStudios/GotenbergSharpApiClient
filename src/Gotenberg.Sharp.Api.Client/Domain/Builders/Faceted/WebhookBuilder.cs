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



namespace Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;

public sealed class WebhookBuilder
{
    private readonly Webhook _webhook;

    internal WebhookBuilder(Webhook webhook)
    {
        this._webhook = webhook;
    }

    /// <summary>
    ///     When testing web hooks against a local container and a service
    ///     running on localhost, to receive the posts, use http://host.docker.internal:your-port
    ///     Reference: https://docs.docker.com/docker-for-windows/networking/#use-cases-and-workarounds
    /// </summary>
    /// <param name="url"></param>
    /// <param name="method"></param>
    /// <returns></returns>
    
    public WebhookBuilder SetUrl(string url, HttpMethod? method = null)
    {
        if (url.IsNotSet()) throw new ArgumentException("url is either null or empty");

        return this.SetUrl(new Uri(url), method);
    }

    
    public WebhookBuilder SetUrl(Uri url, HttpMethod? method = null)
    {
        if (url == null) throw new ArgumentNullException(nameof(url));
        if (!url.IsAbsoluteUri)
            throw new InvalidOperationException("Url base href is not absolute");

        this._webhook.TargetUrl = url;
        this._webhook.HttpMethod = method?.ToString();

        return this;
    }

    
    public WebhookBuilder SetErrorUrl(string errorUrl, HttpMethod? method = null)
    {
        if (errorUrl.IsNotSet()) throw new ArgumentException("url is either null or empty");

        return this.SetErrorUrl(new Uri(errorUrl), method);
    }

    
    public WebhookBuilder SetErrorUrl( Uri url, HttpMethod? method = null)
    {
        if (url == null) throw new ArgumentNullException(nameof(url));
        if (!url.IsAbsoluteUri)
            throw new InvalidOperationException("Url base href is not absolute");

        this._webhook.ErrorUrl = url;
        this._webhook.ErrorHttpMethod = method?.ToString();

        return this;
    }

    
    public WebhookBuilder AddExtraHeader(string name, string value)
    {
        return this.AddExtraHeader(name, [value]);
    }

    
    public WebhookBuilder AddExtraHeader(KeyValuePair<string, string> header)
    {
        return this.AddExtraHeader(header.Key, [header.Value]);
    }

    
    public WebhookBuilder AddExtraHeader(string name, IEnumerable<string> values)
    {
        if (name.IsNotSet())
            throw new ArgumentException("extra header name is null || empty", nameof(name));

        this._webhook.ExtraHttpHeaders.Add(name, values);

        return this;
    }
}