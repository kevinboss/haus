using System.ComponentModel;
using Haus.Auth;
using Haus.Ws;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Dashboard;

public sealed class DashboardDeleteCommand(IAuthService auth, IHassWebSocketClient ws)
    : HausCommand<DashboardDeleteCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<url_path>")]
        [Description("Dashboard URL path to delete")]
        public required string UrlPath { get; init; }
    }

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var dashboards = await ws.ListDashboardsAsync(cancellationToken);
        var entry = dashboards.FirstOrDefault(d => d.UrlPath == settings.UrlPath);
        if (entry is null)
        {
            OutputHelper.WriteError(settings, $"No dashboard with url_path '{settings.UrlPath}'.");
            return 1;
        }

        await ws.DeleteDashboardAsync(entry.Id, cancellationToken);

        OutputHelper.WriteResult(settings, new { action = "deleted", url_path = settings.UrlPath },
            () => AnsiConsole.MarkupLine($"[green]Deleted[/] [bold]{settings.UrlPath.EscapeMarkup()}[/]"),
            () => Console.WriteLine(settings.UrlPath));

        return 0;
    }
}
