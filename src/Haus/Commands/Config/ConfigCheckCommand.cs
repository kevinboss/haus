using Haus.Auth;
using Haus.HassClient;
using Haus.Output;
using Spectre.Console;

namespace Haus.Commands.Config;

public sealed class ConfigCheckCommand(IAuthService auth, IHassClient client) : HausCommand<ConfigCheckCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings;

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var result = await client.Config.CheckAsync(cancellationToken);
        var valid = string.Equals(result.Result, "valid", StringComparison.OrdinalIgnoreCase);

        OutputHelper.WriteResult(settings, result,
            humanOutput: () =>
            {
                if (valid)
                {
                    AnsiConsole.MarkupLine("[green]Configuration is valid.[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]Configuration is invalid.[/]");
                    if (!string.IsNullOrWhiteSpace(result.Errors))
                        AnsiConsole.WriteLine(result.Errors);
                }
            },
            porcelainOutput: () =>
            {
                OutputHelper.WriteKeyValue("result", result.Result ?? "");
                OutputHelper.WriteKeyValue("errors", result.Errors ?? "");
            });

        return valid ? 0 : 1;
    }
}
