using System.ComponentModel;
using Haus.HassClient;
using Haus.Auth;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Integration;

public sealed class IntegrationDisableCommand(IAuthService auth, IHassClient client)
    : HausCommand<IntegrationDisableCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<entry_id>")]
        [Description("Config entry ID (from `haus integration list`)")]
        public required string EntryId { get; init; }
    }

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var result = await client.Integration.SetEnabledAsync(settings.EntryId, false, cancellationToken);

        OutputHelper.WriteResult(settings,
            new { action = "disabled", entry_id = settings.EntryId, require_restart = result.RequireRestart },
            () =>
            {
                AnsiConsole.MarkupLine($"[green]Disabled[/] [bold]{settings.EntryId.EscapeMarkup()}[/]");
                if (result.RequireRestart)
                    AnsiConsole.MarkupLine("[yellow]A Home Assistant restart is required to finish.[/]");
            },
            () => Console.WriteLine(settings.EntryId));

        return 0;
    }
}
