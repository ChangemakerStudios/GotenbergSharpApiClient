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

using Gotenberg.Sharp.API.Client.Domain.ExtraHeaders;
using Gotenberg.Sharp.API.Client.Domain.Shared;
using Gotenberg.Sharp.API.Client.Domain.Stages;

namespace Gotenberg.Sharp.API.Client.Domain.Webhooks;

public sealed class Webhook
{
    private Uri? _errorUrl;

    private Uri? _eventsUrl;

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
    ///     The callback url to use if an error occurs. Gotenberg 7 and 8 post the error as the raw body of
    ///     the failure there. Gotenberg 9.0.0 removed it: it never sends this url, and a request with a
    ///     webhook that has no <see cref="EventsUrl" /> cannot be sent to it.
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
    ///     Not sent to Gotenberg 9, which has no error url.
    /// </summary>
    public string? ErrorHttpMethod { get; set; }

    /// <summary>
    ///     The url that receives a JSON event for each outcome of the webhook: <c>webhook.success</c> once
    ///     the file is delivered, <c>webhook.error</c> (with the status and message of the failure) when
    ///     the conversion or the delivery fails. Gotenberg 9 requires it with the webhook url and reports
    ///     errors only there. Gotenberg 7 and 8 take it as an alternative to <see cref="ErrorUrl" />
    ///     (Gotenberg 8.40.0 accepts a webhook with either or both); only Gotenberg 8.29.0 and newer
    ///     read it, and the client does not check the release of Gotenberg 7 or 8 for it.
    /// </summary>
    public Uri? EventsUrl
    {
        get => this._eventsUrl;
        set
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (!value.IsAbsoluteUri)
                throw new InvalidOperationException("WebHook url must be absolute");

            this._eventsUrl = value;
        }
    }

    public ExtraHttpHeaders ExtraHttpHeaders { get; } = new();

    /*/// <summary>
    ///  By default, the API will wait 10 seconds before it considers the sending of the resulting PDF to be unsuccessful.
    ///  On a per request basis, this property can override the container environment variable, DEFAULT_WEBHOOK_URL_TIMEOUT
    /// </summary>
    public float? Timeout { get; set; }*/

    public void Validate()
    {
        if (this.TargetUrl != null && this.ErrorUrl == null && this.EventsUrl == null)
            throw new ArgumentNullException(
                nameof(this.ErrorUrl),
                "A webhook error url or events url is required");
    }

    public bool IsConfigured()
    {
        return this.TargetUrl != null && (this.ErrorUrl != null || this.EventsUrl != null);
    }

    /// <summary>
    ///     The exception for a configured webhook that Gotenberg 9 cannot be given, or null: it requires
    ///     the events url and has no error url to send in its place.
    /// </summary>
    internal GotenbergVersionNotSupportedException? FindRemoved(GotenbergVersion runningVersion)
    {
        if (!this.IsConfigured() || this.EventsUrl != null) return null;

        return GotenbergVersionNotSupportedException.Removed(
            "A webhook with an error url (Webhook.ErrorUrl)",
            StageList.Gotenberg9,
            runningVersion,
            "Gotenberg 9 reports errors as webhook.error events sent to the webhook events url, together with the success events: set one with WebhookBuilder.SetEventsUrl.");
    }

    public IEnumerable<(string, string?)> GetHeaders() => this.GetHeaders(GotenbergApiVersion.V8);

    /// <summary>
    ///     The webhook request headers for <paramref name="api" />.
    /// </summary>
    internal IEnumerable<(string, string?)> GetHeaders(GotenbergApiVersion api)
    {
        if (!this.IsConfigured()) return [];

        // The client has checked FindRemoved before rendering for Gotenberg 9; rendering without that
        // check must fail the same way rather than drop the webhook.
        if (api == GotenbergApiVersion.V9 && this.FindRemoved(GotenbergVersion.Unknown) is { } removed) throw removed;

        // Gotenberg 9 has no error url or error method; Gotenberg 7 and 8 get the headers they always did.
        var webHookHeaders = api == GotenbergApiVersion.V9
            ? new List<(string Name, string? Value)>
            {
                (Constants.Gotenberg.Webhook.Url, this.TargetUrl?.ToString()),
                (Constants.Gotenberg.Webhook.HttpMethod, this.HttpMethod),
                (Constants.Gotenberg.Webhook.EventsUrl, this.EventsUrl?.ToString())
            }
            : new List<(string Name, string? Value)>
            {
                (Constants.Gotenberg.Webhook.Url, this.TargetUrl?.ToString()),
                (Constants.Gotenberg.Webhook.HttpMethod, this.HttpMethod),
                (Constants.Gotenberg.Webhook.ErrorUrl, this.ErrorUrl?.ToString()),
                (Constants.Gotenberg.Webhook.ErrorHttpMethod, this.ErrorHttpMethod),
                (Constants.Gotenberg.Webhook.EventsUrl, this.EventsUrl?.ToString())
            };

        return webHookHeaders.Concat(this.ExtraHttpHeaders.GetHeaders())
            .Where(entry => !string.IsNullOrWhiteSpace(entry.Value));
    }
}