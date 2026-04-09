using System.ComponentModel;
using System.Text.Json;
using Haus.Auth;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands;

public abstract class HausSettings : CommandSettings, IOutputSettings
{
    [CommandOption("--json")]
    [Description("Output as JSON")]
    public bool Json { get; init; }

    [CommandOption("--porcelain")]
    [Description("Plain text output for scripting")]
    public bool Porcelain { get; init; }
}

public abstract class HausCommand<TSettings>(IAuthService auth) : AsyncCommand<TSettings>
    where TSettings : HausSettings
{
    public override async Task<int> ExecuteAsync(CommandContext context, TSettings settings, CancellationToken cancellationToken)
    {
        if (!auth.IsLoggedIn)
        {
            OutputHelper.WriteError(settings, "Not logged in. Run `haus login` or set HASS_URL/HASS_TOKEN.");
            return 1;
        }

        try
        {
            return await RunAsync(context, settings, cancellationToken);
        }
        catch (Exception ex)
        {
            OutputHelper.WriteError(settings, ex);
            return 1;
        }
    }

    protected abstract Task<int> RunAsync(CommandContext context, TSettings settings, CancellationToken cancellationToken);

    protected static Dictionary<string, object>? ParseJsonData(string? json) =>
        json is not null ? JsonSerializer.Deserialize<Dictionary<string, object>>(json) : null;

    protected static ValidationResult ValidateJsonData(string? json)
    {
        if (json is null) return ValidationResult.Success();
        try { JsonDocument.Parse(json); return ValidationResult.Success(); }
        catch (JsonException) { return ValidationResult.Error("--data must be valid JSON."); }
    }
}
