using System.ComponentModel;
using System.Text.Json;
using Haus.Auth;
using Haus.HassClient;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Repair;

public sealed class RepairGetCommand(IAuthService auth, IHassClient client)
    : HausCommand<RepairGetCommand.Settings>(auth)
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
        var issue = await client.Repairs.GetAsync(settings.Domain, settings.IssueId, cancellationToken);
        if (issue is null)
        {
            OutputHelper.WriteError(settings,
                $"No repair issue '{settings.IssueId}' for domain '{settings.Domain}'. " +
                "Run `haus repair list` to see the current issues.");
            return 1;
        }

        var raw = await client.Repairs.GetIssueDataAsync(settings.Domain, settings.IssueId, cancellationToken);
        var issueData = raw.ValueKind == JsonValueKind.Undefined ? null : (JsonElement?)raw;

        OutputHelper.WriteResult(settings, new { issue, issue_data = issueData },
            () => WriteHuman(issue, issueData),
            () => WritePorcelain(issue));

        return 0;
    }

    private static void WriteHuman(RepairIssue issue, JsonElement? issueData)
    {
        AnsiConsole.MarkupLine($"[bold]{(issue.Title ?? issue.LocalizationKey).EscapeMarkup()}[/]");
        if (issue.Description is { } description)
            AnsiConsole.MarkupLine($"[dim]{description.EscapeMarkup()}[/]");
        AnsiConsole.WriteLine();

        var table = new Table().Border(TableBorder.Rounded).AddColumn("Field").AddColumn("Value");
        table.AddRow("Domain", issue.Domain.EscapeMarkup());
        table.AddRow("Issue ID", issue.IssueId.EscapeMarkup());
        table.AddRow("Severity", (issue.Severity ?? "").EscapeMarkup());
        table.AddRow("Fixable", issue.IsFixable ? "[green]yes[/]" : "[dim]no[/]");
        table.AddRow("Ignored", issue.Ignored ? "[yellow]yes[/]" : "[dim]no[/]");
        if (issue.Created is { } created)
            table.AddRow("Created", created.ToLocalTime().ToString("yyyy-MM-dd HH:mm"));
        if (issue.IssueDomain is { } issueDomain)
            table.AddRow("Issue domain", issueDomain.EscapeMarkup());
        if (issue.BreaksInHaVersion is { } breaks)
            table.AddRow("Breaks in", $"[red]{breaks.EscapeMarkup()}[/]");
        if (issue.LearnMoreUrl is { } url)
            table.AddRow("Learn more", url.EscapeMarkup());
        table.AddRow("Translation key", issue.LocalizationKey.EscapeMarkup());
        AnsiConsole.Write(table);

        WriteIssueData(issueData);

        if (issue.IsFixable)
            Console.WriteLine($"Fix with: haus repair fix {issue.Domain} {issue.IssueId}");
    }

    private static void WriteIssueData(JsonElement? issueData)
    {
        if (issueData is not { ValueKind: JsonValueKind.Object } data) return;
        if (!data.EnumerateObject().Any()) return;

        var table = new Table().Border(TableBorder.Rounded)
            .Title("Issue data")
            .AddColumn("Key").AddColumn("Value");
        foreach (var prop in data.EnumerateObject())
        {
            var value = prop.Value.ValueKind switch
            {
                JsonValueKind.String => prop.Value.GetString() ?? "",
                JsonValueKind.Object => $"({prop.Value.EnumerateObject().Count()} keys)",
                JsonValueKind.Array => $"({prop.Value.GetArrayLength()} items)",
                _ => prop.Value.ToString()
            };
            table.AddRow(prop.Name.EscapeMarkup(), value.EscapeMarkup());
        }
        AnsiConsole.Write(table);
    }

    private static void WritePorcelain(RepairIssue issue)
    {
        OutputHelper.WriteKeyValue("domain", issue.Domain);
        OutputHelper.WriteKeyValue("issue_id", issue.IssueId);
        OutputHelper.WriteKeyValue("title", issue.Title ?? issue.LocalizationKey);
        OutputHelper.WriteKeyValue("description", Flatten(issue.Description));
        OutputHelper.WriteKeyValue("severity", issue.Severity ?? "");
        OutputHelper.WriteKeyValue("fixable", issue.IsFixable ? "yes" : "no");
        OutputHelper.WriteKeyValue("ignored", issue.Ignored ? "yes" : "no");
        OutputHelper.WriteKeyValue("created", issue.Created?.ToString("o") ?? "");
        OutputHelper.WriteKeyValue("issue_domain", issue.IssueDomain ?? "");
        OutputHelper.WriteKeyValue("translation_key", issue.LocalizationKey);
        OutputHelper.WriteKeyValue("breaks_in_ha_version", issue.BreaksInHaVersion ?? "");
        OutputHelper.WriteKeyValue("learn_more_url", issue.LearnMoreUrl ?? "");
    }

    private static string Flatten(string? text) =>
        text is null ? "" : string.Join(" ", text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
}
