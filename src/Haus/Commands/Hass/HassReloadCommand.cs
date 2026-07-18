using System.ComponentModel;
using Haus.Auth;
using Haus.HassClient;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Hass;

public sealed class HassReloadCommand(IAuthService auth, IHassClient client)
    : HausCommand<HassReloadCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<domain>")]
        [Description("What to reload: a domain (automation, script, scene, template, group, input_boolean, ...), 'all', or 'core'")]
        public required string Domain { get; init; }
    }

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        // Most domains expose <domain>.reload; 'all' and core config have dedicated homeassistant services.
        var (domain, service) = settings.Domain.ToLowerInvariant() switch
        {
            "all" => ("homeassistant", "reload_all"),
            "core" => ("homeassistant", "reload_core_config"),
            var d => (d, "reload")
        };

        await client.Services.CallAsync(domain, service, null, cancellationToken);

        OutputHelper.WriteResult(settings, new { action = "reloaded", domain = settings.Domain },
            () => AnsiConsole.MarkupLine($"[green]Reloaded[/] [bold]{settings.Domain.EscapeMarkup()}[/]"),
            () => Console.WriteLine(settings.Domain));

        return 0;
    }
}
