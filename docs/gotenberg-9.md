# Gotenberg 9

One package talks to Gotenberg 7, 8 and 9. Gotenberg 9 changed its HTTP API (new routes, renamed form
fields, post-processing as [stages](stages.md)), so the client reads the version of the service and
sends each request in the form that API takes. Most requests need no change in your code. Check
[webhooks](#what-differs-on-gotenberg-9), watermarks and metadata keys first.

Run it with a pinned major tag: `docker run --rm -p 3000:3000 gotenberg/gotenberg:9`. The `latest` tag
moves to 9 with its release.

## How the client picks the API

`GotenbergSharpClientOptions.ApiVersion` decides which Gotenberg API requests are rendered for:

| Value | Meaning |
|---|---|
| `Auto` (default) | Read the version of the service; major 9 or newer gets the Gotenberg 9 API, anything older the Gotenberg 7 and 8 API |
| `V8` | Gotenberg 7 and 8 API, without reading the version |
| `V9` | Gotenberg 9 API, without reading the version |

```json
{
  "GotenbergSharpClient": {
    "ServiceUrl": "http://localhost:3000",
    "ApiVersion": "Auto"
  }
}
```

- **One lookup.** With `Auto`, the first request of any kind sends `GET /version`. The answer is cached per
  service address and shared by the clients that `AddGotenbergSharpClient()` creates; a client you construct
  yourself keeps its own cache.
- **When `/version` cannot be read.** A `404`, or an answer that is not a version, counts as Gotenberg 7 or 8:
  the client uses that API and `GetGotenbergVersionAsync()` returns `GotenbergVersion.Unknown`. A `401`, `403`,
  `5xx` or other error answer is thrown as a `GotenbergApiException` that says what Gotenberg answered to
  `GET /version` and to set `ApiVersion` (or, with `ApiVersion` already set, `EnforceMinimumVersion = false`);
  a `401` also points at the credentials. It keeps the status code, has the original exception as
  `InnerException`, and is not cached, so the next request tries again. A refused connection or a timeout is
  thrown as it is.
- **Skipping the lookup.** Set `ApiVersion` to `V8` or `V9`, for instance behind a proxy that hides `/version`.
  `V9` skips the version check of the routes with a minimum release (split, flatten, encrypt, rotate, embed,
  bookmarks) too, as they all predate Gotenberg 9. With `V8` those requests still read the version; set
  `EnforceMinimumVersion = false` as well to avoid it (see
  [Version Compatibility](advanced-features.md#version-compatibility)).
- **Refresh.** Every response, an error response too, carries a `Gotenberg-Version` header. When its version
  differs from the cached one (an unknown cached version always does), the cache takes the new one, so a
  service you upgrade or roll back while your application runs, to another minor release as well as another
  major, is rendered for and checked against its minimum releases correctly from the next request on.
  With `V8` or `V9` set, the rendering never changes.
- `GetApiVersionAsync()` returns the API in use, `GetGotenbergVersionAsync()` the version of the service.

## Same code, both majors

You write the same builders; the client translates:

| You write | Gotenberg 7 and 8 receive | Gotenberg 9 receives |
|---|---|---|
| Page ranges (`SetPageRanges`), outline (`SetGenerateDocumentOutline`, `SetExportBookmarks`), tagged PDF, LibreOffice quality and image options, single-page sheets, native watermark font, size, colour and rotation | `nativePageRanges`, `generateDocumentOutline`, `generateTaggedPdf`, `quality`, ... | `nativePages`, `nativeBookmarks`, `nativeTaggedPdf`, `nativeQuality`, ..., colour as `#rrggbb`, rotation in degrees |
| `SetPdfOutputOptions` (PDF/A, PDF/UA, flatten, metadata, passwords), `SetRotationOptions`, `SetSplitOptions`, `SetStampOptions`, LibreOffice `merge` | flat form fields | one `stages` field, in the order Gotenberg 8 ran them |
| `PdfEngineBuilders` operations, PDF merge and PDF conversion | one `forms/pdfengines/...` route each | the `forms/pdf/pipe` route, with the operation as a stage; reading metadata and bookmarks use `forms/pdf/metadata` and `forms/pdf/bookmarks` |
| `WriteBookmarks` | a `bookmarks` JSON field with `page` | a `set-bookmarks` stage with `pageNumber` |
| `PdfEngineBuilders.Embed` | `embeds` and `embedsMetadata` fields | one `attach` stage per file, the files under `files` |
| `SetImageStamp`, `SetPdfStamp` with a file | the file in the `stamp` field | a `stamp-image` or `stamp-pdf` stage, the file under `files` |

`ReadPdfBookmarksAsync` reads the outline of either major. See [Stages](stages.md) to list the stages
yourself instead.

## What differs on Gotenberg 9

Each row is something that works on 7 and 8 and behaves differently, or is refused, on 9.

| What you used | What happens on Gotenberg 9 | What to do |
|---|---|---|
| A webhook with `SetErrorUrl` only | Refused: Gotenberg 9 requires the events URL and has no error URL | Add `SetEventsUrl`; set both URLs to run on every release (see [Events URL and Gotenberg 9](advanced-features.md#events-url-and-gotenberg-9)) |
| `SetWatermarkOptions` | Refused: Gotenberg 9 only draws over the page | Stamp instead, through `WithStages` |
| `SetStampOptions` with free-form `SetOptions` | Refused: Gotenberg 9 has typed stamp options instead | `WithStages(s => s.StampText(...))` with its options builder (position, offset, size, font, colour, opacity, rotation) |
| `SetStampOptions` with `SetSource` (image or PDF) and an expression naming a file on the server | Refused: Gotenberg 9 has no way to name a file on the server, it reads every file from the upload; `SupportsAsync` answers false | Set the stamp with `SetImageStamp` or `SetPdfStamp` and its file, or use `StampImage` or `StampPdf` in `WithStages` |
| `SetStampOptions` for a text, image or PDF stamp | Sent as a stage, but drawn at the default place and size of Gotenberg 9, not where Gotenberg 8 drew it | To place or size it, use `StampText`, `StampImage` or `StampPdf` in `WithStages` |
| `SetMetadata` or `WriteMetadata` with other keys than Title, Author, Subject, Keywords, Creator, Producer, CreationDate (CreateDate) and ModDate (ModifyDate) | Refused: Gotenberg 9 writes these eight fields. A date it cannot read throws `ArgumentException` | Remove the other keys |
| `ReadPdfMetadataAsync` | Returns the eight fields in camelCase (`title`, `author`, ..., `modDate`), those the document has, instead of ExifTool tags | Read the new field names; the JSON is not translated |
| `WriteBookmarks` with a set keyed by file name (`WriteBookmarksPerFile`) | Refused: Gotenberg 9 takes one outline for all PDFs of a request | Send one request per file |
| `ReadPdfBookmarksJsonAsync` | Returns the JSON as Gotenberg wrote it: `pageNumber` where Gotenberg 8 wrote `page` | Read `pageNumber`, or use `ReadPdfBookmarksAsync`, which reads both |
| PDF/A together with a password | Refused: PDF/A forbids encryption, and Gotenberg 9 will not encrypt after a PDF/A conversion | Drop one of them. PDF/UA with a password works |
| `SetFlatten` on an HTML or URL request | Applied. The Chromium routes of Gotenberg 8 did not read `flatten` | Remove it if you relied on it doing nothing |
| `SetPdfOutputOptions`, `SetRotationOptions`, `SetSplitOptions` or `SetStampOptions` on a flatten, rotate, encrypt, embed, metadata, bookmark or conversion request | Applied. Gotenberg 8 read them on merge and split only (a conversion read the PDF/A and PDF/UA options) | Remove the options you never meant to apply |
| `SetPageRanges` of `ConfigureRequest` on a PDF merge, flatten or conversion | Not sent: the Gotenberg 9 PDF routes take no page ranges | Add a `Select` stage with `WithStages` or `PdfEngineBuilders.Pipe` |
| `PdfConversionBuilder` with no PDF format, no PDF/UA and no other post-processing option | Throws `InvalidOperationException` before sending (Gotenberg 8 answered `400`) | Set `SetPdfFormat` or `SetPdfUa` |
| `SetNativeWatermarkColor(0)` or `SetNativeWatermarkRotateAngle(0)` | Sent as `#000000` and `0` degrees. Gotenberg 8 read `0` as "not set" | Leave the option unset for the default |
| The names of result files | LibreOffice names its PDFs after the document, `report.docx.pdf`, even for one; a merged PDF takes the name of its first file in alphanumeric order (Gotenberg 8 chose a random name) | `SetResultFileName` still sets the name (see [Gotenberg's upgrade guide](https://gotenberg.dev/docs/getting-started/upgrade#output-names)) |
| A request or facet class of yours that overrides only `ToHttpContent()` | Throws `InvalidOperationException` naming the class; it still works on 7 and 8 | Override `ToHttpContent(GotenbergApiVersion api)` and return your parts together with `base.ToHttpContent(api)` |
| `WithStages` and `PdfEngineBuilders.Pipe` | Gotenberg 9 only; on 7 and 8 they throw `GotenbergVersionNotSupportedException`. A stamp position (`SetPosition`) needs Gotenberg 9.4.0 | See [Stages](stages.md) |

## Which exception to expect

A feature that Gotenberg 9 removed throws `GotenbergVersionNotSupportedException` before anything is sent.
`Feature` names it, `RemovedInVersion` is `9.0.0` and `Replacement` says what to use. A feature that needs a
newer release than the service runs throws the same exception with `Requirement` and `RequiredVersion` set,
and `RemovedInVersion` null. The other refusals in the table are `InvalidOperationException` and
`ArgumentException`, named in the rows.

To find out in advance, ask the client:

```csharp
var request = new HtmlRequestBuilder()
    .AddDocument(d => d.SetBody(html))
    .SetWatermarkOptions(w => w.SetTextWatermark("DRAFT"))
    .Build();

if (!await sharpClient.SupportsAsync(request))
{
    // Gotenberg 9: stamp instead
}
```

`EnsureSupportedAsync(request)` throws the exception instead of answering `false`.
