# Gotenberg Feature Gap Analysis

> **Purpose**: This document is a guide for continuing work on adding missing Gotenberg features to the GotenbergSharpApiClient. It reflects the state of the Gotenberg API as of March 2026 vs what the client supports after the PdfOutputOptions refactoring.

## Architecture Context

The client was recently refactored to unify PDF output options. Key patterns to follow:

- **Facet classes** extend `FacetBase` and use `[MultiFormHeader("fieldName")]` attributes on properties. The reflection-based `ToHttpContent()` in `FacetBase` handles serialization automatically. This is the preferred way to add new form fields.
- **Facet builders** are fluent builder classes that wrap a facet instance (e.g., `PdfOutputOptionsBuilder` wraps `PdfOutputOptions`).
- **`BuildRequestBase`** is the root of all requests. It holds shared concerns (`Config`, `Assets`, `PdfOutputOptions`). New cross-cutting facets should be added here.
- **`BaseBuilder<TRequest, TBuilder>`** is the root of all builders. Methods added here are available on every builder type.
- **`Constants.Gotenberg`** holds all form field name strings. Add new shared field names to `CrossCutting` (private) and expose via public nested classes.
- **`FacetBase.GetValueAsInvariantCultureString()`** handles type-to-string conversion for form data. If you add a new enum or complex type, add a case there.

### Request Hierarchy
```
BuildRequestBase (has Config, Assets, PdfOutputOptions)
  ChromeRequest (has PageProperties, HtmlConversionBehaviors)
    HtmlRequest
    UrlRequest
  PdfRequestBase (thin wrapper, delegates to base)
    MergeRequest
    MergeOfficeRequest
    PdfConversionRequest
```

### Builder Hierarchy
```
BaseBuilder<TRequest, TBuilder> (has ConfigureRequest, SetPdfOutputOptions)
  BaseChromiumBuilder (has WithPageProperties, SetConversionBehaviors, WithAssets)
    HtmlRequestBuilder
    UrlRequestBuilder
  BaseMergeBuilder (has WithAssets)
    MergeBuilder
    MergeOfficeBuilder
  PdfConversionBuilder
```

### How Form Fields Get Sent
1. Properties decorated with `[MultiFormHeader("fieldName")]` on `FacetBase` subclasses are auto-serialized via reflection.
2. `IConvertToHttpContent.ToHttpContent()` returns `IEnumerable<HttpContent>` which gets assembled into `MultipartFormDataContent`.
3. Headers (like `Gotenberg-Trace`, webhook headers) go through `RequestConfig.GetHeaders()` and are set on the `HttpRequestMessage` directly, not as form data.
4. The `Gotenberg-Output-Filename` header is currently sent as a form field in `RequestConfig.ToHttpContent()` -- Gotenberg accepts it both ways.

---

## Missing Features by Category

### 1. Screenshot Routes (NEW - Entirely new capability)

**API Routes:**
- `POST /forms/chromium/screenshot/url`
- `POST /forms/chromium/screenshot/html`
- `POST /forms/chromium/screenshot/markdown`

**New form fields (screenshot-specific):**
| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `width` | number | 800 | Device screen width in pixels |
| `height` | number | 600 | Device screen height in pixels |
| `clip` | boolean | false | Clip screenshot to device dimensions |
| `format` | enum | png | Image format: `png`, `jpeg`, `webp` |
| `quality` | number | 100 | Compression quality (0-100), JPEG only |
| `optimizeForSpeed` | boolean | false | Optimize encoding for speed over size |

**Shares with Chromium PDF routes:** `omitBackground`, `waitDelay`, `waitForExpression`, `waitForSelector`, `cookies`, `extraHttpHeaders`, `userAgent`, `emulatedMediaFeatures`, `failOnHttpStatusCodes`, `failOnResourceHttpStatusCodes`, `ignoreResourceHttpStatusDomains`, `skipNetworkIdleEvent`, `failOnResourceLoadingFailed`, `failOnConsoleExceptions`.

