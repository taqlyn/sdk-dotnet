using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.OpenSsl;

namespace Taqlyn;

internal static class Signer
{
    internal static string CanonicalMessage(string method, string path, long unixTimestamp, string clientId, byte[] body)
    {
        var hash = Convert.ToHexString(SHA256.HashData(body)).ToLowerInvariant();
        return string.Join('\n', "taqlyn-v1", method.ToUpperInvariant(), path, unixTimestamp.ToString(), clientId, hash);
    }

    internal static Dictionary<string, string> Headers(Ed25519PrivateKeyParameters key, string clientId, string method, string path, byte[] body, long unixTimestamp)
    {
        var message = Encoding.UTF8.GetBytes(CanonicalMessage(method, path, unixTimestamp, clientId, body));
        var signer = new Ed25519Signer();
        signer.Init(true, key);
        signer.BlockUpdate(message, 0, message.Length);
        return new Dictionary<string, string>
        {
            ["X-Taqlyn-Client-Id"] = clientId,
            ["X-Taqlyn-Timestamp"] = unixTimestamp.ToString(),
            ["X-Taqlyn-Signature"] = Convert.ToBase64String(signer.GenerateSignature()),
        };
    }

    internal static Ed25519PrivateKeyParameters LoadPrivateKey(string pem)
    {
        var normalized = pem.Trim().Replace("\\n", "\n", StringComparison.Ordinal);
        if (normalized.StartsWith("sk_", StringComparison.Ordinal))
        {
            throw new ArgumentException("sk_* is a UX handle only; pass the PKCS#8 PEM returned at key issue.");
        }
        if (!normalized.Contains("BEGIN", StringComparison.Ordinal))
        {
            throw new ArgumentException("Private key must be a PKCS#8 PEM.");
        }
        using var reader = new StringReader(normalized);
        var parsed = new PemReader(reader).ReadObject();
        if (parsed is Ed25519PrivateKeyParameters ed)
        {
            return ed;
        }
        throw new ArgumentException("Private key must contain an Ed25519 PKCS#8 key.");
    }
}
