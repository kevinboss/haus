using System.ComponentModel;
using Haus.Auth;
using Haus.Ws;
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

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        await ws.UpdateEntityRegistryEntryAsync(settings.EntityId, new(Name: settings.Name), cancellationToken);

        OutputHelper.WriteResult(settings, new { action = "renamed", entity_id = settings.EntityId, name = settings.Name },
            () => AnsiConsole.MarkupLine($"[green]Renamed[/] [bold]{settings.EntityId.EscapeMarkup()}[/] to [bold]{settings.Name.EscapeMarkup()}[/]"),
            () => Console.WriteLine($"{settings.EntityId}\t{settings.Name}"));

        return 0;
    }
}
