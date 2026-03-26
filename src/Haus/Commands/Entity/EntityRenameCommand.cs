using System.ComponentModel;
using Haus.Auth;
using Haus.Connection;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Entity;

public sealed class EntityRenameCommand(IAuthService auth, IHassWebSocketClient ws)
    : HausCommand<EntityRenameCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<entity_id>")]
        [Description("Entity ID to rename (e.g. sensor.living_room_temperature)")]
        public required string EntityId { get; init; }

        [CommandArgument(1, "<name>")]
        [Description("New display name")]
        public required string Name { get; init; }
    }

    protected override async Task<int> RunAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var result = await ws.SendCommandAsync(new
        {
            type = "config/entity_registry/update",
            entity_id = settings.EntityId,
            name = settings.Name
        }, cancellationToken);

        OutputHelper.WriteResult(settings.Json, result, () =>
        {
            AnsiConsole.MarkupLine($"[green]Renamed[/] [bold]{settings.EntityId.EscapeMarkup()}[/] to [bold]{settings.Name.EscapeMarkup()}[/]");
        });

        return 0;
    }
}