**Does NOT use:** Page properties (paperWidth, margins, etc.), PDF output options, header/footer files.

**Implementation approach:**
- Create `ScreenshotOptions` facet with the screenshot-specific fields.
- Create a `ScreenshotRequest` base class (similar to `ChromeRequest` but with `ScreenshotOptions` + `HtmlConversionBehaviors`, no `PageProperties`).
- Create `ScreenshotUrlRequest`, `ScreenshotHtmlRequest`, `ScreenshotMarkdownRequest`.
- Create corresponding builders.
- Add screenshot API paths to `Constants.Gotenberg.Chromium.ApiPaths`.
- Add methods to `GotenbergSharpClient`: `ScreenshotUrlAsync()`, `ScreenshotHtmlAsync()`, `ScreenshotMarkdownAsync()`.
- **Important**: The client constructor currently hardcodes `Accept: application/pdf`. Screenshot responses are images. Either don't set a default Accept header, or set it per-request. The `SendRequestAsync` method may need to handle non-PDF content types.

### 2. Missing Chromium Form Fields (add to existing facets)

These apply to all Chromium routes (URL/HTML/Markdown to PDF AND screenshots):

| Field | Add To | Type | Default | Description |
|-------|--------|------|---------|-------------|
| `waitForSelector` | `HtmlConversionBehaviors` | string | None | CSS selector; delays until element appears in DOM |
| `emulatedMediaFeatures` | `HtmlConversionBehaviors` | json | None | JSON array to override CSS media features |
| `failOnHttpStatusCodes` | `HtmlConversionBehaviors` | json | [499,599] | HTTP status code ranges that trigger 409 |
| `failOnResourceHttpStatusCodes` | `HtmlConversionBehaviors` | json | None | Asset status codes that trigger failure |
| `ignoreResourceHttpStatusDomains` | `HtmlConversionBehaviors` | json | None | Domains excluded from status checks |
| `failOnResourceLoadingFailed` | `HtmlConversionBehaviors` | bool | false | Fail if any resource fails to load |

These are simple -- just add `[MultiFormHeader("fieldName")]` properties to `HtmlConversionBehaviors` and corresponding methods to `HtmlConversionBehaviorBuilder`.

### 3. Standalone PDF Manipulation Routes (NEW route category)

All under `/forms/pdfengines/...`. These are **independent operations**, not inline options on conversion routes.

| Route | API Path | Description |
|-------|----------|-------------|
| Write Metadata | `POST /forms/pdfengines/metadata/write` | Write metadata to existing PDFs |
| Read Metadata | `POST /forms/pdfengines/metadata/read` | Read metadata from PDFs (returns JSON) |
| Read Bookmarks | `POST /forms/pdfengines/bookmarks/read` | Read bookmarks (returns JSON) |
| Write Bookmarks | `POST /forms/pdfengines/bookmarks/write` | Write bookmarks to PDFs |
| Attachments | `POST /forms/pdfengines/attachments` | Embed files into PDFs |
| Flatten | `POST /forms/pdfengines/flatten` | Flatten PDFs standalone |
| Watermark | `POST /forms/pdfengines/watermark` | Add watermarks to PDFs |
| Stamp | `POST /forms/pdfengines/stamp` | Add stamps (foreground) to PDFs |
| Rotate | `POST /forms/pdfengines/rotate` | Rotate PDF pages |
| Split | `POST /forms/pdfengines/split` | Split PDFs into parts |
| PDF/A & PDF/UA | `POST /forms/pdfengines/convert` | Already supported |
| Encrypt | `POST /forms/pdfengines/encrypt` | Password-protect PDFs |

**Implementation approach:** Each standalone route needs its own request class and builder. Some return JSON (read metadata, read bookmarks) rather than PDF streams -- the client will need methods that return parsed objects, not just `Stream`.

### 4. Watermark & Stamp Fields (cross-cutting, apply to many routes)

These apply as inline options on Chromium, LibreOffice, AND PDF engine routes:

