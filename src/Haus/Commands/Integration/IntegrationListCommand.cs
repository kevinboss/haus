using Haus.HassClient;
using Haus.Auth;
using Haus.Output;
using Spectre.Console;

namespace Haus.Commands.Integration;

public sealed class IntegrationListCommand(IAuthService auth, IHassClient client)
    : HausCommand<IntegrationListCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings;

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var entries = await client.Integration.ListAsync(cancellationToken);
        var ordered = entries
            .OrderBy(e => e.Domain, StringComparer.Ordinal)
            .ThenBy(e => e.Title, StringComparer.Ordinal)
            .ToList();

        OutputHelper.WriteResult(settings, ordered,
            humanOutput: () => WriteHuman(ordered),
            porcelainOutput: () => WritePorcelain(ordered));

        return 0;
    }

    private static void WriteHuman(List<ConfigEntry> entries)
    {
        if (entries.Count == 0)
        {
            AnsiConsole.MarkupLine("[dim]No integration config entries.[/]");
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Entry ID")
            .AddColumn("Domain")
            .AddColumn("Title")
            .AddColumn("State")
            .AddColumn("Options");

        foreach (var e in entries)
        {
            var stateMarkup = e.State switch
            {
                "loaded" => "[green]loaded[/]",
                "not_loaded" => "[yellow]not_loaded[/]",
                "setup_error" or "setup_retry" => $"[red]{e.State}[/]",
                _ => (e.State ?? "").EscapeMarkup()
            };
            table.AddRow(
                e.EntryId.EscapeMarkup(),
                e.Domain.EscapeMarkup(),
                e.Title.EscapeMarkup(),
                stateMarkup,
                e.SupportsOptions ? "[green]yes[/]" : "[dim]no[/]");
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"[dim]{entries.Count} entr{(entries.Count == 1 ? "y" : "ies")}[/]");
    }

    private static void WritePorcelain(List<ConfigEntry> entries) =>
        OutputHelper.WriteColumns(
            ["ENTRY ID", "DOMAIN", "TITLE", "STATE", "OPTIONS"],
            entries.Select(e => new[]
            {
                e.EntryId,
                e.Domain,
                e.Title,
                e.State ?? "",
                e.SupportsOptions ? "yes" : "no"
            }));
}
