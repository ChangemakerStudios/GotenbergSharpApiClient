// Copyright 2019-2026 Chris Mohan, Jaben Cargman
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

using System.Text;

using Gotenberg.Sharp.API.Client.Domain.Requests;
using Gotenberg.Sharp.API.Client.Domain.Requests.ApiRequests;
using Gotenberg.Sharp.API.Client.Domain.Shared;

namespace GotenbergSharpClient.Tests;

/// <summary>
/// One part of a rendered multipart request.
/// </summary>
public sealed record RenderedPart(string? Name, string? FileName, string? MediaType, string Value);

/// <summary>
/// What a request puts on the wire, read back from the <see cref="HttpRequestMessage" /> without a
/// server: method, path, request headers and the multipart form parts in the order they are sent.
/// </summary>
public sealed record RenderedRequest(
    string Method,
    string Path,
    IReadOnlyList<(string Name, string Value)> Headers,
    IReadOnlyList<RenderedPart> Parts)
{
    public IEnumerable<string?> PartNames => this.Parts.Select(p => p.Name);

    /// <summary>
    /// The values of the request header <paramref name="name" />, empty when it is not sent.
    /// </summary>
    public IEnumerable<string> Header(string name) =>
        this.Headers.Where(h => h.Name == name).Select(h => h.Value);

    /// <summary>
    /// The single part named <paramref name="name" />; fails when there is none or more than one.
    /// </summary>
    public RenderedPart Part(string name) => this.Parts.Single(p => p.Name == name);

    public static RenderedRequest From(HttpRequestMessage message)
    {
        var headers = message.Headers
            .SelectMany(h => h.Value.Select(v => (h.Key, v)))
            .ToList();

        var parts = message.Content is MultipartContent multipart
            ? multipart.Select(ToPart).ToList()
            : [];

        return new RenderedRequest(message.Method.Method, message.RequestUri?.ToString() ?? "", headers, parts);
    }

    /// <summary>
    /// A stable text form for snapshot comparison: one line per header and part, each part's value
    /// indented below it.
    /// </summary>
    public string ToSnapshot()
    {
        var text = new StringBuilder();

        text.Append(this.Method).Append(' ').Append(this.Path).Append('\n');

        foreach (var (name, value) in this.Headers)
            text.Append("header ").Append(name).Append(": ").Append(value).Append('\n');

        foreach (var part in this.Parts)
        {
            text.Append("part name=").Append(part.Name);
            if (part.FileName != null) text.Append(" filename=").Append(part.FileName);
            if (part.MediaType != null) text.Append(" type=").Append(part.MediaType);
            text.Append('\n');

            foreach (var line in part.Value.Replace("\r\n", "\n").Split('\n'))
                text.Append("    ").Append(line).Append('\n');
        }

        return text.ToString();
    }

    private static RenderedPart ToPart(HttpContent content) =>
        new(
            content.Headers.ContentDisposition?.Name?.Trim('"'),
            content.Headers.ContentDisposition?.FileName?.Trim('"'),
            content.Headers.ContentType?.MediaType,
            content.ReadAsStringAsync().GetAwaiter().GetResult());
}

/// <summary>
/// Renders requests the way the client would send them, without a server.
/// </summary>
public static class RequestRendering
{
    /// <summary>
    /// Renders through the public <see cref="IApiRequest.ToApiRequestMessage" />.
    /// </summary>
    public static RenderedRequest Render(this IApiRequest request)
    {
        using var message = request.ToApiRequestMessage();

        return RenderedRequest.From(message);
    }

    /// <summary>
    /// Renders through the public <see cref="IApiRequest.ToApiRequestMessage" />.
    /// </summary>
    public static RenderedRequest Render(this BuildRequestBase request) => request.CreateApiRequest().Render();

    /// <summary>
    /// Renders for <paramref name="api" /> the way the client does once it knows the API of the
    /// service; an exception it throws reports <paramref name="runningVersion" />, unknown by default
    /// as when the API is set explicitly.
    /// </summary>
    public static RenderedRequest RenderFor(
        this IApiRequest request,
        GotenbergApiVersion api,
        GotenbergVersion? runningVersion = null)
    {
        using var message = ((IVersionedApiRequest)request)
            .ToApiRequestMessage(api, runningVersion ?? GotenbergVersion.Unknown);

        return RenderedRequest.From(message);
    }

    /// <summary>
    /// Validates <paramref name="request" /> and renders it for <paramref name="api" /> the way the
    /// client does once it knows the API of the service.
    /// </summary>
    public static RenderedRequest RenderFor(
        this BuildRequestBase request,
        GotenbergApiVersion api,
        GotenbergVersion? runningVersion = null) =>
        request.CreateApiRequest().RenderFor(api, runningVersion);
}
