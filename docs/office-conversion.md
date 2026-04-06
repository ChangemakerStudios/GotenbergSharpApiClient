# Office Document Conversion

!!! example "Working Examples"
    [OfficeMerge](https://github.com/ChangemakerStudios/GotenbergSharpApiClient/tree/develop/examples/OfficeMerge) |
    [LibreOfficeOptions](https://github.com/ChangemakerStudios/GotenbergSharpApiClient/tree/develop/examples/LibreOfficeOptions)

Convert and merge Office documents (Word, Excel, PowerPoint, and 100+ formats) to PDF using LibreOffice.

## Basic Merge

```csharp
var builder = new MergeOfficeBuilder()
    .WithAsyncAssets(async a => a.AddItems(await GetDocsAsync(sourceDirectory)))
    .SetPdfOutputOptions(o => o.SetPdfFormat(PdfFormat.A2b));

var result = await sharpClient.MergeOfficeDocsAsync(builder);
```

## LibreOffice Options

Fine-tune conversions with LibreOffice-specific options:

```csharp
var builder = new MergeOfficeBuilder()
    .WithAsyncAssets(async a => a.AddItems(await GetDocsAsync(sourceDirectory)))
    .SetLibreOfficeOptions(o => o
        // Image compression
        .SetQuality(85)                     // JPEG quality 1-100
        .SetReduceImageResolution()
        .SetMaxImageResolution(300)         // DPI: 75, 150, 300, 600, 1200
        // Export options
        .SetExportBookmarks()
        .SetExportFormFields(false)         // Flatten form fields
        .SetUpdateIndexes()
        // Native watermark
        .SetNativeWatermarkText("DRAFT")
        .SetNativeWatermarkFontName("Arial")
        .SetNativeWatermarkFontHeight(48))
    .SetPdfOutputOptions(o => o.SetPdfFormat(PdfFormat.A2b));
```

## All LibreOffice Options

### Layout

| Method | Description | Default |
|--------|-------------|---------|
| `SetSinglePageSheets()` | One page per sheet | false |
| `SetSkipEmptyPages()` | Skip empty pages (Writer only) | false |
| `SetExportPlaceholders()` | Render placeholders as visual markers | false |

### Image Compression

| Method | Description | Default |
|--------|-------------|---------|
| `SetLosslessImageCompression()` | Use PNG instead of JPEG | false |
| `SetQuality(1-100)` | JPEG quality | 90 |
| `SetReduceImageResolution()` | Reduce to max DPI | false |
| `SetMaxImageResolution(dpi)` | Max DPI (75/150/300/600/1200) | 300 |

### Notes & Slides

| Method | Description | Default |
|--------|-------------|---------|
| `SetExportNotes()` | Export notes to PDF | false |
| `SetExportNotesPages()` | Export notes pages (Impress) | false |
| `SetExportOnlyNotesPages()` | Export only notes pages | false |
| `SetExportNotesInMargin()` | Notes in margin | false |
| `SetExportHiddenSlides()` | Include hidden slides | false |

### Links & Outline

| Method | Description | Default |
|--------|-------------|---------|
| `SetConvertOooTargetToPdfTarget()` | Convert .od* links to .pdf | false |
| `SetExportLinksRelativeFsys()` | Relative file:// links | false |
| `SetUpdateIndexes()` | Update TOC before conversion | true |
| `SetExportBookmarks()` | Export bookmarks | true |
| `SetExportBookmarksToPdfDestination()` | Bookmarks as Named Destinations | false |
| `SetAddOriginalDocumentAsStream()` | Embed original document | false |

### Form Fields

| Method | Description | Default |
|--------|-------------|---------|
| `SetExportFormFields(bool)` | Export as interactive widgets | true |
| `SetAllowDuplicateFieldNames()` | Allow duplicate field names | false |

### Native Watermark

| Method | Description |
|--------|-------------|
| `SetNativeWatermarkText(text)` | Single-line text watermark |
| `SetNativeTiledWatermarkText(text)` | Repeating tiled watermark |
| `SetNativeWatermarkColor(rgbDecimal)` | RGB color as decimal (default: 8388223) |
| `SetNativeWatermarkFontName(font)` | Font name (default: Helvetica) |
| `SetNativeWatermarkFontHeight(pts)` | Font size in points (0 = auto) |
| `SetNativeWatermarkRotateAngle(angle)` | Rotation in tenths of degrees |

### Source File Password

```csharp
builder.SetLibreOfficeOptions(o => o.SetPassword("document-password"));
```
