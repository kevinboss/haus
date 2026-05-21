using System.ComponentModel;
using System.Text.Json;
using Haus.Auth;
using Haus.Commands.Entity;
using Haus.Rest;
using Haus.Hass;
using Haus.Ws;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Helper;

public sealed class HelperDeleteCommand(IAuthService auth, IHassWebSocketClient ws)
    : HausCommand<HelperDeleteCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<entity_id>")]
        [Description("Helper entity ID (e.g. input_boolean.bedtime_lock)")]
        public required string EntityId { get; init; }
    }

    protected override async Task<int> RunAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var domain = settings.EntityId.Split('.', 2)[0];

        var entry = await ws.GetEntityRegistryEntryAsync(settings.EntityId, cancellationToken);
        if (entry?.UniqueId is null)
        {
            OutputHelper.WriteError(settings, $"Could not resolve unique_id for '{settings.EntityId}'.");
            return 1;
        }

        await ws.DeleteHelperAsync(domain, entry.UniqueId, cancellationToken);

        OutputHelper.WriteResult(settings, new { action = "deleted", entity_id = settings.EntityId },
            () => AnsiConsole.MarkupLine($"[green]Deleted[/] [bold]{settings.EntityId.EscapeMarkup()}[/]"),
            () => Console.WriteLine(settings.EntityId));

        return 0;
    }
}
