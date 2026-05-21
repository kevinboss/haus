using System.ComponentModel;
using Haus.Auth;
using Haus.Rest;
using Haus.Hass;
using Haus.Ws;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Dashboard;

public sealed class DashboardUpdateCommand(IAuthService auth, IHassWebSocketClient ws)
    : HausCommand<DashboardUpdateCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<url_path>")]
        [Description("Dashboard URL path to update")]
        public required string UrlPath { get; init; }

        [CommandOption("--title <TITLE>")]
        [Description("New display title")]
        public string? Title { get; init; }

        [CommandOption("--icon <ICON>")]
        [Description("New MDI icon (pass empty string to clear)")]
        public string? Icon { get; init; }

        [CommandOption("--show-in-sidebar")]
        public bool? ShowInSidebar { get; init; }

        [CommandOption("--hide-from-sidebar")]
        public bool HideFromSidebar { get; init; }

        [CommandOption("--require-admin")]
        public bool? RequireAdmin { get; init; }

        [CommandOption("--allow-non-admin")]
        public bool AllowNonAdmin { get; init; }

        public override ValidationResult Validate()
        {
            if (ShowInSidebar is true && HideFromSidebar)
                return ValidationResult.Error("--show-in-sidebar and --hide-from-sidebar are mutually exclusive.");
            if (RequireAdmin is true && AllowNonAdmin)
                return ValidationResult.Error("--require-admin and --allow-non-admin are mutually exclusive.");

            var anySet = Title is not null || Icon is not null || ShowInSidebar is not null || HideFromSidebar || RequireAdmin is not null || AllowNonAdmin;
            return anySet
                ? ValidationResult.Success()
                : ValidationResult.Error("At least one field is required (--title, --icon, --show-in-sidebar/--hide-from-sidebar, --require-admin/--allow-non-admin).");
        }
    }

    protected override async Task<int> RunAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var dashboards = await ws.ListDashboardsAsync(cancellationToken);
        var entry = dashboards.FirstOrDefault(d => d.UrlPath == settings.UrlPath);
        if (entry is null)
        {
            OutputHelper.WriteError(settings, $"No dashboard with url_path '{settings.UrlPath}'.");
            return 1;
        }
        if (!string.Equals(entry.Mode, "storage", StringComparison.Ordinal))
        {
            OutputHelper.WriteError(settings, $"Dashboard '{settings.UrlPath}' is in '{entry.Mode}' mode; only storage-mode dashboards are editable.");
            return 1;
        }

        bool? showInSidebar = settings.HideFromSidebar ? false : settings.ShowInSidebar;
        bool? requireAdmin = settings.AllowNonAdmin ? false : settings.RequireAdmin;

        await ws.UpdateDashboardAsync(
            entry.Id,
            new DashboardUpdate(
                Title: settings.Title,
                Icon: settings.Icon,
                ShowInSidebar: showInSidebar,
                RequireAdmin: requireAdmin),
            cancellationToken);

        OutputHelper.WriteResult(settings, new { action = "updated", url_path = settings.UrlPath },
            () => AnsiConsole.MarkupLine($"[green]Updated[/] [bold]{settings.UrlPath.EscapeMarkup()}[/]"),
            () => Console.WriteLine(settings.UrlPath));

        return 0;
    }
}
