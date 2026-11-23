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
using System.Net;

using Gotenberg.Sharp.API.Client.Application.Builders;
using Gotenberg.Sharp.API.Client.Domain.Bookmarks;
using Gotenberg.Sharp.API.Client.Domain.Requests.ApiRequests;
using Gotenberg.Sharp.API.Client.Domain.Settings;
using Gotenberg.Sharp.API.Client.Domain.Shared;
using Gotenberg.Sharp.API.Client.Domain.Stages;

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
        : this(innerClient, new GotenbergVersionCache())
    {
    }

    /// <summary>
    ///     Initializes a client that shares <paramref name="versionCache" /> with the other clients of
    ///     one <c>AddGotenbergSharpClient()</c> registration.
    /// </summary>
    internal GotenbergSharpClient(HttpClient innerClient, GotenbergVersionCache versionCache)
    {
        this.HttpClient = innerClient ?? throw new ArgumentNullException(nameof(innerClient));
        this._versionCache = versionCache ?? throw new ArgumentNullException(nameof(versionCache));

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

    private readonly GotenbergVersionCache _versionCache;

    private GotenbergApiVersion _apiVersion = GotenbergApiVersion.Auto;

    /// <summary>
    /// When true (the default), requests that target a route introduced in a specific Gotenberg
    /// release are checked against the running service before being sent, throwing
    /// <see cref="GotenbergVersionNotSupportedException" /> instead of letting Gotenberg answer with
    /// an opaque 404. The version is fetched once per service address and cached; see
    /// <see cref="GetGotenbergVersionAsync" />.
    /// </summary>
    /// <remarks>
    /// Releases predating the <c>/version</c> route report no version at all. Those are older than
    /// every version this client gates on, so they are treated as unsupported. Set this to false to
    /// skip the check entirely — for instance when a proxy hides <c>/version</c> from the client.
    /// With <see cref="ApiVersion" /> set to <see cref="GotenbergApiVersion.V9" /> the check passes
    /// without reading the version, as every route it gates is older than Gotenberg 9.
    /// </remarks>
    public bool EnforceMinimumVersion { get; set; } = true;

    /// <summary>
    /// The Gotenberg API requests are rendered for. With <see cref="GotenbergApiVersion.Auto" /> (the
    /// default) the client reads the version of the service (see <see cref="GetGotenbergVersionAsync" />)
    /// and uses the API of its major release. <see cref="GotenbergApiVersion.V8" /> (Gotenberg 7 and 8)
    /// and <see cref="GotenbergApiVersion.V9" /> skip that lookup, for instance when a proxy hides
    /// <c>/version</c> from the client; a request on a route with a minimum release still reads the
    /// version for <see cref="EnforceMinimumVersion" />, except with <c>V9</c>. Set from <see cref="GotenbergSharpClientOptions.ApiVersion" />
    /// when the client comes from <c>AddGotenbergSharpClient()</c>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is not a member of <see cref="GotenbergApiVersion" />.</exception>
    public GotenbergApiVersion ApiVersion
    {
        get => this._apiVersion;
        set
        {
            if (!Enum.IsDefined(typeof(GotenbergApiVersion), value))
                throw new ArgumentOutOfRangeException(nameof(value), value, "Use Auto, V8 or V9.");

            this._apiVersion = value;
        }
    }

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
    /// <remarks>
    /// The JSON is returned as Gotenberg wrote it, so its shape depends on the service: ExifTool tags
    /// (<c>Author</c>, <c>PDFVersion</c>, ...) on Gotenberg 7 and 8, eight camelCase fields (<c>title</c>,
    /// <c>author</c>, <c>subject</c>, <c>keywords</c>, <c>creator</c>, <c>producer</c>, <c>creationDate</c>,
    /// <c>modDate</c>, those the document has) on Gotenberg 9.
    /// </remarks>
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
    /// <remarks>
    /// Requires Gotenberg 8.28.0 or newer. The JSON is not translated: a bookmark names its page
    /// <c>page</c> on Gotenberg 8 and <c>pageNumber</c> on Gotenberg 9.
    /// </remarks>
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
    /// Gets the version of the running Gotenberg service, parsed and cached for its address. Returns
    /// <see cref="GotenbergVersion.Unknown" /> when the service does not expose the <c>/version</c>
    /// route or reports something unparsable.
    /// </summary>
    /// <exception cref="GotenbergApiException">
    /// Thrown when <c>GET /version</c> fails with anything but a 404: its message says what Gotenberg
    /// answered and whether to set <see cref="ApiVersion" /> or <see cref="EnforceMinimumVersion" />,
    /// and the original exception is its <see cref="Exception.InnerException" />. The failure is not
    /// cached, so the next call tries again.
    /// </exception>
    /// <remarks>
    /// The clients created by <c>AddGotenbergSharpClient()</c> share the cache of their service
    /// provider; a client constructed directly has its own. When a response reports, in its
    /// <c>Gotenberg-Version</c> header, a version other than the cached one, the cached version is
    /// replaced, so that a service upgraded or rolled back while the application runs is noticed on
    /// the next request.
    /// </remarks>
    public virtual async Task<GotenbergVersion> GetGotenbergVersionAsync(CancellationToken token = default)
    {
        var entry = this.VersionCacheEntry;

        if (entry.Version is { } cached) return cached;

        await entry.Lock.WaitAsync(token).ConfigureAwait(false);

        try
        {
            if (entry.Version is { } read) return read;

            string? reported;

            try
            {
                reported = await this.GetVersion(token).ConfigureAwait(false);
            }
            catch (GotenbergApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Releases predating the /version route answer with a 404. Any other failure —
                // auth, proxy, 5xx — says nothing about the version and is thrown, with what to do
                // about it, rather than being cached as "unknown" for the life of the client.
                reported = null;
            }
            catch (GotenbergApiException ex)
            {
                throw new GotenbergApiException(this.DescribeVersionLookupFailure(ex), ex);
            }

            var parsed = GotenbergVersion.TryParse(reported, out var version)
                ? version!
                : GotenbergVersion.Unknown;

            entry.Version = parsed;

            return parsed;
        }
        finally
        {
            entry.Lock.Release();
        }
    }

    /// <summary>
    /// The message of the exception that reports <paramref name="failure" />, the answer to
    /// <c>GET /version</c>: what Gotenberg answered, why the client reads the version, what to do, then
    /// what Gotenberg said.
    /// </summary>
    private string DescribeVersionLookupFailure(GotenbergApiException failure)
    {
        var answered = $"{(int)failure.StatusCode} ({failure.ReasonPhrase ?? failure.StatusCode.ToString()})";

        var message = this.ApiVersion == GotenbergApiVersion.Auto
            ? $"Gotenberg answered {answered} to GET /version, which the client reads to choose between the Gotenberg 7 and 8 API and the Gotenberg 9 API. If a proxy blocks /version, set {nameof(GotenbergSharpClientOptions)}.{nameof(GotenbergSharpClientOptions.ApiVersion)} (or {nameof(GotenbergSharpClient)}.{nameof(this.ApiVersion)}) to {nameof(GotenbergApiVersion.V8)} or {nameof(GotenbergApiVersion.V9)} to skip the lookup. With {nameof(GotenbergApiVersion.V8)}, a request that needs a minimum Gotenberg release (split, flatten, encrypt, rotate, embed, bookmarks) still reads it unless {nameof(GotenbergSharpClient)}.{nameof(this.EnforceMinimumVersion)} is false."
            : $"Gotenberg answered {answered} to GET /version, which the client reads to check that the service is new enough for a request that needs a minimum Gotenberg release. If a proxy blocks /version, set {nameof(GotenbergSharpClient)}.{nameof(this.EnforceMinimumVersion)} to false to skip the check.";

        if (failure.StatusCode == HttpStatusCode.Unauthorized)
            message += $" Gotenberg refused the credentials, so the conversion would fail as well: check {nameof(GotenbergSharpClientOptions.BasicAuthUsername)} and {nameof(GotenbergSharpClientOptions.BasicAuthPassword)}.";

        return failure.Message.IsSet() ? $"{message} Gotenberg's response: {failure.Message}" : message;
    }

    /// <summary>
    /// True when <see cref="ApiVersion" /> alone satisfies <paramref name="requirement" />: the service
    /// is declared as Gotenberg 9, which has every route that an earlier release introduced. A
    /// requirement of Gotenberg 9 itself or newer is not settled by that and reads the version.
    /// </summary>
    private bool ApiVersionSatisfies(GotenbergFeatureRequirement requirement) =>
        this.ApiVersion == GotenbergApiVersion.V9 && requirement.MinimumVersion < StageList.Gotenberg9;

    /// <summary>
    /// Gets the Gotenberg API requests are rendered for: <see cref="ApiVersion" /> when it is set to
    /// <see cref="GotenbergApiVersion.V8" /> or <see cref="GotenbergApiVersion.V9" />, without any
    /// request; otherwise the API of the major release <see cref="GetGotenbergVersionAsync" /> reports.
    /// A service that does not report its version is older than the <c>/version</c> route and gets
    /// <see cref="GotenbergApiVersion.V8" />.
    /// </summary>
    /// <exception cref="GotenbergApiException">Thrown when reading the version fails for another reason than a missing route; see <see cref="GetGotenbergVersionAsync" />.</exception>
    public virtual async Task<GotenbergApiVersion> GetApiVersionAsync(CancellationToken cancelToken = default)
    {
        if (this.ApiVersion != GotenbergApiVersion.Auto) return this.ApiVersion;

        var running = await this.GetGotenbergVersionAsync(cancelToken).ConfigureAwait(false);

        return running.IsKnown && running.Major >= 9 ? GotenbergApiVersion.V9 : GotenbergApiVersion.V8;
    }

    /// <summary>
    /// Determines whether the running Gotenberg can serve <paramref name="request" />, without sending
    /// it: the service is new enough for its route, the Gotenberg API it is rendered for (see
    /// <see cref="GetApiVersionAsync" />) has an equivalent for each of its options, and the request
    /// can be rendered for that API (a class that overrides only <c>ToHttpContent()</c> cannot be
    /// rendered for Gotenberg 9, nor can a PDF operation with nothing to run there). Requests that
    /// target routes present in every supported release and use no option a release removed are
    /// always supported, without reading the version.
    /// </summary>
    public virtual async Task<bool> SupportsAsync(
        BuildRequestBase request,
        CancellationToken cancelToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        var requirement = request.Requires;

        if (requirement != null
            && !this.ApiVersionSatisfies(requirement)
            && !requirement.IsSatisfiedBy(await this.GetGotenbergVersionAsync(cancelToken).ConfigureAwait(false)))
            return false;

        return await this.FindUnsupportedOptionAsync(request, cancelToken).ConfigureAwait(false) == null;
    }

    /// <summary>
    /// Throws <see cref="GotenbergVersionNotSupportedException" /> when the running Gotenberg is
    /// older than the release that introduced the route <paramref name="request" /> targets, or when
    /// the Gotenberg API it is rendered for has no equivalent for one of its options; throws the
    /// <see cref="InvalidOperationException" /> that sending the request would throw when it cannot
    /// be rendered for that API.
    /// </summary>
    /// <exception cref="GotenbergVersionNotSupportedException">Thrown when the running Gotenberg is too old, or has no equivalent for an option of the request.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the request cannot be rendered for the Gotenberg API of the service: a class that overrides only <c>ToHttpContent()</c> (for Gotenberg 9), or a PDF operation that gives Gotenberg 9 no stage to run.</exception>
    public virtual async Task EnsureSupportedAsync(
        BuildRequestBase request,
        CancellationToken cancelToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        await this.EnsureSupportedAsync(request.Requires, cancelToken).ConfigureAwait(false);

        var unsupported = await this.FindUnsupportedOptionAsync(request, cancelToken).ConfigureAwait(false);

        if (unsupported != null) throw unsupported;
    }

    /// <summary>
    /// The exception sending <paramref name="request" /> would throw before anything goes out because
    /// the API of the service cannot serve it, or null: the first option it has no equivalent for
    /// (<see cref="GotenbergVersionNotSupportedException" />), else the reason it cannot be rendered
    /// (<see cref="InvalidOperationException" />). The API is only resolved for a request that fails
    /// on some API, such as one with a watermark (none on Gotenberg 9) or explicit stages (none on
    /// Gotenberg 7 and 8), so that other requests never cause a version lookup here.
    /// </summary>
    private async Task<Exception?> FindUnsupportedOptionAsync(
        BuildRequestBase request,
        CancellationToken cancelToken)
    {
        if (request.FindUnsupportedOn(GotenbergApiVersion.V8, GotenbergVersion.Unknown) == null
            && request.FindUnsupportedOn(GotenbergApiVersion.V9, GotenbergVersion.Unknown) == null
            && request.FindUnrenderableOn(GotenbergApiVersion.V9) == null)
            return null;

        var (api, running) = await this.ResolveRenderingAsync(cancelToken).ConfigureAwait(false);

        return (Exception?)request.FindUnsupportedOn(api, running) ?? request.FindUnrenderableOn(api);
    }

    private async Task<HttpRequestMessage> RenderAsync(IVersionedApiRequest request, CancellationToken cancelToken)
    {
        var (api, running) = await this.ResolveRenderingAsync(cancelToken).ConfigureAwait(false);

        return request.ToApiRequestMessage(api, running);
    }

    /// <summary>
    /// The API requests are rendered for, and the version of the service to report when one of its
    /// options has no equivalent there: <see cref="GotenbergVersion.Unknown" /> when
    /// <see cref="ApiVersion" /> is set explicitly, which skips the lookup.
    /// </summary>
    private async Task<(GotenbergApiVersion Api, GotenbergVersion Running)> ResolveRenderingAsync(
        CancellationToken cancelToken)
    {
        var api = await this.GetApiVersionAsync(cancelToken).ConfigureAwait(false);

        // With Auto, GetApiVersionAsync has just read the version into the cache.
        var running = this.ApiVersion == GotenbergApiVersion.Auto
            ? await this.GetGotenbergVersionAsync(cancelToken).ConfigureAwait(false)
            : GotenbergVersion.Unknown;

        return (api, running);
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
        await this.EnsureSupportedAsync((request as IRequireGotenbergVersion)?.Requires, cancelToken)
            .ConfigureAwait(false);

        // The version lookup is a plain request, so resolving the API here never re-enters itself.
        using var message = request is IVersionedApiRequest versioned
            ? await this.RenderAsync(versioned, cancelToken).ConfigureAwait(false)
            : request.ToApiRequestMessage();

        var response = await this.HttpClient
            .SendAsync(message, option, cancelToken)
            .ConfigureAwait(false);

        this.RefreshCachedVersion(response);

        cancelToken.ThrowIfCancellationRequested();

        if (response.IsSuccessStatusCode)
            return response;

        throw await GotenbergApiException.CreateAsync(request, response, cancelToken)
            .ConfigureAwait(false);
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
        if (requirement == null || !this.EnforceMinimumVersion || this.ApiVersionSatisfies(requirement)) return;

        var running = await this.GetGotenbergVersionAsync(cancelToken).ConfigureAwait(false);

        if (requirement.IsSatisfiedBy(running)) return;

        throw GotenbergVersionNotSupportedException.Create(requirement, running);
    }

    private GotenbergVersionCache.Entry VersionCacheEntry => this._versionCache.For(this.HttpClient.BaseAddress!);

    /// <summary>
    ///     Both Gotenberg majors report their version on every response, failed ones included.
    /// </summary>
    private void RefreshCachedVersion(HttpResponseMessage response)
    {
        if (this.HttpClient.BaseAddress == null
            || !response.Headers.TryGetValues(Constants.Gotenberg.All.VersionHeader, out var values)
            || !GotenbergVersion.TryParse(values.FirstOrDefault(), out var reported))
            return;

        this.VersionCacheEntry.Refresh(reported!);
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