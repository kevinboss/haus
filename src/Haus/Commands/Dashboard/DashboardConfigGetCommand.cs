using System.ComponentModel;
using System.Text.Json;
using Haus.Auth;
using Haus.Rest;
using Haus.Hass;
using Haus.Ws;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Dashboard;

public sealed class DashboardConfigGetCommand(IAuthService auth, IHassWebSocketClient ws)
    : HausCommand<DashboardConfigGetCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<url_path>")]
        [Description("Dashboard URL path (e.g. 'lovelace' for the default, or your custom path)")]
        public required string UrlPath { get; init; }
    }

    protected override async Task<int> RunAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var payload = new Dictionary<string, object?> { ["type"] = LovelaceCommands.Config };
        if (!string.Equals(settings.UrlPath, "lovelace", StringComparison.Ordinal))
            payload["url_path"] = settings.UrlPath;

        var config = await ws.SendCommandAsync(payload, cancellationToken);

        OutputHelper.WriteResult(settings, config,
            () => WriteHumanOutput(config, settings.UrlPath),
            () => WritePorcelainOutput(config));

        return 0;
    }

    private static void WriteHumanOutput(JsonElement config, string urlPath)
    {
        var views = config.TryGetProperty("views", out var v) && v.ValueKind == JsonValueKind.Array
            ? v.EnumerateArray().ToList()
            : [];

        AnsiConsole.MarkupLine($"[bold]{urlPath.EscapeMarkup()}[/]");
        if (config.TryGetProperty("title", out var t) && t.ValueKind == JsonValueKind.String)
            AnsiConsole.MarkupLine($"[dim]Title:[/]  {(t.GetString() ?? "").EscapeMarkup()}");
        AnsiConsole.MarkupLine($"[dim]Views:[/]  {views.Count}");
        AnsiConsole.WriteLine();

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
            .AddColumn("Icon")
            .AddColumn(new TableColumn("Cards").RightAligned());

        var i = 0;
        foreach (var view in views)
        {
            var title = view.TryGetProperty("title", out var vt) ? vt.GetString() ?? "" : "";
            var path = view.TryGetProperty("path", out var vp) ? vp.GetString() ?? "" : "";
            var icon = view.TryGetProperty("icon", out var vi) ? vi.GetString() ?? "" : "";
            var cards = view.TryGetProperty("cards", out var vc) && vc.ValueKind == JsonValueKind.Array
                ? vc.GetArrayLength() : 0;

            table.AddRow(
                (i++).ToString(),
                title.EscapeMarkup(),
                path.EscapeMarkup(),
                icon.EscapeMarkup(),
                cards.ToString());
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine("[dim]Use --json to view the full config.[/]");
    }

    private static void WritePorcelainOutput(JsonElement config)
    {
        var views = config.TryGetProperty("views", out var v) && v.ValueKind == JsonValueKind.Array
            ? v.EnumerateArray().ToList()
            : [];

        var i = 0;
        OutputHelper.WriteColumns(
            ["INDEX", "TITLE", "PATH", "ICON", "CARDS"],
            views.Select(view => new[]
            {
                (i++).ToString(),
                view.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "",
                view.TryGetProperty("path", out var p) ? p.GetString() ?? "" : "",
                view.TryGetProperty("icon", out var ic) ? ic.GetString() ?? "" : "",
                (view.TryGetProperty("cards", out var c) && c.ValueKind == JsonValueKind.Array ? c.GetArrayLength() : 0).ToString()
            }));
    }
}
