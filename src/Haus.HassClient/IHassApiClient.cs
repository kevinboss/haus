using System.Text.Json;

namespace Haus.HassClient;

public interface IHassApiClient
{
    // GET /api/
    Task<ApiStatus> GetApiStatusAsync(CancellationToken cancellationToken = default);

    // GET /api/states
    Task<IReadOnlyList<T>> ListStatesAsync<T>(CancellationToken cancellationToken = default);

    // GET /api/states/{entity_id}
    Task<T> GetStateAsync<T>(string entityId, CancellationToken cancellationToken = default);

    // POST /api/states/{entity_id}
    Task<T> SetStateAsync<T>(string entityId, object body, CancellationToken cancellationToken = default);

    // DELETE /api/states/{entity_id}
    Task DeleteStateAsync(string entityId, CancellationToken cancellationToken = default);

    // GET /api/events
    Task<IReadOnlyList<EventType>> ListEventTypesAsync(CancellationToken cancellationToken = default);

    // POST /api/events/{event_type}
    Task<JsonElement> FireEventAsync(string eventType, object? data = null, CancellationToken cancellationToken = default);

    // GET /api/services
    Task<IReadOnlyList<ServiceDomain>> ListServiceDomainsAsync(CancellationToken cancellationToken = default);

    // POST /api/services/{domain}/{service}
    Task<JsonElement> CallServiceAsync(string domain, string service, object? data = null, CancellationToken cancellationToken = default);

    // POST /api/template
    Task<string> RenderTemplateAsync(string template, object? variables = null, CancellationToken cancellationToken = default);

    // POST /api/config/core/check_config
    Task<ConfigCheckResult> CheckConfigAsync(CancellationToken cancellationToken = default);

    // GET /api/config/automation/config/{config_id}
    Task<T> GetAutomationConfigAsync<T>(string configId, CancellationToken cancellationToken = default);

    // POST /api/config/automation/config/{config_id}
    Task SaveAutomationConfigAsync(string configId, object config, CancellationToken cancellationToken = default);

    // DELETE /api/config/automation/config/{config_id}
    Task DeleteAutomationConfigAsync(string configId, CancellationToken cancellationToken = default);

    // GET /api/config/script/config/{object_id}
    Task<T> GetScriptConfigAsync<T>(string objectId, CancellationToken cancellationToken = default);

    // POST /api/config/script/config/{object_id}
    Task SaveScriptConfigAsync(string objectId, object config, CancellationToken cancellationToken = default);

    // DELETE /api/config/script/config/{object_id}
    Task DeleteScriptConfigAsync(string objectId, CancellationToken cancellationToken = default);

    // GET /api/config/scene/config/{config_id}
    Task<T> GetSceneConfigAsync<T>(string configId, CancellationToken cancellationToken = default);

    // POST /api/config/scene/config/{config_id}
    Task SaveSceneConfigAsync(string configId, object config, CancellationToken cancellationToken = default);

    // DELETE /api/config/scene/config/{config_id}
    Task DeleteSceneConfigAsync(string configId, CancellationToken cancellationToken = default);

    // GET /api/history/period/{start}?filter_entity_id=...&end_time=...&minimal_response&no_attributes
    Task<IReadOnlyList<IReadOnlyList<HistoryState>>> GetHistoryAsync(
        DateTimeOffset start,
        IEnumerable<string> entityIds,
        DateTimeOffset? end = null,
        bool includeAttributes = true,
        CancellationToken cancellationToken = default);

    // GET /api/logbook/{start}?entity=...&end_time=...
    Task<IReadOnlyList<LogbookEntry>> ListLogbookEntriesAsync(
        DateTimeOffset start,
        string? entityId = null,
        DateTimeOffset? end = null,
        CancellationToken cancellationToken = default);

    // POST /api/config/config_entries/options/flow {"handler": entry_id}
    Task<OptionsFlowStep> InitOptionsFlowAsync(string entryId, CancellationToken cancellationToken = default);

    // POST /api/config/config_entries/options/flow/{flow_id} body=user_input
    Task<OptionsFlowStep> ConfigureOptionsFlowAsync(string flowId, object userInput, CancellationToken cancellationToken = default);

    // DELETE /api/config/config_entries/options/flow/{flow_id}
    Task AbortOptionsFlowAsync(string flowId, CancellationToken cancellationToken = default);
}