**Watermark fields:**
| Field | Type | Description |
|-------|------|-------------|
| `watermarkSource` | enum | `text`, `image`, or `pdf` |
| `watermarkExpression` | string | Text content or uploaded filename |
| `watermarkPages` | string | Page ranges (e.g., `1-3`, `5`) |
| `watermarkOptions` | json | Advanced: font, color, rotation, opacity |
| watermark file | file | Image or PDF file for watermark |

**Stamp fields:** Same structure as watermark with `stamp` prefix.

**Implementation approach:** Create a `WatermarkOptions` facet and `StampOptions` facet. Add to `BuildRequestBase` alongside `PdfOutputOptions` since they're cross-cutting.

### 5. Rotation Fields (cross-cutting)

| Field | Type | Description |
|-------|------|-------------|
| `rotateAngle` | enum | `90`, `180`, or `270` |
| `rotatePages` | string | Page ranges to rotate |

Add as a `RotationOptions` facet on `BuildRequestBase`.

### 6. Split Fields (cross-cutting)

| Field | Type | Description |
|-------|------|-------------|
| `splitMode` | enum | `intervals` or `pages` |
| `splitSpan` | string | The rule for splitting |
| `splitUnify` | boolean | Merge extracted pages into single file |

Add as a `SplitOptions` facet on `BuildRequestBase`. **Note**: When splitting returns multiple files, Gotenberg returns a ZIP. The client will need to handle this.

### 7. Encryption Fields (cross-cutting)

| Field | Type | Description |
|-------|------|-------------|
| `userPassword` | string | Password required to open the PDF |
| `ownerPassword` | string | Password for changing permissions |

