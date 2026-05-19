using System.ComponentModel;
using System.Net;
using System.Text.Json;
using Haus.Auth;
using Haus.Commands.Entity;
using Haus.Connection;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Automation;

public sealed class AutomationCreateCommand(IAuthService auth, IHassApiClient api, IHassWebSocketClient ws)
    : HausCommand<AutomationCreateCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandOption("--data <JSON>")]
        [Description("Full automation configuration as JSON (alias, triggers, actions, ...)")]
        public string? Data { get; init; }

        [CommandOption("--from-file <PATH>")]
        [Description("Read configuration JSON from a file (use --from-file=- for stdin)")]
        public string? FromFile { get; init; }

        [CommandOption("--config-id <ID>")]
        [Description("Config ID for the new automation (default: millisecond timestamp)")]
        public string? ConfigId { get; init; }

        public override ValidationResult Validate() =>
            JsonInput.ValidateRequired(Data, FromFile);
    }

    protected override async Task<int> RunAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var json = TextInput.Resolve(settings.Data, settings.FromFile)!;
        var config = ParseTyped<AutomationConfig>(json);

        var configIdProvided = settings.ConfigId is not null;
        var configId = settings.ConfigId ?? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

        if (await ConfigIdExists(configId, cancellationToken))
        {
            OutputHelper.WriteError(settings, $"Config ID '{configId}' is already in use. Pick a different --config-id.");
            return 1;
        }

        await api.PostAsync<JsonElement>(
            $"/api/config/automation/config/{configId}", config, cancellationToken);

        var entityId = configIdProvided
            ? await AlignConfigIdAsync(configId, cancellationToken)
            : null;

        OutputHelper.WriteResult(settings, new { action = "created", id = configId, entity_id = entityId },
            () =>
            {
                AnsiConsole.MarkupLine($"[green]Created[/] [bold]{configId.EscapeMarkup()}[/]");
                if (entityId is not null)
                    AnsiConsole.MarkupLine($"[dim]Entity:[/] [bold]{entityId.EscapeMarkup()}[/]");
            },
            () => Console.WriteLine(entityId is null ? configId : $"{configId}\t{entityId}"));

        return 0;
    }

    private Task<string?> AlignConfigIdAsync(string configId, CancellationToken cancellationToken) =>
        EntityRegistry.AlignEntityIdAsync(ws, platform: "automation", uniqueId: configId, desiredEntityId: $"automation.{configId}", cancellationToken);

    private async Task<bool> ConfigIdExists(string configId, CancellationToken cancellationToken)
    {
        try
        {
            await api.GetAsync<JsonElement>($"/api/config/automation/config/{configId}", cancellationToken);
            return true;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }
}
