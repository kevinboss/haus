using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace Haus.Auth;

public sealed class AuthService(ILogger<AuthService> logger) : IAuthService
{
    private const string EnvVarToken = "HASS_TOKEN";
    private const string EnvVarUrl = "HASS_URL";
    private const string AuthorizePath = "/auth/authorize";
    private const string TokenPath = "/auth/token";
    private const string SuccessHtml =
        "<html><body><h1>Login successful!</h1><p>You can close this tab and return to the terminal.</p></body></html>";

    public bool IsLoggedIn =>
        (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(EnvVarToken))
         && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(EnvVarUrl)))
        || TokenStore.Exists;

    public async Task LoginAsync(string url, CancellationToken cancellationToken = default)
    {
        url = url.TrimEnd('/');

        var codeVerifier = PkceHelper.GenerateCodeVerifier();
        var codeChallenge = PkceHelper.ComputeCodeChallenge(codeVerifier);
        var state = PkceHelper.GenerateState();

        using var listener = new HttpListener();
        listener.Prefixes.Add(AuthConstants.RedirectUri);
        listener.Start();

        var authorizeUrl = $"{url}{AuthorizePath}" +
            $"?client_id={Uri.EscapeDataString(AuthConstants.ClientId)}" +
            $"&redirect_uri={Uri.EscapeDataString(AuthConstants.RedirectUri)}" +
            $"&response_type=code" +
            $"&code_challenge={codeChallenge}" +
            $"&code_challenge_method=S256" +
            $"&state={state}";

        logger.LogInformation("Opening browser for authentication");
        BrowserHelper.Open(authorizeUrl);

        var code = await WaitForCallbackAsync(listener, state, cancellationToken);

        logger.LogInformation("Authorization code received, exchanging for tokens");
        var tokenResult = await ExchangeCodeAsync(url, code, codeVerifier, cancellationToken);

        var tokenData = new TokenData(
            url,
            tokenResult.RefreshToken,
            tokenResult.AccessToken,
            DateTimeOffset.UtcNow.AddSeconds(tokenResult.ExpiresIn));

        await TokenStore.SaveAsync(tokenData, cancellationToken);
        logger.LogInformation("Login successful — tokens saved");
    }

    public async Task<(string Url, string AccessToken)> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var envToken = Environment.GetEnvironmentVariable(EnvVarToken);
        var envUrl = Environment.GetEnvironmentVariable(EnvVarUrl);
        if (!string.IsNullOrEmpty(envToken) && !string.IsNullOrEmpty(envUrl))
            return (envUrl.TrimEnd('/'), envToken);

        var tokenData = await TokenStore.LoadAsync(cancellationToken)
            ?? throw new InvalidOperationException("Not logged in. Run `haus login` or set HASS_URL/HASS_TOKEN.");

        if (tokenData.ExpiresAt > DateTimeOffset.UtcNow.AddMinutes(1))
            return (tokenData.Url, tokenData.AccessToken);

        logger.LogInformation("Access token expired, refreshing");
        return await RefreshTokenAsync(tokenData, cancellationToken);
    }

    private static async Task<string> WaitForCallbackAsync(
        HttpListener listener, string expectedState, CancellationToken cancellationToken)
    {
        var context = await listener.GetContextAsync().WaitAsync(cancellationToken);
        var query = context.Request.QueryString;
        var code = query["code"];
        var returnedState = query["state"];

        var responseBytes = System.Text.Encoding.UTF8.GetBytes(SuccessHtml);
        context.Response.ContentType = "text/html";
        context.Response.ContentLength64 = responseBytes.Length;
        await context.Response.OutputStream.WriteAsync(responseBytes, cancellationToken);
        context.Response.Close();
        listener.Stop();

        if (returnedState != expectedState)
            throw new InvalidOperationException("OAuth state mismatch — possible CSRF attack.");

        if (string.IsNullOrEmpty(code))
            throw new InvalidOperationException("No authorization code received.");

        return code;
    }

    private static async Task<OAuthTokenResponse> ExchangeCodeAsync(
        string url, string code, string codeVerifier, CancellationToken cancellationToken)
    {
        using var httpClient = new HttpClient();
        var response = await httpClient.PostAsync(
            $"{url}{TokenPath}",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["code"] = code,
                ["client_id"] = AuthConstants.ClientId,
                ["code_verifier"] = codeVerifier,
            }),
            cancellationToken);

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<OAuthTokenResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Empty token response.");
    }

    private async Task<(string Url, string AccessToken)> RefreshTokenAsync(
        TokenData tokenData, CancellationToken cancellationToken)
    {
        using var httpClient = new HttpClient();
        var response = await httpClient.PostAsync(
            $"{tokenData.Url}{TokenPath}",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = tokenData.RefreshToken,
                ["client_id"] = AuthConstants.ClientId,
            }),
            cancellationToken);

        response.EnsureSuccessStatusCode();
        var tokenResult = await response.Content.ReadFromJsonAsync<OAuthTokenResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Empty token response.");

        var updated = tokenData with
        {
            AccessToken = tokenResult.AccessToken,
            ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(tokenResult.ExpiresIn),
        };

        await TokenStore.SaveAsync(updated, cancellationToken);
        return (updated.Url, updated.AccessToken);
    }

    private sealed record OAuthTokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("refresh_token")] string RefreshToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn,
        [property: JsonPropertyName("token_type")] string TokenType);
}
