using System.ComponentModel;
using Haus.Auth;
using Haus.Connection;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Entity;

public sealed class EntityDeleteCommand(IAuthService auth, IHassWebSocketClient ws)
    : HausCommand<EntityDeleteCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<entity_id>")]
        [Description("Entity ID to remove from the registry")]
        public required string EntityId { get; init; }
    }

    protected override async Task<int> RunAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        await ws.SendCommandAsync(new
        {
            type = EntityRegistryCommands.Remove,
            entity_id = settings.EntityId
        }, cancellationToken);

        OutputHelper.WriteResult(settings, new { action = "removed", id = settings.EntityId },
            () => AnsiConsole.MarkupLine($"[green]Removed[/] [bold]{settings.EntityId.EscapeMarkup()}[/]"),
            () => Console.WriteLine(settings.EntityId));

        return 0;
    }
}
