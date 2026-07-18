using Haus.Auth;
using Haus.HassClient;
using Haus.Output;
using Spectre.Console;

namespace Haus.Commands.Hass;

public sealed class HassStopCommand(IAuthService auth, IHassClient client)
    : HausCommand<HassStopCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings;

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        await client.Services.CallAsync("homeassistant", "stop", null, cancellationToken);

        OutputHelper.WriteResult(settings, new { action = "stop" },
            () => AnsiConsole.MarkupLine("[green]Stopping[/] Home Assistant… (a supervisor/OS install will restart it)"),
            () => Console.WriteLine("stop"));

        return 0;
    }
}
