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

using Gotenberg.Sharp.API.Client.Domain.Webhooks;

namespace Gotenberg.Sharp.API.Client.Application.Webhooks;

/// <summary>
/// Configures webhook settings for asynchronous PDF generation. Gotenberg will POST the generated PDF
/// to the specified URL instead of returning it synchronously.
/// </summary>
/// <remarks>
/// When testing webhooks with Docker, use http://host.docker.internal:port to reach services on the host machine.
/// Reference: https://docs.docker.com/docker-for-windows/networking/#use-cases-and-workarounds
/// </remarks>
public sealed class WebhookBuilder
{
    private readonly Webhook _webhook;

    internal WebhookBuilder(Webhook webhook)
    {
        this._webhook = webhook;
    }

    /// <summary>
    /// Sets the URL where Gotenberg will POST the generated PDF.
    /// Must be an absolute URL accessible from the Gotenberg container.
    /// </summary>
    /// <param name="url">Absolute URL for webhook callback.</param>
    /// <param name="method">HTTP method for the webhook (defaults to POST if not specified).</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when URL is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when URL is not absolute.</exception>
    public WebhookBuilder SetUrl(string url, HttpMethod? method = null)
    {
        if (url.IsNotSet()) throw new ArgumentException("url is either null or empty");

        return this.SetUrl(new Uri(url), method);
    }

    /// <summary>
    /// Sets the URL where Gotenberg will POST the generated PDF.
    /// Must be an absolute URL accessible from the Gotenberg container.
    /// </summary>
    /// <param name="url">Absolute URI for webhook callback.</param>
    /// <param name="method">HTTP method for the webhook (defaults to POST if not specified).</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when URL is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when URL is not absolute.</exception>
    public WebhookBuilder SetUrl(Uri url, HttpMethod? method = null)
    {
        if (url == null) throw new ArgumentNullException(nameof(url));
        if (!url.IsAbsoluteUri)
            throw new InvalidOperationException("Url base href is not absolute");

        this._webhook.TargetUrl = url;
        this._webhook.HttpMethod = method?.ToString();

        return this;
    }

    /// <summary>
    /// Sets the URL where Gotenberg will POST error details if PDF generation fails.
    /// Optional - only set if you want to handle conversion errors separately. Gotenberg 9 has no error URL
    /// and never gets it: use <see cref="SetEventsUrl(string)" /> there.
    /// </summary>
    /// <param name="errorUrl">Absolute URL for error callback.</param>
    /// <param name="method">HTTP method for the error webhook (defaults to POST if not specified).</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when URL is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when URL is not absolute.</exception>
    public WebhookBuilder SetErrorUrl(string errorUrl, HttpMethod? method = null)
    {
        if (errorUrl.IsNotSet()) throw new ArgumentException("url is either null or empty");

        return this.SetErrorUrl(new Uri(errorUrl), method);
    }

    /// <summary>
    /// Sets the URL where Gotenberg will POST error details if PDF generation fails.
    /// Optional - only set if you want to handle conversion errors separately. Gotenberg 9 has no error URL
    /// and never gets it: use <see cref="SetEventsUrl(Uri)" /> there.
    /// </summary>
    /// <param name="url">Absolute URI for error callback.</param>
    /// <param name="method">HTTP method for the error webhook (defaults to POST if not specified).</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when URL is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when URL is not absolute.</exception>
    public WebhookBuilder SetErrorUrl( Uri url, HttpMethod? method = null)
    {
        if (url == null) throw new ArgumentNullException(nameof(url));
        if (!url.IsAbsoluteUri)
            throw new InvalidOperationException("Url base href is not absolute");

        this._webhook.ErrorUrl = url;
        this._webhook.ErrorHttpMethod = method?.ToString();

        return this;
    }

    /// <summary>
    /// Sets the URL where Gotenberg will POST a JSON event for each outcome of the webhook:
    /// <c>webhook.success</c> once the file is delivered, <c>webhook.error</c> when the conversion or the
    /// delivery fails. Required on Gotenberg 9, which reports errors only there; on Gotenberg 7 and 8 it is
    /// an alternative to <see cref="SetErrorUrl(string, HttpMethod?)" /> (Gotenberg 8.40.0 accepts either or both),
    /// read by Gotenberg 8.29.0 and newer. The client does not check the release of Gotenberg 7 or 8 for it.
    /// </summary>
    /// <param name="eventsUrl">Absolute URL for the events callback.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when URL is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when URL is not absolute.</exception>
    public WebhookBuilder SetEventsUrl(string eventsUrl)
    {
        if (eventsUrl.IsNotSet()) throw new ArgumentException("url is either null or empty");

        return this.SetEventsUrl(new Uri(eventsUrl));
    }

    /// <summary>
    /// Sets the URL where Gotenberg will POST a JSON event for each outcome of the webhook:
    /// <c>webhook.success</c> once the file is delivered, <c>webhook.error</c> when the conversion or the
    /// delivery fails. Required on Gotenberg 9, which reports errors only there; on Gotenberg 7 and 8 it is
    /// an alternative to <see cref="SetErrorUrl(Uri, HttpMethod?)" /> (Gotenberg 8.40.0 accepts either or both),
    /// read by Gotenberg 8.29.0 and newer. The client does not check the release of Gotenberg 7 or 8 for it.
    /// </summary>
    /// <param name="url">Absolute URI for the events callback.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when URL is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when URL is not absolute.</exception>
    public WebhookBuilder SetEventsUrl(Uri url)
    {
        if (url == null) throw new ArgumentNullException(nameof(url));
        if (!url.IsAbsoluteUri)
            throw new InvalidOperationException("Url base href is not absolute");

        this._webhook.EventsUrl = url;

        return this;
    }

    /// <summary>
    /// Adds a custom HTTP header that Gotenberg will include when POSTing to the webhook URL.
    /// Useful for authentication or request correlation.
    /// </summary>
    /// <param name="name">HTTP header name.</param>
    /// <param name="value">HTTP header value.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public WebhookBuilder AddExtraHeader(string name, string value)
    {
        return this.AddExtraHeader(name, [value]);
    }

    /// <summary>
    /// Adds a custom HTTP header from a key-value pair.
    /// </summary>
    /// <param name="header">Key-value pair representing header name and value.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public WebhookBuilder AddExtraHeader(KeyValuePair<string, string> header)
    {
        return this.AddExtraHeader(header.Key, [header.Value]);
    }

    /// <summary>
    /// Adds a custom HTTP header with multiple values.
    /// </summary>
    /// <param name="name">HTTP header name.</param>
    /// <param name="values">Collection of header values.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when header name is null or empty.</exception>
    public WebhookBuilder AddExtraHeader(string name, IEnumerable<string> values)
    {
        if (name.IsNotSet())
            throw new ArgumentException("extra header name is null || empty", nameof(name));

        this._webhook.ExtraHttpHeaders.Add(name, values);

        return this;
    }
}