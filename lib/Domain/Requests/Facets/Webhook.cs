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

namespace Gotenberg.Sharp.API.Client.Domain.Requests.Facets;

public sealed class Webhook
{
    private Uri? _errorUrl;

    private Uri? _targetUrl;

    /// <summary>
    ///     If set the Gotenberg API will send the resulting PDF file in a POST with
    ///     the application-pdf content type to the given url. Requests to the API
    ///     complete before the conversion is performed.
    /// </summary>
    /// <remarks>
    ///     When testing web hooks against a local container and a service
    ///     running on localhost to receive the posts, use http://host.docker.internal
    ///     Reference: https://docs.docker.com/desktop/windows/networking/#known-limitations-use-cases-and-workarounds
    /// </remarks>
    public Uri? TargetUrl
    {
        get => this._targetUrl;
        set
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (!value.IsAbsoluteUri)
                throw new InvalidOperationException("WebHook url must be absolute");

            this._targetUrl = value;
        }
    }

    /// <summary>
    ///     The HTTP method to use. Defaults to post if nothing is set.
    /// </summary>
    public string? HttpMethod { get; set; }

    /// <summary>
    ///     The callback url to use if an error occurs
    /// </summary>
    public Uri? ErrorUrl
    {
        get => this._errorUrl;
        set
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (!value.IsAbsoluteUri)
                throw new InvalidOperationException("WebHook url must be absolute");

            this._errorUrl = value;
        }
    }

    /// <summary>
    ///     The HTTP method to use when an error occurs. Defaults to post if nothing is set.
    /// </summary>
    public string? ErrorHttpMethod { get; set; }

    public ExtraHttpHeaders ExtraHttpHeaders { get; } = new();

    /*/// <summary>
    ///  By default, the API will wait 10 seconds before it considers the sending of the resulting PDF to be unsuccessful.
    ///  On a per request basis, this property can override the container environment variable, DEFAULT_WEBHOOK_URL_TIMEOUT
    /// </summary>
    public float? Timeout { get; set; }*/

    public void Validate()
    {
        if (this.TargetUrl != null && this.ErrorUrl == null)
            throw new ArgumentNullException(
                nameof(this.ErrorUrl),
                "An webhook error url is required");
    }

    public bool IsConfigured()
    {
        return this.TargetUrl != null && this.ErrorUrl != null;
    }

    public IEnumerable<(string, string?)> GetHeaders()
    {
        if (!this.IsConfigured()) return Enumerable.Empty<(string, string?)>();

        var webHookHeaders = new List<(string Name, string? Value)>
        {
            (Constants.Gotenberg.Webhook.Url, this.TargetUrl?.ToString()),
            (Constants.Gotenberg.Webhook.HttpMethod, this.HttpMethod),
            (Constants.Gotenberg.Webhook.ErrorUrl, this.ErrorUrl?.ToString()),
            (Constants.Gotenberg.Webhook.ErrorHttpMethod, this.ErrorHttpMethod)
        };

        return webHookHeaders.Concat(this.ExtraHttpHeaders.GetHeaders())
            .Where(entry => !string.IsNullOrWhiteSpace(entry.Value));
    }
}