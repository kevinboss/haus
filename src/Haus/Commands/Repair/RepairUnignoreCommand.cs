using System.ComponentModel;
using Haus.Auth;
using Haus.HassClient;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Repair;

public sealed class RepairUnignoreCommand(IAuthService auth, IHassClient client)
    : HausCommand<RepairUnignoreCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<domain>")]
        [Description("Integration domain that raised the issue (from `haus repair list`)")]
        public required string Domain { get; init; }

        [CommandArgument(1, "<issue_id>")]
        [Description("Issue ID (from `haus repair list`)")]
        public required string IssueId { get; init; }
    }

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        await client.Repairs.SetIgnoredAsync(settings.Domain, settings.IssueId, false, cancellationToken);

        OutputHelper.WriteResult(settings,
            new { action = "unignored", domain = settings.Domain, issue_id = settings.IssueId },
            () => AnsiConsole.MarkupLine(
                $"[green]Unignored[/] [bold]{settings.Domain.EscapeMarkup()}/{settings.IssueId.EscapeMarkup()}[/]"),
            () => OutputHelper.WriteKeyValue(settings.Domain, settings.IssueId));

        return 0;
    }
}
