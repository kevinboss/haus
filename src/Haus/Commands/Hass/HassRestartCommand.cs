using Haus.Auth;
using Haus.HassClient;
using Haus.Output;
using Spectre.Console;

namespace Haus.Commands.Hass;

public sealed class HassRestartCommand(IAuthService auth, IHassClient client)
    : HausCommand<HassRestartCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings;

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        await client.Services.CallAsync("homeassistant", "restart", null, cancellationToken);

        OutputHelper.WriteResult(settings, new { action = "restart" },
            () => AnsiConsole.MarkupLine("[green]Restarting[/] Home Assistant… (the API will be briefly unavailable)"),
            () => Console.WriteLine("restart"));

        return 0;
    }
}
