using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Haus.HassClient;

public sealed class HassApiClient(ITokenProvider tokens) : IHassApiClient, IDisposable
{
    private readonly HttpClient _httpClient = new() { Timeout = Timeout.InfiniteTimeSpan };

    public Task<ApiStatus> GetApiStatusAsync(CancellationToken cancellationToken = default) =>
        GetAsync<ApiStatus>("/api/", cancellationToken);

    public async Task<IReadOnlyList<T>> ListStatesAsync<T>(CancellationToken cancellationToken = default) =>
        await GetAsync<List<T>>("/api/states", cancellationToken);

    public Task<T> GetStateAsync<T>(string entityId, CancellationToken cancellationToken = default) =>
        GetAsync<T>($"/api/states/{Uri.EscapeDataString(entityId)}", cancellationToken);

    public Task<T> SetStateAsync<T>(string entityId, object body, CancellationToken cancellationToken = default) =>
        PostAsync<T>($"/api/states/{Uri.EscapeDataString(entityId)}", body, cancellationToken);

    public Task DeleteStateAsync(string entityId, CancellationToken cancellationToken = default) =>
        DeleteAsync($"/api/states/{Uri.EscapeDataString(entityId)}", cancellationToken);

    public async Task<IReadOnlyList<EventType>> ListEventTypesAsync(CancellationToken cancellationToken = default) =>
        await GetAsync<List<EventType>>("/api/events", cancellationToken);

    public Task<JsonElement> FireEventAsync(string eventType, object? data = null, CancellationToken cancellationToken = default) =>
        PostAsync<JsonElement>($"/api/events/{Uri.EscapeDataString(eventType)}", data, cancellationToken);

    public async Task<IReadOnlyList<ServiceDomain>> ListServiceDomainsAsync(CancellationToken cancellationToken = default) =>
        await GetAsync<List<ServiceDomain>>("/api/services", cancellationToken);

    public Task<JsonElement> CallServiceAsync(string domain, string service, object? data = null, CancellationToken cancellationToken = default) =>
        PostAsync<JsonElement>($"/api/services/{Uri.EscapeDataString(domain)}/{Uri.EscapeDataString(service)}", data, cancellationToken);

    public Task<string> RenderTemplateAsync(string template, object? variables = null, CancellationToken cancellationToken = default)
    {
        var body = variables is null
            ? (object)new { template }
            : new { template, variables };
        return PostAsync<string>("/api/template", body, cancellationToken);
    }

    public Task<ConfigCheckResult> CheckConfigAsync(CancellationToken cancellationToken = default) =>
        PostAsync<ConfigCheckResult>("/api/config/core/check_config", null, cancellationToken);

    public Task<T> GetAutomationConfigAsync<T>(string configId, CancellationToken cancellationToken = default) =>
        GetAsync<T>($"/api/config/automation/config/{Uri.EscapeDataString(configId)}", cancellationToken);

    public Task SaveAutomationConfigAsync(string configId, object config, CancellationToken cancellationToken = default) =>
        PostAsync<JsonElement>($"/api/config/automation/config/{Uri.EscapeDataString(configId)}", config, cancellationToken);

    public Task DeleteAutomationConfigAsync(string configId, CancellationToken cancellationToken = default) =>
        DeleteAsync($"/api/config/automation/config/{Uri.EscapeDataString(configId)}", cancellationToken);

    public Task<T> GetScriptConfigAsync<T>(string objectId, CancellationToken cancellationToken = default) =>
        GetAsync<T>($"/api/config/script/config/{Uri.EscapeDataString(objectId)}", cancellationToken);

    public Task SaveScriptConfigAsync(string objectId, object config, CancellationToken cancellationToken = default) =>
        PostAsync<JsonElement>($"/api/config/script/config/{Uri.EscapeDataString(objectId)}", config, cancellationToken);

    public Task DeleteScriptConfigAsync(string objectId, CancellationToken cancellationToken = default) =>
        DeleteAsync($"/api/config/script/config/{Uri.EscapeDataString(objectId)}", cancellationToken);

    public Task<T> GetSceneConfigAsync<T>(string configId, CancellationToken cancellationToken = default) =>
        GetAsync<T>($"/api/config/scene/config/{Uri.EscapeDataString(configId)}", cancellationToken);

    public Task SaveSceneConfigAsync(string configId, object config, CancellationToken cancellationToken = default) =>
        PostAsync<JsonElement>($"/api/config/scene/config/{Uri.EscapeDataString(configId)}", config, cancellationToken);

    public Task DeleteSceneConfigAsync(string configId, CancellationToken cancellationToken = default) =>
        DeleteAsync($"/api/config/scene/config/{Uri.EscapeDataString(configId)}", cancellationToken);

