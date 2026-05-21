using Haus.Commands.Entity;
using Haus.Connection;
using Haus.Output;
using Spectre.Console;

namespace Haus.Commands.Helper;

internal static class HelperCreator
{
    public static async Task<int> CreateAsync(
        IHassWebSocketClient ws,
        HelperKind kind,
        string name,
        string? objectId,
        string? icon,
        Dictionary<string, object?> bodyFields,
        HausSettings settings,
        CancellationToken cancellationToken)
    {
        var domain = kind.Domain();
        var payload = new Dictionary<string, object?>(bodyFields)
        {
            ["type"] = HelperCommands.Create(domain),
            ["name"] = name
        };
        if (icon is not null) payload["icon"] = icon;

        var created = await ws.SendCommandAsync(payload, cancellationToken);
        var createdId = created.TryGetProperty("id", out var idEl) ? idEl.GetString() : null;
        if (createdId is null)
        {
            OutputHelper.WriteError(settings, $"Created helper but no ID returned by HA. Response: {created}");
            return 1;
        }

        string? entityId;
        if (objectId is null)
        {
            var entry = await EntityRegistry.FindByUniqueIdAsync(ws, domain, createdId, cancellationToken);
            entityId = entry?.EntityId;
        }
        else
        {
            entityId = await EntityRegistry.AlignEntityIdAsync(ws, domain, createdId, $"{domain}.{objectId}", cancellationToken);
        }

        var finalId = entityId ?? $"{domain}.{createdId}";
        OutputHelper.WriteResult(settings, new { action = "created", kind = kind.ToString().ToLowerInvariant(), entity_id = finalId },
            () => AnsiConsole.MarkupLine($"[green]Created[/] [bold]{finalId.EscapeMarkup()}[/]"),
            () => Console.WriteLine(finalId));

        return 0;
    }
}
