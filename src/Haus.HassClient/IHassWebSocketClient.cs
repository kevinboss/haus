using System.Text.Json;

namespace Haus.HassClient;

public interface IHassWebSocketClient
{
    // config/entity_registry/list
    Task<IReadOnlyList<EntityRegistryEntry>> ListEntityRegistryAsync(CancellationToken cancellationToken = default);

    // config/entity_registry/get
    Task<EntityRegistryEntry?> GetEntityRegistryEntryAsync(string entityId, CancellationToken cancellationToken = default);

    // config/entity_registry/update — null fields are not sent
    Task UpdateEntityRegistryEntryAsync(string entityId, EntityRegistryUpdate update, CancellationToken cancellationToken = default);

    // config/entity_registry/update — flips disabled_by between "user" and null
    Task SetEntityEnabledAsync(string entityId, bool enabled, CancellationToken cancellationToken = default);

    // config/entity_registry/update — flips hidden_by between "user" and null
    Task SetEntityHiddenAsync(string entityId, bool hidden, CancellationToken cancellationToken = default);

    // config/entity_registry/remove
    Task RemoveEntityRegistryEntryAsync(string entityId, CancellationToken cancellationToken = default);

    // config/area_registry/list
    Task<IReadOnlyList<AreaRegistryEntry>> ListAreaRegistryAsync(CancellationToken cancellationToken = default);

    // config/area_registry/create — returns the created area (area_id is server-assigned)
    Task<AreaRegistryEntry> CreateAreaAsync(NewArea area, CancellationToken cancellationToken = default);

    // config/area_registry/update — null fields are not sent; empty icon/floor clears
    Task UpdateAreaAsync(string areaId, AreaRegistryUpdate update, CancellationToken cancellationToken = default);

    // config/area_registry/delete
    Task DeleteAreaAsync(string areaId, CancellationToken cancellationToken = default);

    // config/label_registry/list
    Task<IReadOnlyList<LabelEntry>> ListLabelRegistryAsync(CancellationToken cancellationToken = default);

    // config/label_registry/create — returns the created label (label_id is server-assigned)
    Task<LabelEntry> CreateLabelAsync(NewLabel label, CancellationToken cancellationToken = default);

    // config/label_registry/update — null fields are not sent; empty color/icon/description clears
    Task UpdateLabelAsync(string labelId, LabelUpdate update, CancellationToken cancellationToken = default);

    // config/label_registry/delete
    Task DeleteLabelAsync(string labelId, CancellationToken cancellationToken = default);

    // config/device_registry/list
    Task<IReadOnlyList<DeviceRegistryEntry>> ListDeviceRegistryAsync(CancellationToken cancellationToken = default);

    // config/device_registry/update — null fields are not sent; empty area clears
    Task UpdateDeviceAsync(string deviceId, DeviceRegistryUpdate update, CancellationToken cancellationToken = default);

    // lovelace/dashboards/list
    Task<IReadOnlyList<DashboardRegistryEntry>> ListDashboardsAsync(CancellationToken cancellationToken = default);

    // lovelace/dashboards/create
    Task<DashboardRegistryEntry> CreateDashboardAsync(NewDashboard dashboard, CancellationToken cancellationToken = default);

    // lovelace/dashboards/update — null fields are not sent
    Task UpdateDashboardAsync(string dashboardId, DashboardUpdate update, CancellationToken cancellationToken = default);

    // lovelace/dashboards/delete
    Task DeleteDashboardAsync(string dashboardId, CancellationToken cancellationToken = default);

    // lovelace/config (omit urlPath for the default dashboard)
    Task<JsonElement> GetDashboardConfigAsync(string? urlPath, CancellationToken cancellationToken = default);

    // lovelace/config/save
    Task SaveDashboardConfigAsync(string? urlPath, JsonElement config, CancellationToken cancellationToken = default);

    // trace/list
    Task<IReadOnlyList<TraceSummary>> ListTracesAsync(string domain, string itemId, CancellationToken cancellationToken = default);

    // trace/get — run details are deeply polymorphic; returned as JsonElement
    Task<JsonElement> GetTraceAsync(string domain, string itemId, string runId, CancellationToken cancellationToken = default);

    // system_log/list
    Task<IReadOnlyList<SystemLogEntry>> ListSystemLogAsync(CancellationToken cancellationToken = default);

    // recorder/statistics_during_period
    Task<IReadOnlyDictionary<string, IReadOnlyList<StatisticsRow>>> GetStatisticsDuringPeriodAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        IEnumerable<string> statisticIds,
        string period,
        CancellationToken cancellationToken = default);

    // {domain}/create — generic helper create (input_boolean, input_text, counter, timer, ...)
    Task<JsonElement> CreateHelperAsync(string domain, IReadOnlyDictionary<string, object?> fields, CancellationToken cancellationToken = default);

    // {domain}/delete — generic helper delete
    Task DeleteHelperAsync(string domain, string id, CancellationToken cancellationToken = default);

    // zone/update — null fields are not sent
    Task UpdateZoneAsync(string zoneId, ZoneUpdate update, CancellationToken cancellationToken = default);

    // config/core/update — installation-level config (used for zone.home location)
    Task UpdateCoreConfigAsync(
        double? latitude = null,
        double? longitude = null,
        double? radius = null,
        CancellationToken cancellationToken = default);

    // config_entries/get
    Task<IReadOnlyList<ConfigEntry>> ListConfigEntriesAsync(CancellationToken cancellationToken = default);

    // config_entries/disable — disabled_by "user" to disable, null to re-enable
    Task<ConfigEntryOperationResult> SetConfigEntryDisabledAsync(string entryId, bool disabled, CancellationToken cancellationToken = default);

    // config_entries/flow/progress — flows HA started itself (reauth, discovery)
    Task<IReadOnlyList<ConfigFlowProgress>> ListFlowsInProgressAsync(CancellationToken cancellationToken = default);
}
