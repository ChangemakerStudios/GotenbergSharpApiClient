# PDF Manipulation

> **Examples:** [PdfEngineOperations](https://github.com/ChangemakerStudios/GotenbergSharpApiClient/tree/develop/examples/PdfEngineOperations), [WatermarkAndRotate](https://github.com/ChangemakerStudios/GotenbergSharpApiClient/tree/develop/examples/WatermarkAndRotate), [EncryptPdf](https://github.com/ChangemakerStudios/GotenbergSharpApiClient/tree/develop/examples/EncryptPdf), [PdfMerge](https://github.com/ChangemakerStudios/GotenbergSharpApiClient/tree/develop/examples/PdfMerge), [PdfConvert](https://github.com/ChangemakerStudios/GotenbergSharpApiClient/tree/develop/examples/PdfConvert)

Standalone operations on existing PDF files using Gotenberg's PDF engine routes.

## Standalone PDF Operations

Use `PdfEngineBuilders` factory methods to create typed builders for each operation.

### Flatten

Merge form fields into static content:

```csharp
using var result = await sharpClient.ExecutePdfEngineAsync(
    PdfEngineBuilders.Flatten()
        .WithPdfs(a => a.AddItem("form.pdf", pdfBytes)));
```

### Rotate

Rotate pages by 90, 180, or 270 degrees:

```csharp
// Rotate all pages
using var result = await sharpClient.ExecutePdfEngineAsync(
    PdfEngineBuilders.Rotate(90)
        .WithPdfs(a => a.AddItem("doc.pdf", pdfBytes)));

// Rotate specific pages
using var result = await sharpClient.ExecutePdfEngineAsync(
    PdfEngineBuilders.Rotate(180, "1-3")
        .WithPdfs(a => a.AddItem("doc.pdf", pdfBytes)));
```

### Split

Split PDFs into chunks or extract page ranges:

```csharp
// Split every 2 pages (returns ZIP)
using var result = await sharpClient.ExecutePdfEngineAsync(
    PdfEngineBuilders.Split(SplitMode.Intervals, "2")
        .WithPdfs(a => a.AddItem("doc.pdf", pdfBytes)));

// Extract pages and unify into one PDF
using var result = await sharpClient.ExecutePdfEngineAsync(
    PdfEngineBuilders.Split(SplitMode.Pages, "1-3,5", unify: true)
        .WithPdfs(a => a.AddItem("doc.pdf", pdfBytes)));
```

### Encrypt

Password-protect PDFs:

```csharp
using var result = await sharpClient.ExecutePdfEngineAsync(
    PdfEngineBuilders.Encrypt(userPassword, ownerPassword)
        .WithPdfs(a => a.AddItem("doc.pdf", pdfBytes)));
```

### Write Metadata

```csharp
using var result = await sharpClient.ExecutePdfEngineAsync(
    PdfEngineBuilders.WriteMetadata(new Dictionary<string, object>
    {
        { "Author", "John Doe" },
        { "Title", "My Document" }
    }).WithPdfs(a => a.AddItem("doc.pdf", pdfBytes)));
```

### Read Metadata

Returns JSON keyed by filename:

```csharp
var json = await sharpClient.ReadPdfMetadataAsync(
    PdfEngineBuilders.ReadMetadata()
        .WithPdfs(a => a.AddItem("doc.pdf", pdfBytes)));

// json: { "doc.pdf": { "Author": "...", "Title": "...", ... } }
```

## Cross-Cutting Options

These options are available on **all** request types (HTML, URL, Office, PDF conversion) via `BuildRequestBase`.

### Watermark

Add a background overlay:

```csharp
var builder = new HtmlRequestBuilder()
    .AddDocument(doc => doc.SetBody(html))
    .SetWatermarkOptions(w => w
        .SetTextWatermark("CONFIDENTIAL")       // text watermark on all pages
        // Or target specific pages:
        // .SetTextWatermark("DRAFT", "1-3")
    );
```

### Stamp

Add a foreground overlay (same API as watermark):

```csharp
builder.SetStampOptions(s => s.SetTextStamp("APPROVED", "1"));
```

### Rotation

Rotate pages inline during conversion:

```csharp
builder.SetRotationOptions(r => r
    .SetAngle(90)           // 90, 180, or 270
    .SetPages("2-3"));      // optional: specific pages
```

### Split

Split output during conversion:

```csharp
builder.SetSplitOptions(s => s.SplitByPages("1-3,5", unify: true));
```

## PDF Format Conversion

Convert existing PDFs to PDF/A or apply transformations:

```csharp
var builder = new PdfConversionBuilder()
    .WithPdfs(b => b.AddItem("document.pdf", File.ReadAllBytes(pdfPath)))
    .SetPdfOutputOptions(o => o
        .SetPdfFormat(PdfFormat.A2b)
        .SetPdfUa()
        .SetFlatten());

var result = await sharpClient.ConvertPdfDocumentsAsync(builder);
```

## PDF Encryption (Inline)

Encrypt PDFs generated from any conversion route:

```csharp
var builder = new HtmlRequestBuilder()
    .AddDocument(doc => doc.SetBody(html))
    .SetPdfOutputOptions(o => o
        .SetEncryption(userPassword: "reader", ownerPassword: "admin"));
```

## Merge PDFs

```csharp
var mergeBuilder = new MergeBuilder()
    .WithAssets(b => b.AddItems(pdfStreams.Select((s, i) =>
        KeyValuePair.Create($"{i}.pdf", s))));

var result = await sharpClient.MergePdfsAsync(mergeBuilder);
```
