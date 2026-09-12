# Taqlyn .NET SDK

**Full guide:** [.NET](../../apps/docs/content/server/dotnet.md) on the docs site. PackageId: [`Taqlyn.Sdk`](https://www.nuget.org/packages/Taqlyn.Sdk).

Server SDK for creating short links with Ed25519-signed requests. Do not use
this package in mobile or other untrusted clients.

## NuGet verified badge (`Taqlyn.*`)

Reserve the ID prefix so nuget.org / Visual Studio show the verified checkmark.

1. Publish at least one `Taqlyn.*` package from the nuget.org org **`taqlyn`**.
2. Email **account@nuget.org** with subject `ID prefix reservation request — Taqlyn.*`:

```
Hello NuGet.org team,

I would like to request an exclusive ID prefix reservation for:

  Prefix: Taqlyn.*
  nuget.org owner display name: taqlyn

Justification:
- Taqlyn is our product/company brand (https://taqlyn.com). The prefix clearly identifies us and is not a common/generic word.
- We publish the official .NET server SDK as Taqlyn.Sdk:
  https://www.nuget.org/packages/Taqlyn.Sdk
- Package metadata uses Authors/Company "Taqlyn", MIT license, and project URL https://taqlyn.com.
- Reserving Taqlyn.* prevents impersonation and gives consumers the verified badge for packages we own.

Reservation type: exclusive (block uploads from non-owners).

Please let me know if you need any additional verification.

Thank you,
<Your Name>
<Your Email>
https://taqlyn.com
```

See [ID Prefix Reservation](https://learn.microsoft.com/en-us/nuget/nuget-org/id-prefix-reservation) and [Trusted publishing](../../docs/guides/sdk-trusted-publishing.md#38-net-c-sdk-taqlynsdk-nugetorg--100-tokenless-oidc-trusted-publishing).

## Usage

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
