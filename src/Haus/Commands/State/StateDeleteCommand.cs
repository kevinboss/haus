using System.ComponentModel;
using Haus.Connection;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.State;

public sealed class StateDeleteCommand(IHassApiClient api) : HausCommand<StateDeleteCommand.Settings>(api)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<entity_id>")]
        [Description("Entity ID to remove from state machine")]
        public required string EntityId { get; init; }
    }

    protected override async Task<int> RunAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        await Api.DeleteAsync($"/api/states/{settings.EntityId}", cancellationToken);

        OutputHelper.WriteResult(settings.Json, new { deleted = settings.EntityId }, () =>
        {
            AnsiConsole.MarkupLine($"[green]Deleted[/] [bold]{settings.EntityId.EscapeMarkup()}[/]");
        });

        return 0;
    }
}
