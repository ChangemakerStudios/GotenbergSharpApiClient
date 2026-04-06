# Screenshots

!!! example "Working Example"
    [Screenshot](https://github.com/ChangemakerStudios/GotenbergSharpApiClient/tree/develop/examples/Screenshot)

Capture screenshots of HTML content or URLs using Chromium. Returns PNG, JPEG, or WebP images.

## Screenshot from HTML

```csharp
var builder = new ScreenshotHtmlRequestBuilder()
    .AddDocument(doc => doc.SetBody(@"
        <html><body style='background: linear-gradient(135deg, #667eea, #764ba2);'>
            <h1 style='color: white;'>Screenshot Demo</h1>
        </body></html>"))
    .WithScreenshotProperties(p => p
        .SetSize(1280, 720)
        .SetFormat(ScreenshotFormat.Png));

var imageStream = await sharpClient.ScreenshotHtmlAsync(builder);
```

## Screenshot from URL

```csharp
var builder = new ScreenshotUrlRequestBuilder()
    .SetUrl("https://example.com")
    .WithScreenshotProperties(p => p
        .SetSize(1024, 768)
        .SetFormat(ScreenshotFormat.Jpeg)
        .SetQuality(90)
        .SetClip());

var imageStream = await sharpClient.ScreenshotUrlAsync(builder);
```

## Screenshot Properties

| Property | Description | Default |
|----------|-------------|---------|
| `SetSize(width, height)` | Device screen dimensions in pixels | 800x600 |
| `SetFormat(format)` | `Png`, `Jpeg`, or `Webp` | Png |
| `SetQuality(0-100)` | Compression quality (JPEG only) | 100 |
| `SetClip()` | Clip to device dimensions | false |
| `SetOmitBackground()` | Transparent background (PNG/WebP) | false |
| `SetOptimizeForSpeed()` | Faster encoding, larger files | false |

## With Conversion Behaviors

Screenshots share the same Chromium rendering behaviors as PDF conversions:

```csharp
var builder = new ScreenshotHtmlRequestBuilder()
    .AddDocument(doc => doc.SetBody(html))
    .WithScreenshotProperties(p => p
        .SetSize(1920, 1080)
        .SetFormat(ScreenshotFormat.Png))
    .SetConversionBehaviors(b => b
        .SetWaitForSelector("#loaded")
        .AddEmulatedMediaFeature("prefers-color-scheme", "dark")
        .SkipNetworkIdleEvent());
```
