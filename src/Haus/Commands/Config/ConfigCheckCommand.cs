using Haus.Auth;
using Haus.Rest;
using Haus.Output;
using Spectre.Console;

namespace Haus.Commands.Config;

public sealed class ConfigCheckCommand(IAuthService auth, IHassApiClient api) : HausCommand<ConfigCheckCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings;

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var result = await api.CheckConfigAsync(cancellationToken);
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
