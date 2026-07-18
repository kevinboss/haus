using System.Globalization;
using System.Net.WebSockets;
using System.Text.Json;

namespace Haus.HassClient;

public sealed class HassWebSocketClient(ITokenProvider tokens) : IHassWebSocketClient, IDisposable
{
    private ClientWebSocket? _ws;
    private int _messageId;

    public async Task<IReadOnlyList<EntityRegistryEntry>> ListEntityRegistryAsync(CancellationToken cancellationToken = default)
    {
        var result = await SendAsync(new() { ["type"] = "config/entity_registry/list" }, cancellationToken);
        return result.Deserialize<List<EntityRegistryEntry>>(HassJsonOptions.Default) ?? [];
    }

    public async Task<EntityRegistryEntry?> GetEntityRegistryEntryAsync(string entityId, CancellationToken cancellationToken = default)
    {
        var result = await SendAsync(new()
        {
            ["type"] = "config/entity_registry/get",
            ["entity_id"] = entityId
        }, cancellationToken);

        // HA wraps the entry under `entity_entry` on some versions, returns it directly on others.
        var entry = result.TryGetProperty("entity_entry", out var e) ? e : result;
        if (entry.ValueKind != JsonValueKind.Object) return null;
        return entry.Deserialize<EntityRegistryEntry>(HassJsonOptions.Default);
    }

    public Task UpdateEntityRegistryEntryAsync(string entityId, EntityRegistryUpdate update, CancellationToken cancellationToken = default)
    {
        var payload = new Dictionary<string, object?>
        {
            ["type"] = "config/entity_registry/update",
            ["entity_id"] = entityId
        };
        if (update.Name is not null) payload["name"] = update.Name;
        if (update.Icon is not null) payload["icon"] = update.Icon;
        if (update.AreaId is not null) payload["area_id"] = update.AreaId;
        if (update.NewEntityId is not null) payload["new_entity_id"] = update.NewEntityId;
        return SendAsync(payload, cancellationToken);
    }

    public Task SetEntityEnabledAsync(string entityId, bool enabled, CancellationToken cancellationToken = default) =>
        SendAsync(new()
        {
            ["type"] = "config/entity_registry/update",
            ["entity_id"] = entityId,
            ["disabled_by"] = enabled ? null : "user"
        }, cancellationToken);

    public Task SetEntityHiddenAsync(string entityId, bool hidden, CancellationToken cancellationToken = default) =>
        SendAsync(new()
        {
            ["type"] = "config/entity_registry/update",
            ["entity_id"] = entityId,
            ["hidden_by"] = hidden ? "user" : null
        }, cancellationToken);

    public Task RemoveEntityRegistryEntryAsync(string entityId, CancellationToken cancellationToken = default) =>
        SendAsync(new()
        {
            ["type"] = "config/entity_registry/remove",
            ["entity_id"] = entityId
        }, cancellationToken);

    public async Task<IReadOnlyList<AreaRegistryEntry>> ListAreaRegistryAsync(CancellationToken cancellationToken = default)
    {
        var result = await SendAsync(new() { ["type"] = "config/area_registry/list" }, cancellationToken);
        return result.Deserialize<List<AreaRegistryEntry>>(HassJsonOptions.Default) ?? [];
    }

    public async Task<AreaRegistryEntry> CreateAreaAsync(NewArea area, CancellationToken cancellationToken = default)
    {
        var payload = new Dictionary<string, object?>
        {
            ["type"] = "config/area_registry/create",
            ["name"] = area.Name
        };
        if (area.Icon is not null) payload["icon"] = area.Icon;
        if (area.FloorId is not null) payload["floor_id"] = area.FloorId;

        var result = await SendAsync(payload, cancellationToken);
        return result.Deserialize<AreaRegistryEntry>(HassJsonOptions.Default)
            ?? throw new InvalidOperationException("Empty response from config/area_registry/create.");
    }

    public Task UpdateAreaAsync(string areaId, AreaRegistryUpdate update, CancellationToken cancellationToken = default)
    {
        var payload = new Dictionary<string, object?>
        {
            ["type"] = "config/area_registry/update",
            ["area_id"] = areaId
        };
        if (update.Name is not null) payload["name"] = update.Name;
        if (update.Icon is not null) payload["icon"] = update.Icon.Length == 0 ? null : update.Icon;
        if (update.FloorId is not null) payload["floor_id"] = update.FloorId.Length == 0 ? null : update.FloorId;
        return SendAsync(payload, cancellationToken);
    }

    public Task DeleteAreaAsync(string areaId, CancellationToken cancellationToken = default) =>
        SendAsync(new()
        {
            ["type"] = "config/area_registry/delete",
            ["area_id"] = areaId
        }, cancellationToken);

