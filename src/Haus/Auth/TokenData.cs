using System.Text.Json.Serialization;

namespace Haus.Auth;

public record TokenData(
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("refresh_token")] string RefreshToken,
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("expires_at")] DateTimeOffset ExpiresAt);
