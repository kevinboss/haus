using System.Text.Json;
using Haus.Rest;
using Haus.Hass;
using Haus.Ws;

namespace Haus.Commands.Entity;

internal static class EntityRegistry
{
    public static async Task<EntityRegistryEntry?> FindByUniqueIdAsync(
        IHassWebSocketClient ws,
        string platform,
        string uniqueId,
        CancellationToken cancellationToken,
        int attempts = 10,
        int delayMs = 200)
    {
        for (var attempt = 0; attempt < attempts; attempt++)
        {
            var result = await ws.SendCommandAsync(new Dictionary<string, object?> { ["type"] = EntityRegistryCommands.List }, cancellationToken);
            var entries = result.Deserialize<List<EntityRegistryEntry>>() ?? [];
            var entry = entries.SingleOrDefault(e => e.Platform == platform && e.UniqueId == uniqueId);
            if (entry is not null) return entry;
            await Task.Delay(delayMs, cancellationToken);
        }
        return null;
    }

    public static async Task<string?> AlignEntityIdAsync(
        IHassWebSocketClient ws,
        string platform,
        string uniqueId,
        string desiredEntityId,
        CancellationToken cancellationToken)
    {
        var entry = await FindByUniqueIdAsync(ws, platform, uniqueId, cancellationToken);
        if (entry is null) return null;

        await ws.SendCommandAsync(new Dictionary<string, object?>
        {
            ["type"] = EntityRegistryCommands.Update,
            ["entity_id"] = entry.EntityId,
            ["new_entity_id"] = desiredEntityId
        }, cancellationToken);
        return desiredEntityId;
    }
}