    public async Task<IReadOnlyList<DashboardRegistryEntry>> ListDashboardsAsync(CancellationToken cancellationToken = default)
    {
        var result = await SendAsync(new() { ["type"] = "lovelace/dashboards/list" }, cancellationToken);
        return result.Deserialize<List<DashboardRegistryEntry>>(HassJsonOptions.Default) ?? [];
    }

    public async Task<DashboardRegistryEntry> CreateDashboardAsync(NewDashboard dashboard, CancellationToken cancellationToken = default)
    {
        var payload = new Dictionary<string, object?>
        {
            ["type"] = "lovelace/dashboards/create",
            ["url_path"] = dashboard.UrlPath,
            ["title"] = dashboard.Title,
            ["mode"] = "storage",
            ["show_in_sidebar"] = dashboard.ShowInSidebar,
            ["require_admin"] = dashboard.RequireAdmin
        };
        if (dashboard.Icon is not null) payload["icon"] = dashboard.Icon;

        var result = await SendAsync(payload, cancellationToken);
        return result.Deserialize<DashboardRegistryEntry>(HassJsonOptions.Default)
            ?? throw new InvalidOperationException("Empty response from lovelace/dashboards/create.");
    }

    public Task UpdateDashboardAsync(string dashboardId, DashboardUpdate update, CancellationToken cancellationToken = default)
    {
        var payload = new Dictionary<string, object?>
        {
            ["type"] = "lovelace/dashboards/update",
            ["dashboard_id"] = dashboardId
        };
        if (update.Title is not null) payload["title"] = update.Title;
        if (update.Icon is not null) payload["icon"] = update.Icon.Length == 0 ? null : update.Icon;
        if (update.ShowInSidebar is not null) payload["show_in_sidebar"] = update.ShowInSidebar.Value;
        if (update.RequireAdmin is not null) payload["require_admin"] = update.RequireAdmin.Value;
        return SendAsync(payload, cancellationToken);
    }

    public Task DeleteDashboardAsync(string dashboardId, CancellationToken cancellationToken = default) =>
        SendAsync(new()
        {
            ["type"] = "lovelace/dashboards/delete",
            ["dashboard_id"] = dashboardId
        }, cancellationToken);

    public Task<JsonElement> GetDashboardConfigAsync(string? urlPath, CancellationToken cancellationToken = default)
    {
        var payload = new Dictionary<string, object?> { ["type"] = "lovelace/config" };
        if (urlPath is not null) payload["url_path"] = urlPath;
        return SendAsync(payload, cancellationToken);
    }

    public Task SaveDashboardConfigAsync(string? urlPath, JsonElement config, CancellationToken cancellationToken = default)
    {
        var payload = new Dictionary<string, object?>
        {
            ["type"] = "lovelace/config/save",
            ["config"] = config
        };
        if (urlPath is not null) payload["url_path"] = urlPath;
        return SendAsync(payload, cancellationToken);
    }

    public async Task<IReadOnlyList<TraceSummary>> ListTracesAsync(string domain, string itemId, CancellationToken cancellationToken = default)
    {
        var result = await SendAsync(new()
        {
            ["type"] = "trace/list",
            ["domain"] = domain,
            ["item_id"] = itemId
        }, cancellationToken);
        return result.Deserialize<List<TraceSummary>>(HassJsonOptions.Default) ?? [];
    }

    public Task<JsonElement> GetTraceAsync(string domain, string itemId, string runId, CancellationToken cancellationToken = default) =>
        SendAsync(new()
        {
            ["type"] = "trace/get",
            ["domain"] = domain,
            ["item_id"] = itemId,
            ["run_id"] = runId
        }, cancellationToken);

    public async Task<IReadOnlyList<SystemLogEntry>> ListSystemLogAsync(CancellationToken cancellationToken = default)
    {
        var result = await SendAsync(new() { ["type"] = "system_log/list" }, cancellationToken);
        return result.ValueKind == JsonValueKind.Array
            ? result.EnumerateArray().Select(SystemLogEntry.From).ToList()
            : [];
    }

