# Taqlyn .NET SDK

Server SDK for creating short links with Ed25519-signed requests. Do not use
this package in mobile or other untrusted clients.

```bash
export TAQLYN_BASE_URL=https://api.rutvik.qzz.io
export TAQLYN_CLIENT_ID=app_test_...
export TAQLYN_PRIVATE_KEY='-----BEGIN PRIVATE KEY-----...'
dotnet test Taqlyn.Sdk.Tests/Taqlyn.Sdk.Tests.csproj
```

```csharp
var client = new Taqlyn.TaqlynClient(
    Environment.GetEnvironmentVariable("TAQLYN_BASE_URL")!,
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
