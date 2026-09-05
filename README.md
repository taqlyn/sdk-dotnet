# Taqlyn .NET SDK

**Full guide:** [.NET](../../apps/docs/content/server/dotnet.md) on the docs site. NuGet PackageId is not published yet.

Server SDK for creating short links with Ed25519-signed requests. Do not use
this package in mobile or other untrusted clients.

```bash
# Optional override: defaults to https://api.taqlyn.com in production
# export TAQLYN_BASE_URL=https://api.taqlyn.com

export TAQLYN_CLIENT_ID=app_test_...
export TAQLYN_PRIVATE_KEY='-----BEGIN PRIVATE KEY-----...'
dotnet test Taqlyn.Sdk.Tests/Taqlyn.Sdk.Tests.csproj
```

```csharp
// Zero-config: baseUrl is optional, defaults to TAQLYN_BASE_URL env var or "https://api.taqlyn.com"
var client = new Taqlyn.TaqlynClient(
    Environment.GetEnvironmentVariable("TAQLYN_CLIENT_ID")!,
    Environment.GetEnvironmentVariable("TAQLYN_PRIVATE_KEY")!);

var link = await client.CreateShortLinkAsync(new()
{
    DestinationWeb = "https://example.com/offer",
    Mode = "web_only",
});
Console.WriteLine(link.ShortUrl);
```

Demo: [`examples/server/dotnet`](../../examples/server/dotnet).