Add to `PdfOutputOptions` (they're PDF output concerns).

### 8. Embed Files

| Field | Type | Description |
|-------|------|-------------|
| `embeds` | file[] | Files to embed/attach inside the PDF |

This is a file upload field, not a simple string. Needs special handling similar to how `Assets` works.

### 9. Output Filename Header

| Header | Description |
|--------|-------------|
| `Gotenberg-Output-Filename` | Control the output filename (auto-appends extension) |

Already partially supported via `RequestConfig.ResultFileName` which sends it as a form field. Gotenberg accepts it both ways. May want to verify this works correctly or switch to sending it as an HTTP header.

### 10. Missing LibreOffice-Specific Fields

These only apply to the LibreOffice route (`/forms/libreoffice/convert`):

**Layout:**
| Field | Type | Default |
|-------|------|---------|
| `singlePageSheets` | bool | false |
| `skipEmptyPages` | bool | false |
| `exportPlaceholders` | bool | false |

**Image compression:**
| Field | Type | Default |
|-------|------|---------|
| `losslessImageCompression` | bool | false |
| `quality` | int | 90 |
| `reduceImageResolution` | bool | false |
| `maxImageResolution` | int | 300 |

**Notes/slides (Impress/Writer):**
| Field | Type | Default |
|-------|------|---------|
| `exportNotes` | bool | false |
| `exportNotesPages` | bool | false |
| `exportOnlyNotesPages` | bool | false |
| `exportNotesInMargin` | bool | false |
| `exportHiddenSlides` | bool | false |

**Links:**
| Field | Type | Default |
|-------|------|---------|
| `convertOooTargetToPdfTarget` | bool | false |
| `exportLinksRelativeFsys` | bool | false |

**Document outline:**
| Field | Type | Default |
|-------|------|---------|
| `updateIndexes` | bool | true |
| `exportBookmarks` | bool | true |
| `exportBookmarksToPdfDestination` | bool | false |
| `addOriginalDocumentAsStream` | bool | false |

**Form fields:**
| Field | Type | Default |
|-------|------|---------|
| `exportFormFields` | bool | true |
| `allowDuplicateFieldNames` | bool | false |

**Native watermark (LibreOffice-specific, different from the cross-cutting watermark):**
| Field | Type | Default |
|-------|------|---------|
| `nativeWatermarkText` | string | None |
| `nativeWatermarkColor` | int | 8388223 |
| `nativeWatermarkFontHeight` | int | 0 (auto) |
| `nativeWatermarkRotateAngle` | int | 0 |
| `nativeWatermarkFontName` | string | Helvetica |
| `nativeTiledWatermarkText` | string | None |

**Source file password:**
| Field | Type | Description |
|-------|------|-------------|
| `password` | string | Password to open password-protected source files |

**Implementation approach:** Create a `LibreOfficeOptions` facet using `FacetBase`/attributes. Add it to `MergeOfficeRequest` (not to `BuildRequestBase` -- these are LibreOffice-specific). Create a `LibreOfficeOptionsBuilder` and expose it on `MergeOfficeBuilder`.

---

## Client-Level Changes Needed

### `GotenbergSharpClient` Response Handling

The client currently:
1. Hardcodes `Accept: application/pdf` in the constructor.
2. Always returns `Stream` (via `MemoryStream`).

For new features, it needs to handle:
- **Screenshots**: Returns PNG/JPEG/WebP image streams. Remove or make the Accept header conditional.
- **Read Metadata / Read Bookmarks**: Returns JSON. Need methods that return deserialized objects.
- **Split PDFs**: Can return a ZIP when multiple files result. Consider returning the raw stream and letting callers handle it, or providing a helper.

### Suggested Method Additions on `GotenbergSharpClient`

```csharp
// Screenshots
Task<Stream> ScreenshotUrlAsync(ScreenshotUrlRequest request, ...)
Task<Stream> ScreenshotHtmlAsync(ScreenshotHtmlRequest request, ...)

// Standalone PDF operations
Task<Stream> WatermarkPdfsAsync(WatermarkPdfRequest request, ...)
Task<Stream> StampPdfsAsync(StampPdfRequest request, ...)
Task<Stream> RotatePdfsAsync(RotatePdfRequest request, ...)
Task<Stream> SplitPdfsAsync(SplitPdfRequest request, ...)
Task<Stream> EncryptPdfsAsync(EncryptPdfRequest request, ...)
Task<Stream> FlattenPdfsAsync(FlattenPdfRequest request, ...)
Task<Stream> WriteMetadataAsync(WriteMetadataPdfRequest request, ...)
Task<JObject> ReadMetadataAsync(ReadMetadataPdfRequest request, ...)
Task<Stream> WriteBookmarksAsync(WriteBookmarksPdfRequest request, ...)
Task<JArray> ReadBookmarksAsync(ReadBookmarksPdfRequest request, ...)
Task<Stream> AttachFilesAsync(AttachFilesPdfRequest request, ...)
```

---

## Suggested Implementation Order

1. **Chromium missing fields** (waitForSelector, failOn*, emulatedMediaFeatures) -- Easiest. Just add properties to existing facets.
2. **Encryption fields** (userPassword, ownerPassword) -- Add to `PdfOutputOptions`.
3. **LibreOffice options** -- New facet, lots of simple fields.
4. **Cross-cutting options** (watermark, stamp, rotation, split) -- New facets on `BuildRequestBase`.
5. **Screenshot routes** -- New request/builder hierarchy + client response handling changes.
6. **Standalone PDF manipulation routes** -- New request classes for each route + JSON response handling.

---

## Reference URLs

- Gotenberg docs home: https://gotenberg.dev/docs/getting-started/introduction
- Chromium URL to PDF: https://gotenberg.dev/docs/convert-with-chromium/convert-url-to-pdf
- Chromium screenshots: https://gotenberg.dev/docs/convert-with-chromium/screenshot-url
- LibreOffice: https://gotenberg.dev/docs/convert-with-libreoffice/convert-to-pdf
- PDF manipulation: https://gotenberg.dev/docs/manipulate-pdfs/write-metadata (sidebar has all routes)
- Webhook & Download: https://gotenberg.dev/docs/webhook-download
