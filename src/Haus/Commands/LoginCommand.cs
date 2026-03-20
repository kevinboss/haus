using System.ComponentModel;
using Haus.Auth;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands;

public sealed class LoginCommand(IAuthService authService) : AsyncCommand<LoginCommand.Settings>
{
    private const string DefaultUrl = "http://homeassistant.local:8123";
    private const string EnvVarUrl = "HASS_URL";

    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "[url]")]
        [Description("Home Assistant URL (default: HASS_URL env var or http://homeassistant.local:8123)")]
        public string? Url { get; init; }

        [CommandOption("--json")]
        [Description("Output as JSON")]
        public bool Json { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var url = settings.Url
            ?? Environment.GetEnvironmentVariable(EnvVarUrl)
            ?? DefaultUrl;

        if (!settings.Json)
            AnsiConsole.MarkupLine($"[dim]Logging in to[/] [blue]{url.EscapeMarkup()}[/][dim]...[/]");

        try
        {
            await authService.LoginAsync(url, cancellationToken);
            OutputHelper.WriteResult(settings.Json, new { status = "ok", url }, () =>
                AnsiConsole.MarkupLine("[green]Login successful![/] Token saved."));
            return 0;
        }
        catch (Exception ex)
        {
            OutputHelper.WriteError(settings.Json, ex);
            return 1;
        }
    }
}
