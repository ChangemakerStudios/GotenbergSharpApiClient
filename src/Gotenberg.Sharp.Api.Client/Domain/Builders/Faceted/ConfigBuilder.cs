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

using Gotenberg.Sharp.API.Client.Domain.Pages;

namespace Gotenberg.Sharp.API.Client.Domain.Builders.Faceted;

/// <summary>
/// Configures request-level settings including page ranges, webhooks, result filename, and trace ID for correlation.
/// </summary>
public sealed class ConfigBuilder
{
    private readonly RequestConfig _requestConfig;

    internal ConfigBuilder(RequestConfig requestConfig)
    {
        this._requestConfig = requestConfig;
    }

    /// <summary>
    /// Specifies which pages to include in the resulting PDF using Chrome print format (e.g., "1-3,5,7-9").
    /// </summary>
    /// <param name="pageRanges">Page range specification string, or null for all pages.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ConfigBuilder SetPageRanges(string? pageRanges)
    {
        this._requestConfig.PageRanges = Pages.PageRanges.Create(pageRanges);

        return this;
    }

    /// <summary>
    /// Sets page ranges using a PageRanges instance.
    /// </summary>
    /// <param name="pageRanges">Pre-configured PageRanges instance, or null for all pages.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ConfigBuilder SetPageRanges(PageRanges? pageRanges)
    {
        this._requestConfig.PageRanges = pageRanges ?? Pages.PageRanges.All;

        return this;
    }

    [Obsolete("Renamed: Use SetPageRanges")]
    public ConfigBuilder PageRanges(string pageRanges)
    {
        return this.SetPageRanges(pageRanges);
    }

    /// <summary>
    /// Sets the suggested filename for the resulting PDF when Gotenberg returns it.
    /// Useful when using webhooks to identify which request generated which PDF.
    /// </summary>
    /// <param name="resultFileName">Desired filename for the PDF result.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when filename is null or empty.</exception>
    public ConfigBuilder SetResultFileName(string resultFileName)
    {
        if (resultFileName.IsNotSet())
            throw new ArgumentException("Cannot be null or empty", nameof(resultFileName));

        this._requestConfig.ResultFileName = resultFileName;

        return this;
    }

    [Obsolete("Renamed: Use SetResultFileName")]
    public ConfigBuilder ResultFileName(string resultFileName)
    {
        return this.SetResultFileName(resultFileName);
    }

    /// <summary>
    /// Sets a trace ID for correlating this request across logs and metrics in both your application and Gotenberg.
    /// Useful for debugging and monitoring distributed systems.
    /// </summary>
    /// <param name="trace">Trace or correlation ID for this request.</param>
    /// <returns>The builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when trace is null or empty.</exception>
    public ConfigBuilder SetTrace(string trace)
    {
        if (trace.IsNotSet())
            throw new ArgumentException("Trace cannot be null or empty", nameof(trace));

        this._requestConfig.Trace = trace;

        return this;
    }

    /// <summary>
    /// Configures webhook settings for asynchronous PDF generation. When configured, Gotenberg will POST the
    /// generated PDF to the specified URL instead of returning it in the response.
    /// </summary>
    /// <param name="action">Configuration action for webhook settings.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ConfigBuilder AddWebhook(Action<WebhookBuilder> action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));

        this._requestConfig.Webhook ??= new Webhook();

        action(new WebhookBuilder(this._requestConfig.Webhook));

        return this;
    }

    /// <summary>
    /// Sets pre-configured webhook settings.
    /// </summary>
    /// <param name="webhook">Pre-configured Webhook instance.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public ConfigBuilder SetWebhook(Webhook webhook)
    {
        this._requestConfig.Webhook = webhook ?? throw new ArgumentNullException(nameof(webhook));

        return this;
    }

    [Obsolete("Renamed: Use SetWebhook instead.")]
    public ConfigBuilder AddWebhook(Webhook webhook)
    {
        return this.SetWebhook(webhook);
    }
}