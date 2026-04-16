using Haus.Auth;
using Haus.Connection;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Update;

public sealed class UpdateListCommand(IAuthService auth, IHassApiClient api) : HausCommand<UpdateListCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings;

    protected override async Task<int> RunAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var states = await api.GetAsync<List<UpdateState>>("/api/states", cancellationToken);
        var updates = states
            .Where(s => s.EntityId.StartsWith("update.", StringComparison.Ordinal))
            .OrderByDescending(s => IsAvailable(s))
            .ThenBy(s => s.EntityId)
            .ToList();

        OutputHelper.WriteResult(settings, updates,
            () => WriteHumanOutput(updates),
            () => WritePorcelainOutput(updates));

        return 0;
    }

    private static bool IsAvailable(UpdateState s) =>
        string.Equals(s.State, "on", StringComparison.OrdinalIgnoreCase);

    private static string StatusLabel(UpdateState s)
    {
        if (s.Attributes.InProgress) return "installing";
        if (IsAvailable(s))
        {
            if (!string.IsNullOrEmpty(s.Attributes.SkippedVersion) &&
                s.Attributes.SkippedVersion == s.Attributes.LatestVersion)
                return "skipped";
            return "available";
        }
        return "up to date";
    }

    private static string FormatVersion(UpdateState s)
    {
        var installed = s.Attributes.InstalledVersion ?? "";
        var latest = s.Attributes.LatestVersion ?? "";
        if (IsAvailable(s) && installed != latest)
            return $"{installed} → {latest}";
        return installed;
    }

    private static string StatusMarkup(UpdateState s) => StatusLabel(s) switch
    {
        "available" => "[yellow]available[/]",
        "installing" => "[cyan]installing[/]",
        "skipped" => "[dim]skipped[/]",
        _ => "[green]up to date[/]"
    };

    private static void WriteHumanOutput(List<UpdateState> updates)
    {
        if (updates.Count == 0)
        {
            AnsiConsole.MarkupLine("[dim]No update entities found.[/]");
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Entity ID")
            .AddColumn("Name")
            .AddColumn("Version")
            .AddColumn("Status");

        foreach (var u in updates)
        {
            table.AddRow(
                u.EntityId.EscapeMarkup(),
                (u.Attributes.Title ?? u.Attributes.FriendlyName ?? "").EscapeMarkup(),
                FormatVersion(u).EscapeMarkup(),
                StatusMarkup(u));
        }

        AnsiConsole.Write(table);

        var available = updates.Count(IsAvailable);
        AnsiConsole.MarkupLine(available == 0
            ? $"[dim]{updates.Count} update entities — all up to date[/]"
            : $"[yellow]{available}[/] [dim]of {updates.Count} update{(updates.Count == 1 ? "" : "s")} available[/]");
    }

    private static void WritePorcelainOutput(List<UpdateState> updates)
    {
        OutputHelper.WriteColumns(
            ["ENTITY ID", "TITLE", "INSTALLED", "LATEST", "STATUS"],
            updates.Select(u => new[]
            {
                u.EntityId,
                u.Attributes.Title ?? u.Attributes.FriendlyName ?? "",
                u.Attributes.InstalledVersion ?? "",
                u.Attributes.LatestVersion ?? "",
                StatusLabel(u)
            }));
    }
}
