using System.ComponentModel;
using Haus.Auth;
using Haus.Rest;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.State;

public sealed class StateDeleteCommand(IAuthService auth, IHassApiClient api) : HausCommand<StateDeleteCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<entity_id>")]
        [Description("Entity ID to remove from state machine")]
        public required string EntityId { get; init; }
    }

    protected override async Task<int> RunAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        await api.DeleteAsync($"/api/states/{settings.EntityId}", cancellationToken);

        OutputHelper.WriteResult(settings, new { deleted = settings.EntityId },
            () => AnsiConsole.MarkupLine($"[green]Deleted[/] [bold]{settings.EntityId.EscapeMarkup()}[/]"),
            () => Console.WriteLine(settings.EntityId));

        return 0;
    }
}
