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

using System.ComponentModel;

using Gotenberg.Sharp.API.Client.Application.Builders;
using Gotenberg.Sharp.API.Client.Domain.Bookmarks;
using Gotenberg.Sharp.API.Client.Domain.Requests.ApiRequests;
using Gotenberg.Sharp.API.Client.Domain.Shared;

namespace Gotenberg.Sharp.API.Client;

/// <summary>
///     C# Client for Gotenberg api
/// </summary>
/// <remarks>
///     <para>
///         Gotenberg:
///         https://gotenberg.dev
///         https://github.com/gotenberg/gotenberg
///     </para>
///     <para>
///         Other clients available:
///         https://github.com/gotenberg/awesome-gotenberg#clients
///     </para>
/// </remarks>
public class GotenbergSharpClient
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public GotenbergSharpClient(string address)
        : this(new Uri(address))
    {
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public GotenbergSharpClient(Uri address)
        : this(new HttpClient { BaseAddress = address })
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="GotenbergSharpClient" /> class.
    /// </summary>
    /// <param name="innerClient"></param>
    /// <remarks>Client was built for DI use</remarks>
    public GotenbergSharpClient(HttpClient innerClient)
    {
        this.HttpClient = innerClient ?? throw new ArgumentNullException(nameof(innerClient));

        if (this.HttpClient.BaseAddress == null)
            throw new InvalidOperationException($"{nameof(innerClient.BaseAddress)} is null");

        this.HttpClient.DefaultRequestHeaders.Add(
            Constants.HttpContent.Headers.UserAgent,
            nameof(GotenbergSharpClient));

        this.HttpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue(
                Constants.HttpContent.MediaTypes.ApplicationPdf));
    }

    protected HttpClient HttpClient { get; }

    private readonly SemaphoreSlim _versionLock = new(1, 1);

    private GotenbergVersion? _cachedVersion;

    /// <summary>
    /// When true (the default), requests that target a route introduced in a specific Gotenberg
    /// release are checked against the running service before being sent, throwing
    /// <see cref="GotenbergVersionNotSupportedException" /> instead of letting Gotenberg answer with
    /// an opaque 404. The version is fetched once per client instance and cached.
    /// </summary>
    /// <remarks>
    /// Releases predating the <c>/version</c> route report no version at all. Those are older than
    /// every version this client gates on, so they are treated as unsupported. Set this to false to
    /// skip the check entirely — for instance when a proxy hides <c>/version</c> from the client.
    /// </remarks>
    public bool EnforceMinimumVersion { get; set; } = true;

    /// <summary>
    /// Converts a remote URL to PDF using Gotenberg's Chromium module.
    /// </summary>
    /// <param name="request">The URL conversion request.</param>
    /// <param name="cancelToken">Cancellation token for the async operation.</param>
    /// <returns>A stream containing the generated PDF.</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    /// <exception cref="GotenbergApiException">Thrown when Gotenberg returns an error response.</exception>
    /// <seealso href="https://gotenberg.dev/docs/routes#convert-with-chromium">Gotenberg Chromium Route Documentation</seealso>
    public virtual Task<Stream> UrlToPdfAsync(
        UrlRequest request,
        CancellationToken cancelToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        return this.ExecuteRequestAsync(request.CreateApiRequest(), cancelToken);
    }

    /// <summary>
    /// Converts a remote URL to PDF using Gotenberg's Chromium module with a builder pattern.
    /// </summary>
    /// <param name="builder">The builder for configuring URL conversion settings.</param>
    /// <param name="cancelToken">Cancellation token for the async operation.</param>
    /// <returns>A stream containing the generated PDF.</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null.</exception>
    /// <exception cref="GotenbergApiException">Thrown when Gotenberg returns an error response.</exception>
    /// <seealso href="https://gotenberg.dev/docs/routes#convert-with-chromium">Gotenberg Chromium Route Documentation</seealso>
    public virtual async Task<Stream> UrlToPdfAsync(
        UrlRequestBuilder builder,
        CancellationToken cancelToken = default)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));

        var urlRequest = await builder.BuildAsync().ConfigureAwait(false);

        return await this.UrlToPdfAsync(urlRequest, cancelToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Converts HTML or Markdown content to PDF using Gotenberg's Chromium module.
    /// </summary>
    /// <param name="request">The HTML/Markdown conversion request.</param>
    /// <param name="cancelToken">Cancellation token for the async operation.</param>
    /// <returns>A stream containing the generated PDF.</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    /// <exception cref="GotenbergApiException">Thrown when Gotenberg returns an error response.</exception>
    /// <seealso href="https://gotenberg.dev/docs/routes#convert-with-chromium">Gotenberg Chromium Route Documentation</seealso>
    public virtual Task<Stream> HtmlToPdfAsync(
        HtmlRequest request,
        CancellationToken cancelToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        return this.ExecuteRequestAsync(request.CreateApiRequest(), cancelToken);
    }

    /// <summary>
    /// Converts HTML or Markdown content to PDF using Gotenberg's Chromium module with a builder pattern.
    /// </summary>
    /// <param name="builder">The builder for configuring HTML/Markdown conversion settings.</param>
    /// <param name="cancelToken">Cancellation token for the async operation.</param>
    /// <returns>A stream containing the generated PDF.</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null.</exception>
    /// <exception cref="GotenbergApiException">Thrown when Gotenberg returns an error response.</exception>
    /// <seealso href="https://gotenberg.dev/docs/routes#convert-with-chromium">Gotenberg Chromium Route Documentation</seealso>
    public virtual async Task<Stream> HtmlToPdfAsync(
        HtmlRequestBuilder builder,
        CancellationToken cancelToken = default)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));

        var htmlRequest = await builder.BuildAsync().ConfigureAwait(false);

        return await this.HtmlToPdfAsync(htmlRequest, cancelToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Merges multiple PDF files into a single PDF using Gotenberg's PDF engines module.
    /// </summary>
    /// <param name="request">The merge request containing PDFs to merge.</param>
    /// <param name="cancelToken">Cancellation token for the async operation.</param>
    /// <returns>A stream containing the merged PDF.</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    /// <exception cref="GotenbergApiException">Thrown when Gotenberg returns an error response.</exception>
    /// <seealso href="https://gotenberg.dev/docs/routes#merge-pdfs-route">Gotenberg Merge PDFs Route Documentation</seealso>
    public virtual Task<Stream> MergePdfsAsync(
        MergeRequest request,
        CancellationToken cancelToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        return this.ExecuteRequestAsync(request.CreateApiRequest(), cancelToken);
    }

    /// <summary>
    /// Converts and merges Office documents (Word, Excel, PowerPoint, etc.) into a single PDF using Gotenberg's LibreOffice module.
    /// </summary>
    /// <param name="request">The merge request containing Office documents to convert and merge.</param>
    /// <param name="cancelToken">Cancellation token for the async operation.</param>
    /// <returns>A stream containing the merged PDF.</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    /// <exception cref="GotenbergApiException">Thrown when Gotenberg returns an error response.</exception>
    /// <seealso href="https://gotenberg.dev/docs/routes#convert-with-libreoffice">Gotenberg LibreOffice Route Documentation</seealso>
    public virtual Task<Stream> MergeOfficeDocsAsync(
        MergeOfficeRequest request,
        CancellationToken cancelToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        return this.ExecuteRequestAsync(request.CreateApiRequest(), cancelToken);
    }

    /// <summary>
    /// Converts and merges Office documents into a single PDF using Gotenberg's LibreOffice module with a builder pattern.
    /// </summary>
    /// <param name="builder">The builder for configuring Office document merge settings.</param>
    /// <param name="cancelToken">Cancellation token for the async operation.</param>
    /// <returns>A stream containing the merged PDF.</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null.</exception>
    /// <exception cref="GotenbergApiException">Thrown when Gotenberg returns an error response.</exception>
    /// <seealso href="https://gotenberg.dev/docs/routes#convert-with-libreoffice">Gotenberg LibreOffice Route Documentation</seealso>
    public virtual async Task<Stream> MergeOfficeDocsAsync(
        MergeOfficeBuilder builder,
        CancellationToken cancelToken = default)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));

        var mergeOfficeRequest = await builder.BuildAsync().ConfigureAwait(false);

        return await this.MergeOfficeDocsAsync(mergeOfficeRequest, cancelToken);
    }

    /// <summary>
    /// Converts existing PDF files to PDF/A formats or applies transformations like flattening using Gotenberg's PDF engines module.
    /// </summary>
    /// <param name="request">The PDF conversion request containing PDFs to convert and conversion settings.</param>
    /// <param name="cancelToken">Cancellation token for the async operation.</param>
    /// <returns>A stream containing the converted PDF.</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    /// <exception cref="GotenbergApiException">Thrown when Gotenberg returns an error response.</exception>
    /// <seealso href="https://gotenberg.dev/docs/routes#convert-into-pdfa--pdfua-route">Gotenberg PDF/A Conversion Route Documentation</seealso>
    public virtual Task<Stream> ConvertPdfDocumentsAsync(
        PdfConversionRequest request,
        CancellationToken cancelToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        return this.ExecuteRequestAsync(request.CreateApiRequest(), cancelToken);
    }

    /// <summary>
    /// Converts existing PDF files to PDF/A formats or applies transformations using a builder pattern.
    /// </summary>
    /// <param name="builder">The builder for configuring PDF conversion settings.</param>
    /// <param name="cancelToken">Cancellation token for the async operation.</param>
    /// <returns>A stream containing the converted PDF.</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null.</exception>
    /// <exception cref="GotenbergApiException">Thrown when Gotenberg returns an error response.</exception>
    /// <seealso href="https://gotenberg.dev/docs/routes#convert-into-pdfa--pdfua-route">Gotenberg PDF/A Conversion Route Documentation</seealso>
    public virtual async Task<Stream> ConvertPdfDocumentsAsync(
        PdfConversionBuilder builder,
        CancellationToken cancelToken = default)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));

        var request = await builder.BuildAsync().ConfigureAwait(false);

        return await this.ExecuteRequestAsync(request.CreateApiRequest(), cancelToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Captures a screenshot of HTML content using Gotenberg's Chromium module.
    /// </summary>
    /// <param name="request">The HTML screenshot request.</param>
    /// <param name="cancelToken">Cancellation token for the async operation.</param>
    /// <returns>A stream containing the screenshot image (PNG, JPEG, or WebP).</returns>
    /// <seealso href="https://gotenberg.dev/docs/convert-with-chromium/screenshot-html">Gotenberg Screenshot HTML Documentation</seealso>
    public virtual Task<Stream> ScreenshotHtmlAsync(
        ScreenshotHtmlRequest request,
        CancellationToken cancelToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        return this.ExecuteRequestAsync(request.CreateApiRequest(), cancelToken);
    }

    /// <summary>
    /// Captures a screenshot of HTML content using a builder pattern.
    /// </summary>
    public virtual async Task<Stream> ScreenshotHtmlAsync(
        ScreenshotHtmlRequestBuilder builder,
        CancellationToken cancelToken = default)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));

        var request = await builder.BuildAsync().ConfigureAwait(false);

        return await this.ScreenshotHtmlAsync(request, cancelToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Captures a screenshot of a URL using Gotenberg's Chromium module.
    /// </summary>
    /// <param name="request">The URL screenshot request.</param>
    /// <param name="cancelToken">Cancellation token for the async operation.</param>
    /// <returns>A stream containing the screenshot image (PNG, JPEG, or WebP).</returns>
    /// <seealso href="https://gotenberg.dev/docs/convert-with-chromium/screenshot-url">Gotenberg Screenshot URL Documentation</seealso>
    public virtual Task<Stream> ScreenshotUrlAsync(
        ScreenshotUrlRequest request,
        CancellationToken cancelToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        return this.ExecuteRequestAsync(request.CreateApiRequest(), cancelToken);
    }

    /// <summary>
    /// Captures a screenshot of a URL using a builder pattern.
    /// </summary>
    public virtual async Task<Stream> ScreenshotUrlAsync(
        ScreenshotUrlRequestBuilder builder,
        CancellationToken cancelToken = default)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));

        var request = await builder.BuildAsync().ConfigureAwait(false);

        return await this.ScreenshotUrlAsync(request, cancelToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes a standalone PDF engine operation (flatten, rotate, split, encrypt, metadata).
    /// </summary>
    public virtual Task<Stream> ExecutePdfEngineAsync(
        PdfEngineRequest request,
        CancellationToken cancelToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        return this.ExecuteRequestAsync(request.CreateApiRequest(), cancelToken);
    }

    /// <summary>
    /// Executes a standalone PDF engine operation using a builder.
    /// </summary>
    public virtual async Task<Stream> ExecutePdfEngineAsync<TRequest>(
        PdfEngineBuilder<TRequest> builder,
        CancellationToken cancelToken = default)
        where TRequest : PdfEngineRequest
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));

        var request = await builder.BuildAsync().ConfigureAwait(false);

        return await this.ExecutePdfEngineAsync(request, cancelToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Reads metadata from PDF files. Returns JSON string keyed by filename.
    /// </summary>
    public virtual async Task<string> ReadPdfMetadataAsync(
        PdfEngineBuilder<ReadMetadataRequest> builder,
        CancellationToken cancelToken = default)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));

        var request = await builder.BuildAsync().ConfigureAwait(false);

        return await this.ReadResponseAsStringAsync(request.CreateApiRequest(), cancelToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Reads the document outline (table of contents) from PDF files, keyed by the filename each
    /// PDF was uploaded under. Files without bookmarks come back with an empty outline.
    /// </summary>
    /// <remarks>Requires Gotenberg 8.28.0 or newer.</remarks>
    /// <exception cref="GotenbergVersionNotSupportedException">Thrown when the running Gotenberg is too old.</exception>
    /// <seealso href="https://gotenberg.dev/docs/manipulate-pdfs/read-bookmarks">Gotenberg Read Bookmarks Documentation</seealso>
    public virtual async Task<IReadOnlyDictionary<string, IReadOnlyList<Bookmark>>> ReadPdfBookmarksAsync(
        PdfEngineBuilder<ReadBookmarksRequest> builder,
        CancellationToken cancelToken = default)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));

        var request = await builder.BuildAsync().ConfigureAwait(false);

        return await this.ReadPdfBookmarksAsync(request, cancelToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Reads the document outline (table of contents) from PDF files, keyed by the filename each
    /// PDF was uploaded under. Files without bookmarks come back with an empty outline.
    /// </summary>
    /// <remarks>Requires Gotenberg 8.28.0 or newer.</remarks>
    /// <exception cref="GotenbergVersionNotSupportedException">Thrown when the running Gotenberg is too old.</exception>
    public virtual async Task<IReadOnlyDictionary<string, IReadOnlyList<Bookmark>>> ReadPdfBookmarksAsync(
        ReadBookmarksRequest request,
        CancellationToken cancelToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        var json = await this.ReadResponseAsStringAsync(request.CreateApiRequest(), cancelToken)
            .ConfigureAwait(false);

        var parsed = JsonConvert.DeserializeObject<Dictionary<string, List<Bookmark>?>>(json)
                     ?? new Dictionary<string, List<Bookmark>?>();

        return parsed.ToDictionary(
            entry => entry.Key,
            entry => (IReadOnlyList<Bookmark>)(entry.Value ?? new List<Bookmark>()));
    }

    /// <summary>
    /// Reads the document outline from PDF files as the raw JSON Gotenberg returned, keyed by filename.
    /// Use <see cref="ReadPdfBookmarksAsync(PdfEngineBuilder{ReadBookmarksRequest},CancellationToken)" />
    /// for a deserialized outline.
    /// </summary>
    /// <remarks>Requires Gotenberg 8.28.0 or newer.</remarks>
    /// <exception cref="GotenbergVersionNotSupportedException">Thrown when the running Gotenberg is too old.</exception>
    public virtual async Task<string> ReadPdfBookmarksJsonAsync(
        PdfEngineBuilder<ReadBookmarksRequest> builder,
        CancellationToken cancelToken = default)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));

        var request = await builder.BuildAsync().ConfigureAwait(false);

        return await this.ReadResponseAsStringAsync(request.CreateApiRequest(), cancelToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the version of the running Gotenberg service, parsed and cached for the lifetime of this
    /// client. Returns <see cref="GotenbergVersion.Unknown" /> when the service does not expose the
    /// <c>/version</c> route or reports something unparsable.
    /// </summary>
    public virtual async Task<GotenbergVersion> GetGotenbergVersionAsync(CancellationToken token = default)
    {
        if (this._cachedVersion != null) return this._cachedVersion;

        await this._versionLock.WaitAsync(token).ConfigureAwait(false);

        try
        {
            if (this._cachedVersion != null) return this._cachedVersion;

            string? reported;

            try
            {
                reported = await this.GetVersion(token).ConfigureAwait(false);
            }
            catch (GotenbergApiException)
            {
                // Releases predating the /version route answer with a 404.
                reported = null;
            }

            this._cachedVersion = GotenbergVersion.TryParse(reported, out var version)
                ? version!
                : GotenbergVersion.Unknown;

            return this._cachedVersion;
        }
        finally
        {
            this._versionLock.Release();
        }
    }

    /// <summary>
    /// Determines whether the running Gotenberg is new enough to serve <paramref name="request" />,
    /// without sending it. Requests that target routes present in every supported release are
    /// always supported.
    /// </summary>
    public virtual async Task<bool> SupportsAsync(
        BuildRequestBase request,
        CancellationToken cancelToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        var requirement = request.Requires;

        if (requirement == null) return true;

        var running = await this.GetGotenbergVersionAsync(cancelToken).ConfigureAwait(false);

        return requirement.IsSatisfiedBy(running);
    }

    /// <summary>
    /// Throws <see cref="GotenbergVersionNotSupportedException" /> when the running Gotenberg is
    /// older than the release that introduced the route <paramref name="request" /> targets.
    /// </summary>
    /// <exception cref="GotenbergVersionNotSupportedException">Thrown when the running Gotenberg is too old.</exception>
    public virtual async Task EnsureSupportedAsync(
        BuildRequestBase request,
        CancellationToken cancelToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        await this.EnsureSupportedAsync(request.Requires, cancelToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the current version of Gotenberg.
    /// Custom variants of Gotenberg may not print a strict semver version.
    /// For instance, the live demo prints 8.17.0-live-demo-snapshot.
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public virtual async Task<string?> GetVersion(CancellationToken token = default)
    {
        var request = new GetApiRequestImpl(Constants.Gotenberg.All.ApiPaths.Version);

        var response = await SendRequestAsync(request, HttpCompletionOption.ResponseContentRead, token);

        if (response.IsSuccessStatusCode)
        {
#if NET5_0_OR_GREATER
            return await response.Content.ReadAsStringAsync(token);
#else
            return await response.Content.ReadAsStringAsync();
#endif
        }

        return null;
    }

    /// <summary>
    /// Sends a request to Gotenberg configured for asynchronous webhook processing. Returns immediately after
    /// Gotenberg accepts the request. Gotenberg will POST the generated PDF to the configured webhook URL.
    /// </summary>
    /// <typeparam name="TBuilder">The request type.</typeparam>
    /// <typeparam name="TRequest">The builder type.</typeparam>
    /// <param name="builder">The builder with webhook configuration.</param>
    /// <param name="cancelToken">Cancellation token for the async operation.</param>
    /// <returns>A task that completes when Gotenberg accepts the request (not when PDF is generated).</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the request doesn't have a webhook configured.</exception>
    /// <exception cref="GotenbergApiException">Thrown when Gotenberg returns an error response.</exception>
    /// <remarks>
    /// Use this method when you want asynchronous PDF generation with callback. Gotenberg will generate the PDF
    /// and POST it to your webhook URL. This method returns as soon as Gotenberg accepts the request.
    /// </remarks>
    public virtual async Task FireWebhookAndForgetAsync<TBuilder, TRequest>(
        BaseBuilder<TBuilder, TRequest> builder,
        CancellationToken cancelToken = default)
        where TBuilder : BuildRequestBase where TRequest : BaseBuilder<TBuilder, TRequest>
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));

        var request = await builder.BuildAsync().ConfigureAwait(false);

        await this.FireWebhookAndForgetAsync(request, cancelToken);
    }

    /// <summary>
    /// Sends a request to Gotenberg configured for asynchronous webhook processing. Returns immediately after
    /// Gotenberg accepts the request. Gotenberg will POST the generated PDF to the configured webhook URL.
    /// </summary>
    /// <param name="request">The request with webhook configuration.</param>
    /// <param name="cancelToken">Cancellation token for the async operation.</param>
    /// <returns>A task that completes when Gotenberg accepts the request (not when PDF is generated).</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the request doesn't have a webhook configured.</exception>
    /// <exception cref="GotenbergApiException">Thrown when Gotenberg returns an error response.</exception>
    public virtual async Task FireWebhookAndForgetAsync(
        BuildRequestBase request,
        CancellationToken cancelToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        var apiRequest = request.CreateApiRequest();

        await this.FireWebhookAndForgetAsync(apiRequest, cancelToken);
    }

    /// <summary>
    /// Sends a request to Gotenberg configured for asynchronous webhook processing. Returns immediately after
    /// Gotenberg accepts the request. Gotenberg will POST the generated PDF to the configured webhook URL.
    /// </summary>
    /// <param name="request">The API request with webhook configuration.</param>
    /// <param name="cancelToken">Cancellation token for the async operation.</param>
    /// <returns>A task that completes when Gotenberg accepts the request (not when PDF is generated).</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the request doesn't have a webhook configured.</exception>
    /// <exception cref="GotenbergApiException">Thrown when Gotenberg returns an error response.</exception>
    public virtual async Task FireWebhookAndForgetAsync(
        IApiRequest request,
        CancellationToken cancelToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (!request.IsWebhookRequest)
            throw new InvalidOperationException(
                "Only call this for webhook configured requests");

        using var response = await this.SendRequestAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancelToken);
    }

    protected virtual async Task<Stream> ExecuteRequestAsync(
        IApiRequest request,
        CancellationToken cancelToken)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        using var response = await this.SendRequestAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancelToken);

        var ms = new MemoryStream();

