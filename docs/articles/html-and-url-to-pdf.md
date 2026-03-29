# HTML & URL to PDF

## HTML to PDF

Convert HTML content with embedded assets, headers, and footers.

```csharp
var builder = new HtmlRequestBuilder()
    .AddDocument(doc =>
        doc.SetBody(GetBody()).SetFooter(GetFooter()))
    .WithPageProperties(pp =>
    {
        pp.SetPaperSize(PaperSizes.A3)
            .SetMargins(Margins.None)
            .SetScale(.99);
    })
    .WithAsyncAssets(async assets =>
        assets.AddItem("some-image.jpg", await GetImageBytes()));

var result = await sharpClient.HtmlToPdfAsync(builder);
```

## URL to PDF

Convert any URL with custom page ranges, headers, and footers.

```csharp
var builder = new UrlRequestBuilder()
    .SetUrl("https://www.example.com")
    .ConfigureRequest(config => config.SetPageRanges("1-2"))
    .AddAsyncHeaderFooter(async doc =>
        doc.SetHeader(await File.ReadAllTextAsync(headerPath))
            .SetFooter(await File.ReadAllBytesAsync(footerPath)))
    .WithPageProperties(pp =>
    {
        pp.SetPaperSize(PaperSizes.A4)
            .SetMargins(Margins.None)
            .SetScale(.90)
            .SetLandscape();
    });

var result = await sharpClient.UrlToPdfAsync(builder);
```

## Markdown to PDF

```csharp
var builder = new HtmlRequestBuilder()
    .AddAsyncDocument(async doc =>
        doc.SetHeader(await GetHeaderAsync())
            .SetBody(await GetBodyAsync())
            .SetContainsMarkdown()
            .SetFooter(await GetFooterAsync()))
    .WithPageProperties(pp =>
        pp.UseChromeDefaults().SetLandscape().SetScale(.90))
    .WithAsyncAssets(async a =>
        a.AddItems(await GetMarkdownAssets()));

var result = await sharpClient.HtmlToPdfAsync(builder);
```

## Chromium Rendering Behaviors

### Wait for Selector & Media Features

Control Chromium's rendering with wait conditions, media emulation, and error handling.

```csharp
var builder = new HtmlRequestBuilder()
    .AddDocument(doc => doc.SetBody(html))
    .SetConversionBehaviors(b => b
        // Wait for a DOM element before converting
        .SetWaitForSelector("#app")
        // Emulate dark mode
        .AddEmulatedMediaFeature("prefers-color-scheme", "dark")
        // Strict error handling
        .SetFailOnHttpStatusCodes(499, 599)
        .FailOnResourceLoadingFailed()
        .FailOnConsoleExceptions()
        // Ignore specific CDN domains for status checks
        .AddIgnoreResourceHttpStatusDomains("cdn.example.com", "fonts.googleapis.com"));
```

### Cookies

```csharp
builder.SetConversionBehaviors(b => b
    .AddCookie(new Cookie
    {
        Name = "session_token",
        Value = "abc123xyz",
        Domain = "example.com",
        Path = "/",
        Secure = true,
        HttpOnly = true,
        SameSite = "Lax"
    }));
```

### Custom HTTP Headers

```csharp
builder.SetConversionBehaviors(b => b
    .AddAdditionalHeaders("Authorization", "Bearer token123"));
```

## Page Properties

### Paper Sizes & Margins

```csharp
builder.WithPageProperties(pp =>
{
    pp.SetPaperSize(PaperSizes.A4)        // or Letter, A3, etc.
        .SetMargins(Margins.Normal)         // or None, Large
        .SetScale(0.95)
        .SetLandscape()
        .SetPrintBackground(true)
        .SetGenerateDocumentOutline(true);
});
```

### Single Page Output

Force all content onto one very long page:

```csharp
builder.WithPageProperties(pp => pp.UseChromeDefaults().SetSinglePage(true));
```

### Custom Dimensions

```csharp
builder.WithPageProperties(pp =>
{
    pp.SetPaperWidth(Dimension.FromInches(8.5))
        .SetPaperHeight(Dimension.FromInches(14))
        .SetMargins("1in 0.5in");  // top/bottom, left/right
});
```
