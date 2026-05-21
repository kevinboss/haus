using System.Text.Json;
using Haus.Auth;
using Haus.Rest;
using Haus.Hass;
using Haus.Ws;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Dashboard;

public sealed class DashboardListCommand(IAuthService auth, IHassWebSocketClient ws) : HausCommand<DashboardListCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings;

    protected override async Task<int> RunAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var raw = await ws.SendCommandAsync(new Dictionary<string, object?> { ["type"] = LovelaceCommands.DashboardsList }, cancellationToken);
        var entries = raw.Deserialize<List<DashboardRegistryEntry>>(HassJsonOptions.Default) ?? [];

        OutputHelper.WriteResult(settings, entries,
            () => WriteHumanOutput(entries),
            () => WritePorcelainOutput(entries));

        return 0;
    }

    private static void WriteHumanOutput(List<DashboardRegistryEntry> entries)
    {
        if (entries.Count == 0)
        {
            AnsiConsole.MarkupLine("[dim]No dashboards.[/]");
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("URL path")
            .AddColumn("Title")
            .AddColumn("Icon")
            .AddColumn("Mode")
            .AddColumn("Sidebar")
            .AddColumn("Admin only");

        foreach (var e in entries)
        {
            table.AddRow(
                e.UrlPath.EscapeMarkup(),
                e.Title.EscapeMarkup(),
                (e.Icon ?? "").EscapeMarkup(),
                e.Mode.EscapeMarkup(),
                e.ShowInSidebar ? "[green]yes[/]" : "[dim]no[/]",
                e.RequireAdmin ? "[yellow]yes[/]" : "[dim]no[/]");
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"[dim]{entries.Count} dashboard{(entries.Count == 1 ? "" : "s")}[/]");
    }

    private static void WritePorcelainOutput(List<DashboardRegistryEntry> entries)
    {
        OutputHelper.WriteColumns(
            ["URL_PATH", "TITLE", "ICON", "MODE", "SIDEBAR", "ADMIN_ONLY"],
            entries.Select(e => new[]
            {
                e.UrlPath,
                e.Title,
                e.Icon ?? "",
                e.Mode,
                e.ShowInSidebar ? "yes" : "no",
                e.RequireAdmin ? "yes" : "no"
            }));
    }
}
