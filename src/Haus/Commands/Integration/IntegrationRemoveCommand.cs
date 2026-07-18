using System.ComponentModel;
using Haus.HassClient;
using Haus.Auth;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Integration;

public sealed class IntegrationRemoveCommand(IAuthService auth, IHassClient client)
    : HausCommand<IntegrationRemoveCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<entry_id>")]
        [Description("Config entry ID to uninstall (from `haus integration list`)")]
        public required string EntryId { get; init; }
    }

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var result = await client.Integration.RemoveAsync(settings.EntryId, cancellationToken);

        OutputHelper.WriteResult(settings,
            new { action = "removed", entry_id = settings.EntryId, require_restart = result.RequireRestart },
            () =>
            {
                AnsiConsole.MarkupLine($"[green]Removed[/] [bold]{settings.EntryId.EscapeMarkup()}[/]");
                if (result.RequireRestart)
                    AnsiConsole.MarkupLine("[yellow]A Home Assistant restart is required to finish.[/]");
            },
            () => Console.WriteLine(settings.EntryId));

        return 0;
    }
}
