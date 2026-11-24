# Getting Started

!!! example "Working Example"
    [DIExample](https://github.com/ChangemakerStudios/GotenbergSharpApiClient/tree/develop/examples/DIExample) — Full DI setup with logging and a retry

## Prerequisites

- [Docker](https://www.docker.com/) for running Gotenberg 7, 8 or 9. The client reads the version of the service and sends each request in the form that version takes; see [Gotenberg 9](gotenberg-9.md)
- [.NET 8.0+](https://dotnet.microsoft.com/), or any runtime that supports .NET Standard 2.0 or 2.1 (the package also targets .NET 9 and 10)

## Running Gotenberg

### Docker Run

```bash
docker pull gotenberg/gotenberg:9
docker run --name gotenberg --rm -p 3000:3000 gotenberg/gotenberg:9 gotenberg --api-timeout=1800s --log-level=debug
```

### Docker Compose (with Basic Auth)

```bash
docker-compose -f docker/docker-compose-basic-auth.yml up -d
```

Pre-configured with test credentials: `testuser` / `testpass`.

## Install the NuGet Package

```bash
dotnet add package Gotenberg.Sharp.Api.Client
```

## Configure Services

### Basic (appsettings.json)

```json
{
  "GotenbergSharpClient": {
    "ServiceUrl": "http://localhost:3000",
    "HealthCheckUrl": "http://localhost:3000/health"
  }
}
```

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddOptions<GotenbergSharpClientOptions>()
        .Bind(Configuration.GetSection("GotenbergSharpClient"));
    services.AddGotenbergSharpClient();
}
```

### With Basic Authentication

```json
{
  "GotenbergSharpClient": {
    "ServiceUrl": "http://localhost:3000",
    "BasicAuthUsername": "your-username",
    "BasicAuthPassword": "your-password"
  }
}
```

### Programmatic Configuration

```csharp
services.AddOptions<GotenbergSharpClientOptions>()
    .Configure(options =>
    {
        options.ServiceUrl = new Uri("http://localhost:3000");
        options.TimeOut = TimeSpan.FromMinutes(5);
        options.BasicAuthUsername = "username";
        options.BasicAuthPassword = "password";
    });
services.AddGotenbergSharpClient();
```

### Hybrid (appsettings + overrides)

```csharp
services.AddOptions<GotenbergSharpClientOptions>()
    .Bind(Configuration.GetSection("GotenbergSharpClient"))
    .PostConfigure(options =>
    {
        options.BasicAuthUsername = Environment.GetEnvironmentVariable("GOTENBERG_USER");
        options.BasicAuthPassword = Environment.GetEnvironmentVariable("GOTENBERG_PASS");
    });
services.AddGotenbergSharpClient();
```

### Retries

The client sends each request once. To retry failed requests, add a resilience handler to the builder that `AddGotenbergSharpClient()` returns; [Retries and Timeouts](resilience.md) shows how, and which failures are worth retrying.

### Options Validation

Settings that could never work throw an `OptionsValidationException` when the options are first read:

- `ServiceUrl` must be an absolute `http` or `https` URL
- `TimeOut` must be positive, or `Timeout.InfiniteTimeSpan`
- `RetryPolicy.Enabled` must not be `true`: the client has no built-in retry since 4.0 (see [Retries and Timeouts](resilience.md))

Add `ValidateOnStart()` to fail at application start-up instead of on the first conversion:

```csharp
services.AddOptions<GotenbergSharpClientOptions>()
    .Bind(Configuration.GetSection("GotenbergSharpClient"))
    .ValidateOnStart();
services.AddGotenbergSharpClient();
```

## Required Using Statements

```csharp
using Gotenberg.Sharp.API.Client;
using Gotenberg.Sharp.API.Client.Application.Builders;
using Gotenberg.Sharp.API.Client.Domain.Settings;
using Gotenberg.Sharp.API.Client.Extensions;
```

## Your First PDF

```csharp
public async Task<Stream> CreatePdf([FromServices] GotenbergSharpClient sharpClient)
{
    var builder = new HtmlRequestBuilder()
        .AddDocument(doc => doc.SetBody("<html><body><h1>Hello PDF!</h1></body></html>"))
        .WithPageProperties(pp => pp.UseChromeDefaults());

    return await sharpClient.HtmlToPdfAsync(builder);
}
```