    public async Task<IReadOnlyDictionary<string, IReadOnlyList<StatisticsRow>>> GetStatisticsDuringPeriodAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        IEnumerable<string> statisticIds,
        string period,
        CancellationToken cancellationToken = default)
    {
        var result = await SendAsync(new()
        {
            ["type"] = "recorder/statistics_during_period",
            ["start_time"] = start.ToString("o", CultureInfo.InvariantCulture),
            ["end_time"] = end.ToString("o", CultureInfo.InvariantCulture),
            ["statistic_ids"] = statisticIds.ToArray(),
            ["period"] = period
        }, cancellationToken);
        var byEntity = result.Deserialize<Dictionary<string, List<StatisticsRow>>>(HassJsonOptions.Default) ?? [];
        return byEntity.ToDictionary(kv => kv.Key, kv => (IReadOnlyList<StatisticsRow>)kv.Value);
    }

    public Task<JsonElement> CreateHelperAsync(string domain, IReadOnlyDictionary<string, object?> fields, CancellationToken cancellationToken = default)
    {
        var payload = new Dictionary<string, object?>(fields) { ["type"] = $"{domain}/create" };
        return SendAsync(payload, cancellationToken);
    }

    public Task DeleteHelperAsync(string domain, string id, CancellationToken cancellationToken = default) =>
        SendAsync(new()
        {
            ["type"] = $"{domain}/delete",
            [$"{domain}_id"] = id
        }, cancellationToken);

    public Task UpdateZoneAsync(string zoneId, ZoneUpdate update, CancellationToken cancellationToken = default)
    {
        var payload = new Dictionary<string, object?>
        {
            ["type"] = "zone/update",
            ["zone_id"] = zoneId
        };
        if (update.Name is not null) payload["name"] = update.Name;
        if (update.Latitude is not null) payload["latitude"] = update.Latitude.Value;
        if (update.Longitude is not null) payload["longitude"] = update.Longitude.Value;
        if (update.Radius is not null) payload["radius"] = update.Radius.Value;
        if (update.Passive is not null) payload["passive"] = update.Passive.Value;
        if (update.Icon is not null) payload["icon"] = update.Icon;
        return SendAsync(payload, cancellationToken);
    }

    public Task UpdateCoreConfigAsync(
        double? latitude = null,
        double? longitude = null,
        double? radius = null,
        CancellationToken cancellationToken = default)
    {
        var payload = new Dictionary<string, object?> { ["type"] = "config/core/update" };
        if (latitude is not null) payload["latitude"] = latitude.Value;
        if (longitude is not null) payload["longitude"] = longitude.Value;
        if (radius is not null) payload["radius"] = radius.Value;
        return SendAsync(payload, cancellationToken);
    }

    public async Task<IReadOnlyList<ConfigEntry>> ListConfigEntriesAsync(CancellationToken cancellationToken = default)
    {
        var result = await SendAsync(new() { ["type"] = "config_entries/get" }, cancellationToken);
        return result.Deserialize<List<ConfigEntry>>(HassJsonOptions.Default) ?? [];
    }

    private async Task<JsonElement> SendAsync(Dictionary<string, object?> command, CancellationToken cancellationToken)
    {
        var ws = await EnsureConnectedAsync(cancellationToken);
        var id = Interlocked.Increment(ref _messageId);
        command["id"] = id;

        await SendRawAsync(ws, command, cancellationToken);
        return await ReceiveResultAsync(ws, id, cancellationToken);
    }

    private async Task<ClientWebSocket> EnsureConnectedAsync(CancellationToken cancellationToken)
    {
        if (_ws is { State: WebSocketState.Open })
            return _ws;

        var (url, token) = await tokens.GetAccessTokenAsync(cancellationToken);
        var wsUrl = url.Replace("http://", "ws://").Replace("https://", "wss://") + "/api/websocket";

        _ws = new ClientWebSocket();
        await _ws.ConnectAsync(new Uri(wsUrl), cancellationToken);

        // auth_required
        await ReceiveAsync(_ws, cancellationToken);

        // authenticate
        await SendRawAsync(_ws, new { type = "auth", access_token = token }, cancellationToken);
        var authResp = await ReceiveAsync(_ws, cancellationToken);
        if (authResp.GetProperty("type").GetString() != "auth_ok")
            throw new InvalidOperationException("WebSocket authentication failed.");

        return _ws;
    }

    private static async Task SendRawAsync(ClientWebSocket ws, object data, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.SerializeToUtf8Bytes(data);
        await ws.SendAsync(json, WebSocketMessageType.Text, true, cancellationToken);
    }

    private static async Task<JsonElement> ReceiveAsync(ClientWebSocket ws, CancellationToken cancellationToken)
    {
        var buffer = new byte[65536];
        using var ms = new MemoryStream();
        WebSocketReceiveResult result;
        do
        {
            result = await ws.ReceiveAsync(buffer, cancellationToken);
            ms.Write(buffer, 0, result.Count);
        } while (!result.EndOfMessage);

        ms.Position = 0;
        using var doc = await JsonDocument.ParseAsync(ms, cancellationToken: cancellationToken);
        return doc.RootElement.Clone();
    }

    private static async Task<JsonElement> ReceiveResultAsync(ClientWebSocket ws, int expectedId, CancellationToken cancellationToken)
    {
        while (true)
        {
            var msg = await ReceiveAsync(ws, cancellationToken);

            if (!msg.TryGetProperty("id", out var idProp) || idProp.GetInt32() != expectedId)
                continue;

            if (msg.TryGetProperty("success", out var success) && !success.GetBoolean())
            {
                var error = msg.TryGetProperty("error", out var err)
                    ? err.GetProperty("message").GetString() : "Unknown error";
                throw new InvalidOperationException(error);
            }

            return msg.TryGetProperty("result", out var r) ? r.Clone() : default;
        }
    }

    public void Dispose() => _ws?.Dispose();
}
