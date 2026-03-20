using System.Security.Cryptography;
using System.Text;

namespace Haus.Auth;

public static class PkceHelper
{
    public static string GenerateCodeVerifier() =>
        Base64UrlEncode(RandomNumberGenerator.GetBytes(32));

    public static string ComputeCodeChallenge(string codeVerifier) =>
        Base64UrlEncode(SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier)));

    public static string GenerateState() =>
        Base64UrlEncode(RandomNumberGenerator.GetBytes(16));

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}
