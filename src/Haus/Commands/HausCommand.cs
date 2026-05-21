using System.ComponentModel;
using System.Text.Json;
using Haus.Auth;
using Haus.Hass;
using Haus.Output;
using JetBrains.Annotations;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors | ImplicitUseTargetFlags.Members)]
public abstract class HausSettings : CommandSettings, IOutputSettings
{
    [CommandOption("--json")]
    [Description("Output as JSON")]
    public bool Json { get; init; }

    [CommandOption("--porcelain")]
    [Description("Plain text output for scripting")]
    public bool Porcelain { get; init; }
}

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public abstract class HausCommand<TSettings>(IAuthService auth) : AsyncCommand<TSettings>
    where TSettings : HausSettings
{
    protected override async Task<int> ExecuteAsync(CommandContext context, TSettings settings, CancellationToken cancellationToken)
    {
        if (!auth.IsLoggedIn)
        {
            OutputHelper.WriteError(settings, "Not logged in. Run `haus login` or set HASS_URL/HASS_TOKEN.");
            return 1;
        }

        try
        {
            if (settings.Json || settings.Porcelain || !AnsiConsole.Profile.Capabilities.Interactive)
                return await RunAsync(settings, cancellationToken);

            var exitCode = 0;
            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync("Running...", async _ =>
                {
                    exitCode = await RunAsync(settings, cancellationToken);
                });
            return exitCode;
        }
        catch (Exception ex)
        {
            OutputHelper.WriteError(settings, ex);
            return 1;
        }
    }

    protected abstract Task<int> RunAsync(TSettings settings, CancellationToken cancellationToken);

    protected static Dictionary<string, object>? ParseJsonData(string? json) =>
        json is not null ? JsonSerializer.Deserialize<Dictionary<string, object>>(json) : null;

    protected static T ParseTyped<T>(string json) where T : class
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, HassJsonOptions.Default)
                ?? throw new InvalidOperationException($"--data deserialized to null for {typeof(T).Name}.");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"--data does not match the expected {typeof(T).Name} shape: {ex.Message}", ex);
        }
    }

    protected static ValidationResult ValidateJsonData(string? json)
    {
        if (json is null) return ValidationResult.Success();
        try { JsonDocument.Parse(json); return ValidationResult.Success(); }
        catch (JsonException) { return ValidationResult.Error("--data must be valid JSON."); }
    }
}
