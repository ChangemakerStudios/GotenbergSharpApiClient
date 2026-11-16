# Advanced Features

!!! example "Working Examples"
    [Webhook](https://github.com/ChangemakerStudios/GotenbergSharpApiClient/tree/develop/examples/Webhook) |
    [UrlsToMergedPdf](https://github.com/ChangemakerStudios/GotenbergSharpApiClient/tree/develop/examples/UrlsToMergedPdf) |
    [DIExample](https://github.com/ChangemakerStudios/GotenbergSharpApiClient/tree/develop/examples/DIExample)

## Webhooks

All request types support asynchronous webhook processing. Gotenberg generates the PDF and POSTs it to your webhook URL.

```csharp
var builder = new UrlRequestBuilder()
    .SetUrl("https://www.example.com")
    .ConfigureRequest(req =>
    {
        req.AddWebhook(hook =>
        {
            hook.SetUrl("http://host.docker.internal:5000/api/webhook/pdf")
                .SetErrorUrl("http://host.docker.internal:5000/api/webhook/error")
                .AddExtraHeader("custom-header", "value");
        });
        req.SetPageRanges("1-2");
    })
    .WithPageProperties(pp => pp.SetPaperSize(PaperSizes.A4));

await sharpClient.FireWebhookAndForgetAsync(builder);
```

### Events URL and Gotenberg 9

`SetEventsUrl` sets the URL that receives a JSON event for each outcome of the webhook: `webhook.success` once the file is delivered and `webhook.error` when the conversion or the delivery fails.

| Gotenberg | Error URL (`SetErrorUrl`) | Events URL (`SetEventsUrl`) |
|-----------|---------------------------|-----------------------------|
| 7 and 8 before 8.29.0 | Required: receives the error as the raw body of the failure | Not read |
| 8.29.0 and newer | Optional if there is an events URL | Optional if there is an error URL; both can be set |
| 9 | Removed: never sent | Required |

A webhook needs an error URL or an events URL. The client does not check the release of Gotenberg 7 or 8 for the events URL.

Gotenberg 9 has no error URL, so a webhook with only `SetErrorUrl` throws `GotenbergVersionNotSupportedException` before anything is sent to it (`SupportsAsync` answers `false`). The error URL is not sent as the events URL: it received the raw error, the events URL receives JSON events of both outcomes. To run on every release, set both URLs: Gotenberg 7 and 8 get the error URL (and the events URL from 8.29.0), Gotenberg 9 gets the events URL only.

```csharp
hook.SetUrl("http://host.docker.internal:5000/api/webhook/pdf")
    .SetErrorUrl("http://host.docker.internal:5000/api/webhook/error")
    .SetEventsUrl("http://host.docker.internal:5000/api/webhook/events");
```

A failed conversion reaches the events URL as a `webhook.error` event; the `status` and `message` are those of the error:

```json
{
  "event": "webhook.error",
  "correlationId": "…",
  "timestamp": "2026-11-16T10:15:30.123456789Z",
  "error": {
    "status": 400,
    "message": "…"
  }
}
```

A `webhook.success` event has the same `event`, `correlationId` and `timestamp` fields and no `error`. Gotenberg 9 refuses a webhook on a route that answers JSON, such as reading metadata, with a 400.

## PDF Output Options

Available on all request types via `SetPdfOutputOptions()`:

| Method | Description |
|--------|-------------|
| `SetPdfFormat(PdfFormat.A2b)` | Convert to PDF/A (A1b, A2b, A3b) |
| `SetPdfUa()` | Enable PDF/UA accessibility |
| `SetFlatten()` | Flatten form fields |
| `SetGenerateTaggedPdf()` | Embed logical structure tags |
| `SetMetadata(dict)` | Write XMP metadata |
| `SetEncryption(user, owner)` | Password-protect the PDF |

## DDD Value Objects

The client uses validated value objects to prevent invalid state at construction time. Invalid values throw immediately rather than failing at the Gotenberg API level.

| Value Object | Constraint | Example |
|-------------|------------|---------|
| `CssSelector` | Non-empty string | `CssSelector.Create("#app")` |
| `GotenbergStatusCode` | 100-599 | `GotenbergStatusCode.Create(499)` |
| `DomainName` | Non-empty, trimmed | `DomainName.Create("cdn.example.com")` |
| `EmulatedMediaFeature` | Non-empty name + value | `EmulatedMediaFeature.PrefersColorScheme("dark")` |
| `PdfPassword` | Non-empty (redacted ToString) | `PdfPassword.Create("secret")` |
| `ImageQuality` | 1-100 | `ImageQuality.Create(85)` |
| `ImageResolution` | 75/150/300/600/1200 DPI | `ImageResolution.Dpi300` |
| `RotationAngle` | 90/180/270 | `RotationAngle.Degrees90` |
| `PageRanges` | Validated format "1-3,5" | `PageRanges.Create("1-3,5,8-10")` |
| `ScreenDimension` | Positive integer | `ScreenDimension.Create(1920)` |
| `CompressionQuality` | 0-100 | `CompressionQuality.Create(90)` |

## Merge Multiple URLs

Build a multi-page PDF by merging the front pages of multiple websites:

```csharp
var sites = new[] { "https://example.com", "https://example.org" }
    .Select(u => new Uri(u));

var tasks = sites.Select(uri =>
{
    var builder = new UrlRequestBuilder()
        .SetUrl(uri)
        .ConfigureRequest(r => r.SetPageRanges("1-2"))
        .WithPageProperties(pp => pp.UseChromeDefaults().SetLandscape());
    return sharpClient.UrlToPdfAsync(builder);
});

var pdfs = await Task.WhenAll(tasks);

var mergeBuilder = new MergeBuilder()
    .WithAssets(b => b.AddItems(
        pdfs.Select((s, i) => KeyValuePair.Create($"{i}.pdf", s))));

var merged = await sharpClient.MergePdfsAsync(mergeBuilder);
```

## Request Configuration

All builders support `ConfigureRequest()` for cross-cutting request settings:

```csharp
builder.ConfigureRequest(config =>
{
    config.SetTrace("my-request-id")        // Custom trace ID for logs
        .SetPageRanges("1-5")               // Page range selection
        .SetResultFileName("output.pdf");   // Output filename
});
```

## Tracing a Failed Request

When Gotenberg returns an error, `GotenbergApiException.Trace` holds the `Gotenberg-Trace` header from the response, so you can find the request in Gotenberg's logs. It is `null` if the response did not carry one.

```csharp
try
{
    var merged = await sharpClient.MergePdfsAsync(mergeBuilder);
}
catch (GotenbergApiException ex)
{
    logger.LogError("Gotenberg returned {Status}, trace {Trace}", ex.StatusCode, ex.Trace);
}
```

## Version Compatibility

Some Gotenberg routes only exist in newer releases. Requests that use one declare the release that
introduced it, and the client checks it against the running service before sending — so an
unsupported feature surfaces as an explanatory error instead of an opaque `404`:

```csharp
// Against Gotenberg 8.20:
await sharpClient.ExecutePdfEngineAsync(
    PdfEngineBuilders.WriteBookmarks(b => b.Add("Intro", 1))
        .WithPdfs(a => a.AddItem("doc.pdf", pdfBytes)));

// GotenbergVersionNotSupportedException:
//   Writing PDF bookmarks requires Gotenberg 8.28.0 or newer, but the service at this
//   address reports 8.20.0. Upgrade Gotenberg to use this feature.
```

The version is fetched from `/version` once per service address and cached; the clients created by
`AddGotenbergSharpClient()` share that cache, and a client constructed directly keeps its own. Routes
available in every supported release have no minimum to check. Which Gotenberg API the client renders
requests for, and when the lookup runs, is described in [Gotenberg 9](gotenberg-9.md#how-the-client-picks-the-api).

Currently gated:

| Feature | Requires |
| --- | --- |
| Split (`PdfEngineBuilders.Split`) | 8.15.0 |
| Flatten (`PdfEngineBuilders.Flatten`) | 8.16.0 |
| Encrypt (`PdfEngineBuilders.Encrypt`) | 8.23.1 |
| Embedded files (`PdfEngineBuilders.Embed`) | 8.25.0 |
| Bookmarks (`ReadBookmarks` / `WriteBookmarks`) | 8.28.0 |
| Rotate (`PdfEngineBuilders.Rotate`) | 8.28.0 |

Merge, metadata (8.3.0) and the screenshot routes (8.0.0) are not gated: they predate the
`/version` route (8.5.0), so a gate would turn away the 8.0–8.4 services that serve them.

### Checking ahead of time

To branch on support rather than catch an exception:

```csharp
var request = PdfEngineBuilders.ReadBookmarks()
    .WithPdfs(a => a.AddItem("doc.pdf", pdfBytes))
    .Build();

if (await sharpClient.SupportsAsync(request))
    outlines = await sharpClient.ReadPdfBookmarksAsync(request);
```

`EnsureSupportedAsync(request)` does the same but throws, and `GetGotenbergVersionAsync()` returns
the parsed, cached `GotenbergVersion` directly.

### Services that don't report a version

Gotenberg releases predating the `/version` route report nothing at all. Those releases are older
than every version this client gates on, so an undeterminable version is treated as unsupported.

If your service does support the feature but hides `/version` — behind a proxy, for instance — turn
the check off:

```csharp
sharpClient.EnforceMinimumVersion = false;
```

### Declaring a minimum on your own requests

Custom requests deriving from the built-in request types can declare their own floor with
`[MinimumGotenbergVersion]`:

```csharp
[MinimumGotenbergVersion("8.28.0", Feature = "My custom route")]
public sealed class MyCustomRequest : PdfEngineRequest { /* ... */ }
```

## Examples

See the [examples folder](https://github.com/ChangemakerStudios/GotenbergSharpApiClient/tree/develop/examples) for complete working console applications demonstrating each feature.
