using System.Net.Http.Headers;
using System.Net.Http.Json;
using Haus.Auth;

namespace Haus.Connection;

public sealed class HassApiClient(IAuthService authService) : IHassApiClient, IDisposable
{
    private readonly HttpClient _httpClient = new() { Timeout = Timeout.InfiniteTimeSpan };

    public async Task<T> GetAsync<T>(string path, CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);
        return await _httpClient.GetFromJsonAsync<T>(path, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Empty response from Home Assistant API.");
    }

    public async Task<T> PostAsync<T>(string path, object? data = null, CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);
        var response = await _httpClient.PostAsJsonAsync(path, data ?? new object(), cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(body) ? $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}" : body);
        }
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Empty response from Home Assistant API.");
    }

    public async Task DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);
        var response = await _httpClient.DeleteAsync(path, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private async Task EnsureAuthenticatedAsync(CancellationToken cancellationToken)
    {
        var (url, token) = await authService.GetAccessTokenAsync(cancellationToken);
        _httpClient.BaseAddress ??= new Uri(url.TrimEnd('/'));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public void Dispose() => _httpClient.Dispose();
}
