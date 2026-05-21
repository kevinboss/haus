using System.ComponentModel;
using System.Text.Json;
using Haus.Auth;
using Haus.Connection;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Dashboard;

public sealed class DashboardGetCommand(IAuthService auth, IHassWebSocketClient ws)
    : HausCommand<DashboardGetCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<url_path>")]
        [Description("Dashboard URL path ('lovelace' for the default)")]
        public required string UrlPath { get; init; }
    }

    protected override async Task<int> RunAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var entry = await DashboardRegistry.FindByUrlPathAsync(ws, settings.UrlPath, cancellationToken);
        if (entry is null)
        {
            OutputHelper.WriteError(settings, $"No dashboard with url_path '{settings.UrlPath}'.");
            return 1;
        }

        var configPayload = new Dictionary<string, object?> { ["type"] = LovelaceCommands.Config };
        if (!string.Equals(settings.UrlPath, "lovelace", StringComparison.Ordinal))
            configPayload["url_path"] = settings.UrlPath;

        JsonElement config = default;
        var hasConfig = false;
        try
        {
            config = await ws.SendCommandAsync(configPayload, cancellationToken);
            hasConfig = config.ValueKind == JsonValueKind.Object;
        }
        catch (InvalidOperationException)
        {
            // YAML-mode dashboards or empty dashboards may not return a config; keep going with registry data.
        }

        OutputHelper.WriteResult(settings, new { registry = entry, config = hasConfig ? (object)config : null },
            () => WriteHumanOutput(entry, hasConfig ? config : default),
            () => WritePorcelainOutput(entry, hasConfig ? config : default));

        return 0;
    }

    private static void WriteHumanOutput(DashboardRegistryEntry entry, JsonElement config)
    {
        AnsiConsole.MarkupLine($"[bold]{entry.UrlPath.EscapeMarkup()}[/]");
        AnsiConsole.MarkupLine($"[dim]Title:[/]        {entry.Title.EscapeMarkup()}");
        AnsiConsole.MarkupLine($"[dim]Icon:[/]         {(entry.Icon ?? "—").EscapeMarkup()}");
        AnsiConsole.MarkupLine($"[dim]Mode:[/]         {entry.Mode.EscapeMarkup()}");
        AnsiConsole.MarkupLine($"[dim]Sidebar:[/]      {(entry.ShowInSidebar ? "[green]yes[/]" : "[dim]no[/]")}");
        AnsiConsole.MarkupLine($"[dim]Admin only:[/]   {(entry.RequireAdmin ? "[yellow]yes[/]" : "[dim]no[/]")}");
        AnsiConsole.MarkupLine($"[dim]Registry ID:[/]  {entry.Id.EscapeMarkup()}");
        AnsiConsole.WriteLine();

        if (config.ValueKind != JsonValueKind.Object)
        {
            AnsiConsole.MarkupLine("[dim]No view config available (YAML-mode or empty).[/]");
            return;
        }

        var views = config.TryGetProperty("views", out var v) && v.ValueKind == JsonValueKind.Array
            ? v.EnumerateArray().ToList() : [];

        AnsiConsole.MarkupLine($"[bold]Views ({views.Count})[/]");
        if (views.Count == 0)
        {
            AnsiConsole.MarkupLine("[dim]No views.[/]");
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("#")
            .AddColumn("Title")
            .AddColumn("Path")
            .AddColumn(new TableColumn("Cards").RightAligned());

        var i = 0;
        foreach (var view in views)
        {
            var title = view.TryGetProperty("title", out var vt) ? vt.GetString() ?? "" : "";
            var path = view.TryGetProperty("path", out var vp) ? vp.GetString() ?? "" : "";
            var cards = view.TryGetProperty("cards", out var vc) && vc.ValueKind == JsonValueKind.Array
                ? vc.GetArrayLength() : 0;

            table.AddRow((i++).ToString(), title.EscapeMarkup(), path.EscapeMarkup(), cards.ToString());
        }
        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine("[dim]Use `dashboard config get` for the full view config.[/]");
    }

    private static void WritePorcelainOutput(DashboardRegistryEntry entry, JsonElement config)
    {
        OutputHelper.WriteKeyValue("url_path", entry.UrlPath);
        OutputHelper.WriteKeyValue("title", entry.Title);
        OutputHelper.WriteKeyValue("icon", entry.Icon ?? "");
        OutputHelper.WriteKeyValue("mode", entry.Mode);
        OutputHelper.WriteKeyValue("sidebar", entry.ShowInSidebar ? "yes" : "no");
        OutputHelper.WriteKeyValue("admin_only", entry.RequireAdmin ? "yes" : "no");
        OutputHelper.WriteKeyValue("registry_id", entry.Id);

        var views = config.ValueKind == JsonValueKind.Object && config.TryGetProperty("views", out var v) && v.ValueKind == JsonValueKind.Array
            ? v.GetArrayLength() : 0;
        OutputHelper.WriteKeyValue("views", views.ToString());
    }
}
