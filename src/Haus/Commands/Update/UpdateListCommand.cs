using Haus.Auth;
using Haus.Rest;
using Haus.Output;
using Spectre.Console;

namespace Haus.Commands.Update;

public sealed class UpdateListCommand(IAuthService auth, IHassApiClient api) : HausCommand<UpdateListCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings;

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var states = await api.GetAsync<List<UpdateState>>("/api/states", cancellationToken);
        var updates = states
            .Where(s => s.EntityId.StartsWith("update.", StringComparison.Ordinal))
            .OrderByDescending(IsAvailable)
            .ThenBy(s => s.EntityId)
            .ToList();

        OutputHelper.WriteResult(settings, updates,
            () => WriteHumanOutput(updates),
            () => WritePorcelainOutput(updates));

        return 0;
    }

    private enum Status { UpToDate, Available, AvailableReadOnly, Installing, Skipped }

    private static bool IsAvailable(UpdateState s) =>
        string.Equals(s.State, "on", StringComparison.OrdinalIgnoreCase);

    private static Status ClassifyStatus(UpdateState s)
    {
        if (s.Attributes.InProgress) return Status.Installing;
        if (!IsAvailable(s)) return Status.UpToDate;
        if (!string.IsNullOrEmpty(s.Attributes.SkippedVersion) &&
            s.Attributes.SkippedVersion == s.Attributes.LatestVersion)
            return Status.Skipped;
        return (s.Attributes.SupportedFeatures & UpdateEntityFeature.Install) == 0
            ? Status.AvailableReadOnly
            : Status.Available;
    }

    private static string FormatVersion(UpdateState s)
    {
        var installed = s.Attributes.InstalledVersion ?? "";
        var latest = s.Attributes.LatestVersion ?? "";
        if (IsAvailable(s) && installed != latest)
            return $"{installed} → {latest}";
        return installed;
    }

    private static string PorcelainLabel(Status s) => s switch
    {
        Status.UpToDate => "up to date",
        Status.Available => "available",
        Status.AvailableReadOnly => "available (read-only)",
        Status.Installing => "installing",
        Status.Skipped => "skipped",
        _ => throw new ArgumentOutOfRangeException(nameof(s), s, null)
    };

    private static string HumanMarkup(Status s) => s switch
    {
        Status.UpToDate => "[green]up to date[/]",
        Status.Available => "[yellow]available[/]",
        Status.AvailableReadOnly => "[yellow]available[/] [dim](read-only)[/]",
        Status.Installing => "[cyan]installing[/]",
        Status.Skipped => "[dim]skipped[/]",
        _ => throw new ArgumentOutOfRangeException(nameof(s), s, null)
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
                HumanMarkup(ClassifyStatus(u)));
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
                PorcelainLabel(ClassifyStatus(u))
            }));
    }
}
