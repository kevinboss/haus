using System.Net.Http.Headers;
using System.Net.Http.Json;
using Haus.Hass;

namespace Haus.Rest;

public sealed class HassApiClient(ITokenProvider tokens) : IHassApiClient, IDisposable
{
    private readonly HttpClient _httpClient = new() { Timeout = Timeout.InfiniteTimeSpan };

    public async Task<T> GetAsync<T>(string path, CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);
        return await _httpClient.GetFromJsonAsync<T>(path, HassJsonOptions.Default, cancellationToken)
            ?? throw new InvalidOperationException("Empty response from Home Assistant API.");
    }

    public async Task<T> PostAsync<T>(string path, object? data = null, CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);
        var response = await _httpClient.PostAsJsonAsync(path, data ?? new object(), HassJsonOptions.Default, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(body) ? $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}" : body);
        }
        if (typeof(T) == typeof(string))
            return (T)(object)await response.Content.ReadAsStringAsync(cancellationToken);
        return await response.Content.ReadFromJsonAsync<T>(HassJsonOptions.Default, cancellationToken)
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
        var (url, token) = await tokens.GetAccessTokenAsync(cancellationToken);
        _httpClient.BaseAddress ??= new Uri(url.TrimEnd('/'));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public void Dispose() => _httpClient.Dispose();
}
