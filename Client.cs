using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Org.BouncyCastle.Crypto.Parameters;

namespace Taqlyn;

public sealed class TaqlynClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public const string DefaultApiBaseUrl = "https://api.taqlyn.com";

    private readonly string _baseUrl;
    private readonly string _clientId;
    private readonly Ed25519PrivateKeyParameters _key;
    private readonly HttpClient _http;
    private readonly Func<long> _now;

    public TaqlynClient(string clientId, string privateKeyPem, HttpClient? httpClient = null, Func<long>? now = null)
        : this(null, clientId, privateKeyPem, httpClient, now)
    {
    }

    public TaqlynClient(string? baseUrl, string clientId, string privateKeyPem, HttpClient? httpClient = null, Func<long>? now = null)
    {
        var rawUrl = !string.IsNullOrWhiteSpace(baseUrl)
            ? baseUrl
            : Environment.GetEnvironmentVariable("TAQLYN_BASE_URL")
              ?? Environment.GetEnvironmentVariable("TAQLYN_API_URL")
              ?? DefaultApiBaseUrl;

        if (string.IsNullOrWhiteSpace(rawUrl)) throw new ArgumentException("baseUrl is required");
        if (string.IsNullOrWhiteSpace(clientId)) throw new ArgumentException("clientId is required");
        if (!clientId.StartsWith("app_test_", StringComparison.Ordinal) && !clientId.StartsWith("app_live_", StringComparison.Ordinal))
        {
            throw new ArgumentException("clientId must start with app_test_ or app_live_");
        }
        _baseUrl = rawUrl.TrimEnd('/');
        _clientId = clientId.Trim();
        _key = Signer.LoadPrivateKey(privateKeyPem);
        _http = httpClient ?? new HttpClient();
        _now = now ?? (() => DateTimeOffset.UtcNow.ToUnixTimeSeconds());
    }

    public async Task<ShortLink> CreateShortLinkAsync(CreateShortLinkRequest input, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(input.DestinationWeb))
        {
            throw new ArgumentException("destinationWeb is required");
        }
        return await SendAsync<ShortLink>(HttpMethod.Post, "/v1/short-links", input, cancellationToken).ConfigureAwait(false);
    }

    private async Task<T> SendAsync<T>(HttpMethod method, string path, object? body, CancellationToken cancellationToken)
    {
        var payload = body is null ? "{}"u8.ToArray() : JsonSerializer.SerializeToUtf8Bytes(body, JsonOptions);
        using var request = new HttpRequestMessage(method, _baseUrl + path)
        {
            Content = new ByteArrayContent(payload),
        };
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        foreach (var header in Signer.Headers(_key, _clientId, method.Method, path, payload, _now()))
        {
            request.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
        using var response = await _http.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var text = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new TaqlynApiException((int)response.StatusCode, text);
        }
        return JsonSerializer.Deserialize<T>(text, JsonOptions) ?? throw new TaqlynApiException((int)response.StatusCode, text);
    }
}

public sealed class CreateShortLinkRequest
{
    public required string DestinationWeb { get; init; }
    public string? Mode { get; init; }
    public string? DestinationPath { get; init; }
}

public sealed class ShortLink
{
    public string? Id { get; init; }
    public string? ShortUrl { get; init; }
    public string? Code { get; init; }
}

public sealed class TaqlynApiException : Exception
{
    public int Status { get; }
    public string Body { get; }

    public TaqlynApiException(int status, string body) : base($"Taqlyn API {status}")
    {
        Status = status;
        Body = body;
    }
}
