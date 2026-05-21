using System.ComponentModel;
using Haus.HassClient;
using Haus.Auth;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Entity;

public sealed class EntityDeleteCommand(IAuthService auth, IHassClient client)
    : HausCommand<EntityDeleteCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<entity_id>")]
        [Description("Entity ID to remove from the registry")]
        public required string EntityId { get; init; }
    }

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        await client.EntityRegistry.RemoveAsync(settings.EntityId, cancellationToken);

        OutputHelper.WriteResult(settings, new { action = "removed", id = settings.EntityId },
            () => AnsiConsole.MarkupLine($"[green]Removed[/] [bold]{settings.EntityId.EscapeMarkup()}[/]"),
            () => Console.WriteLine(settings.EntityId));

        return 0;
    }
}
