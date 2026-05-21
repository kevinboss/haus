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

public sealed class DashboardCreateCommand(IAuthService auth, IHassWebSocketClient ws)
    : HausCommand<DashboardCreateCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandOption("--url-path <PATH>")]
        [Description("URL path for the new dashboard (e.g. 'kitchen-tablet'; must be unique)")]
        public required string UrlPath { get; init; }

        [CommandOption("--title <TITLE>")]
        [Description("Display title shown in the sidebar")]
        public required string Title { get; init; }

        [CommandOption("--icon <ICON>")]
        [Description("MDI icon (e.g. mdi:tablet)")]
        public string? Icon { get; init; }

        [CommandOption("--show-in-sidebar")]
        [Description("Show this dashboard in the sidebar (default: true)")]
        public bool? ShowInSidebar { get; init; }

        [CommandOption("--require-admin")]
        [Description("Restrict access to admin users")]
        public bool RequireAdmin { get; init; }
    }

    protected override async Task<int> RunAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var payload = new Dictionary<string, object?>
        {
            ["type"] = LovelaceCommands.DashboardsCreate,
            ["url_path"] = settings.UrlPath,
            ["title"] = settings.Title,
            ["mode"] = "storage",
            ["require_admin"] = settings.RequireAdmin,
            ["show_in_sidebar"] = settings.ShowInSidebar ?? true
        };
        if (settings.Icon is not null) payload["icon"] = settings.Icon;

        var result = await ws.SendCommandAsync(payload, cancellationToken);
        var created = result.Deserialize<DashboardRegistryEntry>(HassJsonOptions.Default);

        OutputHelper.WriteResult(settings, created ?? (object)result,
            () => AnsiConsole.MarkupLine(
                $"[green]Created[/] [bold]{settings.UrlPath.EscapeMarkup()}[/] — \"{settings.Title.EscapeMarkup()}\""),
            () => Console.WriteLine(settings.UrlPath));

        return 0;
    }
}
