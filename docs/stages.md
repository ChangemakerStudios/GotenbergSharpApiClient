# Stages (Gotenberg 9)

Gotenberg 9 post-processes a PDF with **stages**: a list of operations it runs in the order given. The
client already turns the cross-cutting options (`SetPdfOutputOptions`, `SetRotationOptions`,
`SetSplitOptions`, `SetStampOptions`) into stages for you. List stages yourself with `WithStages` when
you need what those options cannot say:

- a stamp placed, sized, faded or turned (`StampText`, `StampImage`, `StampPdf` and their options);
- an order of your own, such as a stamp after the rotation;
- an operation the options never had: PDF/A-4, PDF/UA-2, permissions, signatures, redaction and the
  other [stages of Gotenberg](https://gotenberg.dev/docs/pdf/pipe).

```csharp
var builder = new HtmlRequestBuilder()
    .AddDocument(d => d.SetBody(html))
    .WithStages(s => s
        .StampText("DRAFT", o => o.SetPosition(StampPosition.Center).SetOpacity(0.3).SetFontSize(48))
        .SetMetadata(m => m.SetTitle("Quarterly report"))
        .Remediate(PdfProfile.A2b, PdfProfile.Ua1));
```

## Where

| Builder | Stages run |
|---|---|
| `HtmlRequestBuilder`, `UrlRequestBuilder`: `WithStages` | after Chromium converts |
| `MergeOfficeBuilder`: `WithStages` | after LibreOffice converts and the merge |
| `MergeBuilder`: `WithStages` | after the merge, which stays the first stage |
| `PdfEngineBuilders.Pipe(s => ...)` | on the uploaded PDFs, alone |

The other PDF operations (`Flatten`, `Rotate`, `Split`, `Encrypt`, `Embed`, `WriteMetadata`,
`WriteBookmarks`, `PdfConversionBuilder`) have no `WithStages`: list the same operation in a `Pipe`.
Screenshots and the read routes take no stages.

```csharp
await client.ExecutePdfEngineAsync(PdfEngineBuilders
    .Pipe(s => s
        .Merge()
        .StampPdf("letterhead.pdf", letterhead, o => o.SetPages("1"))
        .Encrypt("user", "owner", o => o.SetPermissions(PdfPermissions.Print)))
    .WithPdfs(a => a.AddItem("one.pdf", one).AddItem("two.pdf", two)));
```

## Typed methods

| Method | Stage | Options |
|---|---|---|
| `Merge()` | `merge` | |
| `SplitEvery(pages)`, `SplitRanges("1-3", "4")` | `split` | |
| `Select("1-3,5")` | `select` | |
| `Rotate(90, "1-2")` | `rotate` | 90, 180 or 270 (an `int` or a `RotationAngle`); every page without pages |
| `StampText(text, o => ...)` | `stamp-text` | `SetPages`, `SetPosition`, `SetOffset`, `SetOpacity`, `SetRotation`, `SetFontSize`, `SetFont`, `SetColor` |
| `StampImage(fileName, content, o => ...)` | `stamp-image` | the same, then `SetWidth`, `SetHeight` instead of the font |
| `StampPdf(fileName, content, o => ...)` | `stamp-pdf` | the same, then `SetSourcePage`, `SetScale` |
| `FlattenForm()` | `flatten-form` | |
| `SetMetadata(m => ...)` | `set-metadata` | the eight fields; dates as `DateTimeOffset` |
| `SetBookmarks(b => ...)` | `set-bookmarks` | the `BookmarkBuilder` of `WriteBookmarks` |
| `Attach(fileName, content, mimeType, relationship)` | `attach` | |
| `Remediate(PdfProfile.A2b)`, `Remediate(PdfProfile.A4, PdfProfile.Ua2)` | `remediate` | one profile, or a PDF/A and a PDF/UA one |
| `Encrypt(user, owner, o => ...)` | `encrypt` | `SetAlgorithm`, `SetPermissions` |
| `Optimize()` | `optimize` | |

`SetPosition` needs Gotenberg 9.4.0; older 9.x releases answer `400`. An opacity is greater than 0 and
at most 1: Gotenberg reads 0 as "not set", which is opaque.

## Any other operation

`Add(op, options)` sends any operation with the options you give, as an `IDictionary<string, object>`
or a `System.Text.Json.Nodes.JsonObject`, copied when the stage is added; null options are left out.
`AddFile` uploads a file such a stage names:

```csharp
.WithStages(s => s
    .AddFile("cert.p12", certificate)
    .Add("sign", new Dictionary<string, object>
    {
        ["certificate"] = "cert.p12", ["certificatePassword"] = "secret", ["reason"] = "Approved"
    }))
```

Every file a stage reads (stamps, attachments, `AddFile`) is uploaded under `files` with its name. A
name that another file of the request already has, such as an asset or `index.html`, throws an
`ArgumentException`.

## Rules

- **Order.** Stages are sent as listed; the client does not reorder them. Gotenberg answers `400` to an
  order it cannot run, for example anything but `encrypt`, `optimize`, `sign`, `timestamp` and the
  final readers after `remediate`, or a second `remediate`. Its message arrives in
  `GotenbergApiException.Message`. See [Gotenberg's pipe route](https://gotenberg.dev/docs/pdf/pipe).
- **No mixing.** A request with stages must not also set an option the client sends as a stage:
  PDF/A format, PDF/UA, flatten, metadata or passwords of `SetPdfOutputOptions`, nor rotation, split,
  watermark or stamp options. Building the request for sending throws `InvalidOperationException`
  naming the option and its stage method. `SetGenerateTaggedPdf` is a form field and combines.
- **Gotenberg 7 and 8** have no stages: the request throws `GotenbergVersionNotSupportedException`
  ("requires Gotenberg 9.0.0 or newer") before anything is sent, and `SupportsAsync` answers `false`.
