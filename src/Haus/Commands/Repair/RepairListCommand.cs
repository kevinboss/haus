using Haus.Auth;
using Haus.HassClient;
using Haus.Output;
using Spectre.Console;

namespace Haus.Commands.Repair;

public sealed class RepairListCommand(IAuthService auth, IHassClient client)
    : HausCommand<RepairListCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings;

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var issues = await client.Repairs.ListAsync(cancellationToken);
        var ordered = issues
            .OrderBy(i => i.Ignored)
            .ThenBy(i => SeverityRank(i.Severity))
            .ThenBy(i => i.Domain, StringComparer.Ordinal)
            .ThenBy(i => i.IssueId, StringComparer.Ordinal)
            .ToList();

        OutputHelper.WriteResult(settings, ordered,
            humanOutput: () => WriteHuman(ordered),
            porcelainOutput: () => WritePorcelain(ordered));

        return 0;
    }

    private static int SeverityRank(string? severity) => severity switch
    {
        "critical" => 0,
        "error" => 1,
        "warning" => 2,
        _ => 3
    };

    private static void WriteHuman(List<RepairIssue> issues)
    {
        if (issues.Count == 0)
        {
            AnsiConsole.MarkupLine("[dim]No repair issues — nothing needs attention.[/]");
            return;
        }

        var breaking = issues.Any(i => i.BreaksInHaVersion is not null);

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Domain")
            .AddColumn("Issue")
            .AddColumn("Severity")
            .AddColumn("Fixable")
            .AddColumn("Ignored");
        if (breaking) table.AddColumn("Breaks In");

        foreach (var i in issues)
        {
            string[] row =
            [
                i.Domain.EscapeMarkup(),
                $"{(i.Title ?? i.LocalizationKey).EscapeMarkup()}\n[dim]{i.IssueId.EscapeMarkup()}[/]",
                SeverityMarkup(i.Severity),
                i.IsFixable ? "[green]yes[/]" : "[dim]no[/]",
                i.Ignored ? "[yellow]yes[/]" : "[dim]no[/]",
                i.BreaksInHaVersion is { } v ? $"[red]{v.EscapeMarkup()}[/]" : "[dim]—[/]"
            ];
            table.AddRow(breaking ? row : row[..^1]);
        }

        AnsiConsole.Write(table);

        var fixable = issues.Count(i => i is { IsFixable: true, Ignored: false });
        AnsiConsole.MarkupLine($"[dim]{issues.Count} issue{(issues.Count == 1 ? "" : "s")}, {fixable} fixable from here (`haus repair fix <domain> <issue_id>`)[/]");
    }

    private static string SeverityMarkup(string? severity) => severity switch
    {
        "critical" or "error" => $"[red]{severity}[/]",
        "warning" => "[yellow]warning[/]",
        _ => $"[dim]{(severity ?? "").EscapeMarkup()}[/]"
    };

    private static void WritePorcelain(List<RepairIssue> issues) =>
        OutputHelper.WriteColumns(
            ["DOMAIN", "ISSUE ID", "TITLE", "SEVERITY", "FIXABLE", "IGNORED", "BREAKS IN"],
            issues.Select(i => new[]
            {
                i.Domain,
                i.IssueId,
                i.Title ?? i.LocalizationKey,
                i.Severity ?? "",
                i.IsFixable ? "yes" : "no",
                i.Ignored ? "yes" : "no",
                i.BreaksInHaVersion ?? ""
            }));
}