#if NET5_0_OR_GREATER
        await response.Content.CopyToAsync(ms, cancelToken);
#else
        await response.Content.CopyToAsync(ms).ConfigureAwait(false);
#endif

        ms.Position = 0;

        return ms;
    }

    /// <summary>
    ///     Send the API request
    /// </summary>
    /// <param name="request"></param>
    /// <param name="option"></param>
    /// <param name="cancelToken"></param>
    /// <returns></returns>
    protected virtual async Task<HttpResponseMessage> SendRequestAsync(
        IApiRequest request,
        HttpCompletionOption option,
        CancellationToken cancelToken)
    {
        await this.EnsureSupportedAsync(request.Requires, cancelToken).ConfigureAwait(false);

        using var message = request.ToApiRequestMessage();

        var response = await this.HttpClient
            .SendAsync(message, option, cancelToken)
            .ConfigureAwait(false);

        cancelToken.ThrowIfCancellationRequested();

        if (response.IsSuccessStatusCode)
            return response;

        throw GotenbergApiException.Create(request, response);
    }

    /// <summary>
    ///     Verifies the running Gotenberg satisfies a request's declared minimum version. Requests
    ///     without a declared minimum — and the version lookup itself — short-circuit, so this never
    ///     re-enters itself.
    /// </summary>
    private async Task EnsureSupportedAsync(
        GotenbergFeatureRequirement? requirement,
        CancellationToken cancelToken)
    {
        if (requirement == null || !this.EnforceMinimumVersion) return;

        var running = await this.GetGotenbergVersionAsync(cancelToken).ConfigureAwait(false);

        if (requirement.IsSatisfiedBy(running)) return;

        throw GotenbergVersionNotSupportedException.Create(requirement, running);
    }

    private async Task<string> ReadResponseAsStringAsync(
        IApiRequest request,
        CancellationToken cancelToken)
    {
        using var response = await this.SendRequestAsync(
            request,
            HttpCompletionOption.ResponseContentRead,
            cancelToken).ConfigureAwait(false);

#if NET5_0_OR_GREATER
        return await response.Content.ReadAsStringAsync(cancelToken).ConfigureAwait(false);
#else
        return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#endif
    }
}