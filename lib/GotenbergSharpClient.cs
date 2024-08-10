//  Copyright 2019-2024 Chris Mohan, Jaben Cargman
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

using Gotenberg.Sharp.API.Client.Domain.Builders;

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

    /// <summary>
    ///     For remote URL conversions. Works just like <see cref="HtmlToPdfAsync" />
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancelToken"></param>
    /// <returns></returns>
    public virtual Task<Stream> UrlToPdfAsync(
        UrlRequest request,
        CancellationToken cancelToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        return this.ExecuteRequestAsync(request.CreateApiRequest(), cancelToken);
    }

    /// <summary>
    ///     For remote URL conversions. Works just like <see cref="HtmlToPdfAsync" />
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="cancelToken"></param>
    /// <returns></returns>
    public virtual async Task<Stream> UrlToPdfAsync(
        UrlRequestBuilder builder,
        CancellationToken cancelToken = default)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));

        var urlRequest = await builder.BuildAsync().ConfigureAwait(false);

        return await this.UrlToPdfAsync(urlRequest, cancelToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Converts the specified request to a PDF document.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancelToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public virtual Task<Stream> HtmlToPdfAsync(
        HtmlRequest request,
        CancellationToken cancelToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        return this.ExecuteRequestAsync(request.CreateApiRequest(), cancelToken);
    }

    /// <summary>
    ///     Converts the specified request to a PDF document.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="cancelToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public virtual async Task<Stream> HtmlToPdfAsync(
        HtmlRequestBuilder builder,
        CancellationToken cancelToken = default)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));

        var htmlRequest = await builder.BuildAsync().ConfigureAwait(false);

        return await this.HtmlToPdfAsync(htmlRequest, cancelToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Merges items specified by the request
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancelToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public virtual Task<Stream> MergePdfsAsync(
        MergeRequest request,
        CancellationToken cancelToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        return this.ExecuteRequestAsync(request.CreateApiRequest(), cancelToken);
    }

    /// <summary>
    ///     Converts one or more office documents into a merged pdf.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancelToken"></param>
    public virtual Task<Stream> MergeOfficeDocsAsync(
        MergeOfficeRequest request,
        CancellationToken cancelToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        return this.ExecuteRequestAsync(request.CreateApiRequest(), cancelToken);
    }

    /// <summary>
    ///     Converts one or more office documents into a merged pdf.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="cancelToken"></param>
    public virtual async Task<Stream> MergeOfficeDocsAsync(
        MergeOfficeBuilder builder,
        CancellationToken cancelToken = default)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));

        var mergeOfficeRequest = await builder.BuildAsync().ConfigureAwait(false);

        return await this.MergeOfficeDocsAsync(mergeOfficeRequest, cancelToken);
    }

    public virtual Task<Stream> ConvertPdfDocumentsAsync(
        PdfConversionRequest request,
        CancellationToken cancelToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        return this.ExecuteRequestAsync(request.CreateApiRequest(), cancelToken);
    }

    public virtual async Task<Stream> ConvertPdfDocumentsAsync(
        PdfConversionBuilder builder,
        CancellationToken cancelToken = default)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));

        var request = await builder.BuildAsync().ConfigureAwait(false);

        return await this.ExecuteRequestAsync(request.CreateApiRequest(), cancelToken)
            .ConfigureAwait(false);
    }

    public virtual async Task FireWebhookAndForgetAsync<TBuilder, TRequest>(
        BaseBuilder<TBuilder, TRequest> builder,
        CancellationToken cancelToken = default)
        where TBuilder : BuildRequestBase where TRequest : BaseBuilder<TBuilder, TRequest>
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));

        var request = await builder.BuildAsync().ConfigureAwait(false);

        await this.FireWebhookAndForgetAsync(request, cancelToken);
    }

    public virtual async Task FireWebhookAndForgetAsync(
        BuildRequestBase request,
        CancellationToken cancelToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        var apiRequest = request.CreateApiRequest();

        await this.FireWebhookAndForgetAsync(apiRequest, cancelToken);
    }

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
        using var message = request.ToApiRequestMessage();

        var response = await this.HttpClient
            .SendAsync(message, option, cancelToken)
            .ConfigureAwait(false);

        cancelToken.ThrowIfCancellationRequested();

        if (response.IsSuccessStatusCode)
            return response;

        throw GotenbergApiException.Create(request, response);
    }
}