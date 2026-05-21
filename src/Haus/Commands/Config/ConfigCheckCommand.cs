using System.Text.Json.Serialization;
using Haus.Auth;
using Haus.Rest;
using Haus.Output;
using Spectre.Console;
using JetBrains.Annotations;

namespace Haus.Commands.Config;

public sealed class ConfigCheckCommand(IAuthService auth, IHassApiClient api) : HausCommand<ConfigCheckCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings;

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var result = await api.PostAsync<ConfigCheckResult>("/api/config/core/check_config", null, cancellationToken);
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

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ConfigCheckResult(
    [property: JsonPropertyName("result")] string? Result,
    [property: JsonPropertyName("errors")] string? Errors);
