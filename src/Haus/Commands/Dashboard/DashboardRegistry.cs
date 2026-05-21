using System.Text.Json;
using Haus.Connection;

namespace Haus.Commands.Dashboard;

internal static class DashboardRegistry
{
    public static async Task<List<DashboardRegistryEntry>> ListAsync(IHassWebSocketClient ws, CancellationToken cancellationToken)
    {
        var raw = await ws.SendCommandAsync(new Dictionary<string, object?> { ["type"] = LovelaceCommands.DashboardsList }, cancellationToken);
        return raw.Deserialize<List<DashboardRegistryEntry>>(HausJsonOptions.Default) ?? [];
    }

    public static async Task<DashboardRegistryEntry?> FindByUrlPathAsync(IHassWebSocketClient ws, string urlPath, CancellationToken cancellationToken)
    {
        var entries = await ListAsync(ws, cancellationToken);
        return entries.FirstOrDefault(e => string.Equals(e.UrlPath, urlPath, StringComparison.Ordinal));
    }
}
