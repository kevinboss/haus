using Haus.HassClient;
using Haus.Output;
using Spectre.Console;

namespace Haus.Commands.Helper;

internal static class HelperCreator
{
    public static async Task<int> CreateAsync(
        IHassClient client,
        HelperKind kind,
        string name,
        string? objectId,
        string? icon,
        Dictionary<string, object?> bodyFields,
        HausSettings settings,
        CancellationToken cancellationToken)
    {
        var domain = kind.Domain();
        var fields = new Dictionary<string, object?>(bodyFields) { ["name"] = name };
        if (icon is not null) fields["icon"] = icon;

        var created = await client.Helper.CreateAsync(domain, fields, cancellationToken);
        var createdId = created.TryGetProperty("id", out var idEl) ? idEl.GetString() : null;
        if (createdId is null)
        {
            OutputHelper.WriteError(settings, $"Created helper but no ID returned by HA. Response: {created}");
            return 1;
        }

        var entry = await FindByUniqueIdAsync(client, domain, createdId, cancellationToken);
        string? entityId = entry?.EntityId;
        if (objectId is not null && entry is not null)
        {
            var desiredEntityId = $"{domain}.{objectId}";
            await client.EntityRegistry.UpdateAsync(entry.EntityId, new(NewEntityId: desiredEntityId), cancellationToken);
            entityId = desiredEntityId;
        }

        var finalId = entityId ?? $"{domain}.{createdId}";
        OutputHelper.WriteResult(settings, new { action = "created", kind = kind.ToString().ToLowerInvariant(), entity_id = finalId },
            () => AnsiConsole.MarkupLine($"[green]Created[/] [bold]{finalId.EscapeMarkup()}[/]"),
            () => Console.WriteLine(finalId));

        return 0;
    }

    private static async Task<EntityRegistryEntry?> FindByUniqueIdAsync(
        IHassClient client,
        string platform,
        string uniqueId,
        CancellationToken cancellationToken,
        int attempts = 10,
        int delayMs = 200)
    {
        for (var attempt = 0; attempt < attempts; attempt++)
        {
            var entries = await client.EntityRegistry.ListAsync(cancellationToken);
            var entry = entries.SingleOrDefault(e => e.Platform == platform && e.UniqueId == uniqueId);
            if (entry is not null) return entry;
            await Task.Delay(delayMs, cancellationToken);
        }
        return null;
    }
}