    public async Task<IReadOnlyList<IReadOnlyList<HistoryState>>> GetHistoryAsync(
        DateTimeOffset start,
        IEnumerable<string> entityIds,
        DateTimeOffset? end = null,
        bool includeAttributes = true,
        CancellationToken cancellationToken = default)
    {
        var path = $"/api/history/period/{Uri.EscapeDataString(start.ToString("o", CultureInfo.InvariantCulture))}";
        var queryParts = new List<string>
        {
            $"filter_entity_id={Uri.EscapeDataString(string.Join(',', entityIds))}"
        };
        if (end is not null)
            queryParts.Add($"end_time={Uri.EscapeDataString(end.Value.ToString("o", CultureInfo.InvariantCulture))}");
        if (!includeAttributes)
        {
            queryParts.Add("minimal_response");
            queryParts.Add("no_attributes");
        }
        var url = $"{path}?{string.Join('&', queryParts)}";

        var groups = await GetAsync<List<List<HistoryState>>>(url, cancellationToken);
        return groups.Select(g => (IReadOnlyList<HistoryState>)g).ToList();
    }

    public async Task<IReadOnlyList<LogbookEntry>> ListLogbookEntriesAsync(
        DateTimeOffset start,
        string? entityId = null,
        DateTimeOffset? end = null,
        CancellationToken cancellationToken = default)
    {
        var path = $"/api/logbook/{Uri.EscapeDataString(start.ToString("o", CultureInfo.InvariantCulture))}";
        var queryParts = new List<string>();
        if (entityId is not null)
            queryParts.Add($"entity={Uri.EscapeDataString(entityId)}");
        if (end is not null)
            queryParts.Add($"end_time={Uri.EscapeDataString(end.Value.ToString("o", CultureInfo.InvariantCulture))}");
        var url = queryParts.Count == 0 ? path : $"{path}?{string.Join('&', queryParts)}";

        return await GetAsync<List<LogbookEntry>>(url, cancellationToken);
    }

    public Task<OptionsFlowStep> InitOptionsFlowAsync(string entryId, CancellationToken cancellationToken = default) =>
        PostAsync<OptionsFlowStep>("/api/config/config_entries/options/flow",
            new { handler = entryId, show_advanced_options = true }, cancellationToken);

    public Task<OptionsFlowStep> ConfigureOptionsFlowAsync(string flowId, object userInput, CancellationToken cancellationToken = default) =>
        PostAsync<OptionsFlowStep>($"/api/config/config_entries/options/flow/{Uri.EscapeDataString(flowId)}",
            userInput, cancellationToken);

    public Task AbortOptionsFlowAsync(string flowId, CancellationToken cancellationToken = default) =>
        DeleteAsync($"/api/config/config_entries/options/flow/{Uri.EscapeDataString(flowId)}", cancellationToken);

    public Task<ConfigEntryOperationResult> ReloadConfigEntryAsync(string entryId, CancellationToken cancellationToken = default) =>
        PostAsync<ConfigEntryOperationResult>(
            $"/api/config/config_entries/entry/{Uri.EscapeDataString(entryId)}/reload", null, cancellationToken);

    public Task<ConfigEntryOperationResult> RemoveConfigEntryAsync(string entryId, CancellationToken cancellationToken = default) =>
        DeleteAsync<ConfigEntryOperationResult>(
            $"/api/config/config_entries/entry/{Uri.EscapeDataString(entryId)}", cancellationToken);

    private async Task<T> GetAsync<T>(string path, CancellationToken cancellationToken)
    {
        await EnsureAuthenticatedAsync(cancellationToken);
        return await _httpClient.GetFromJsonAsync<T>(path, HassJsonOptions.Default, cancellationToken)
            ?? throw new InvalidOperationException("Empty response from Home Assistant API.");
    }

    private async Task<T> PostAsync<T>(string path, object? data, CancellationToken cancellationToken)
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

    private async Task DeleteAsync(string path, CancellationToken cancellationToken)
    {
        await EnsureAuthenticatedAsync(cancellationToken);
        var response = await _httpClient.DeleteAsync(path, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private async Task<T> DeleteAsync<T>(string path, CancellationToken cancellationToken)
    {
        await EnsureAuthenticatedAsync(cancellationToken);
        var response = await _httpClient.DeleteAsync(path, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                string.IsNullOrWhiteSpace(body) ? $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}" : body);
        }
        return await response.Content.ReadFromJsonAsync<T>(HassJsonOptions.Default, cancellationToken)
            ?? throw new InvalidOperationException("Empty response from Home Assistant API.");
    }

    private async Task EnsureAuthenticatedAsync(CancellationToken cancellationToken)
    {
        var (url, token) = await tokens.GetAccessTokenAsync(cancellationToken);
        _httpClient.BaseAddress ??= new Uri(url.TrimEnd('/'));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public void Dispose() => _httpClient.Dispose();
}
