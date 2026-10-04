using System.Security.Cryptography;
using System.Text;

namespace ComptaClub.Security;

public static class McpApiKeyGenerator
{
    private const string PREFIX = "cc-";

    public static (string Identifier, string Secret, string PlainText, string Hash, string LastFour) Generate()
    {
        var _identifier = Convert.ToHexString(RandomNumberGenerator.GetBytes(12)).ToLowerInvariant();
        var _secret = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
        return (_identifier, _secret, $"{PREFIX}{_identifier}.{_secret}", ComputeHash(_secret), _secret[^4..]);
    }

    public static string ComputeHash(string secret) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(secret)));

    public static bool TryParse(string value, out string identifier, out string secret)
    {
        identifier = string.Empty;
        secret = string.Empty;
        if (value.Length != 92 || !value.StartsWith(PREFIX, StringComparison.Ordinal) || value[27] != '.')
        {
            return false;
        }
        identifier = value[3..27];
        secret = value[28..];
        return identifier.All(Uri.IsHexDigit) && secret.All(Uri.IsHexDigit);
    }

    public static bool FixedTimeEquals(string expectedHash, string suppliedHash)
    {
        if (expectedHash.Length != 64 || !expectedHash.All(Uri.IsHexDigit))
        {
            return false;
        }
        return CryptographicOperations.FixedTimeEquals(Convert.FromHexString(expectedHash), Convert.FromHexString(suppliedHash));
    }
}
