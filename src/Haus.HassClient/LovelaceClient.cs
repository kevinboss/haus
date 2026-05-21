using System.Text.Json;

namespace Haus.HassClient;

public interface ILovelaceClient
{
    Task<IReadOnlyList<DashboardRegistryEntry>> ListDashboardsAsync(CancellationToken cancellationToken = default);
    Task<DashboardRegistryEntry> CreateDashboardAsync(NewDashboard dashboard, CancellationToken cancellationToken = default);
    Task UpdateDashboardAsync(string dashboardId, DashboardUpdate update, CancellationToken cancellationToken = default);
    Task DeleteDashboardAsync(string dashboardId, CancellationToken cancellationToken = default);
    Task<JsonElement> GetConfigAsync(string? urlPath, CancellationToken cancellationToken = default);
    Task SaveConfigAsync(string? urlPath, JsonElement config, CancellationToken cancellationToken = default);
}

internal sealed class LovelaceClient(IHassWebSocketClient ws) : ILovelaceClient
{
    public Task<IReadOnlyList<DashboardRegistryEntry>> ListDashboardsAsync(CancellationToken cancellationToken = default) =>
        ws.ListDashboardsAsync(cancellationToken);

    public Task<DashboardRegistryEntry> CreateDashboardAsync(NewDashboard dashboard, CancellationToken cancellationToken = default) =>
        ws.CreateDashboardAsync(dashboard, cancellationToken);

    public Task UpdateDashboardAsync(string dashboardId, DashboardUpdate update, CancellationToken cancellationToken = default) =>
        ws.UpdateDashboardAsync(dashboardId, update, cancellationToken);

    public Task DeleteDashboardAsync(string dashboardId, CancellationToken cancellationToken = default) =>
        ws.DeleteDashboardAsync(dashboardId, cancellationToken);

    public Task<JsonElement> GetConfigAsync(string? urlPath, CancellationToken cancellationToken = default) =>
        ws.GetDashboardConfigAsync(urlPath, cancellationToken);

    public Task SaveConfigAsync(string? urlPath, JsonElement config, CancellationToken cancellationToken = default) =>
        ws.SaveDashboardConfigAsync(urlPath, config, cancellationToken);
}
