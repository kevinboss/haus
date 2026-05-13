using System.ComponentModel;
using System.Net;
using System.Text.Json;
using Haus.Auth;
using Haus.Connection;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Automation;

public sealed class AutomationCreateCommand(IAuthService auth, IHassApiClient api)
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

        [CommandOption("--id <ID>")]
        [Description("Config ID for the new automation (default: millisecond timestamp)")]
        public string? Id { get; init; }

        public override ValidationResult Validate() =>
            JsonInput.ValidateRequired(Data, FromFile);
    }

    protected override async Task<int> RunAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var json = JsonInput.Resolve(settings.Data, settings.FromFile)!;
        var config = ParseTyped<AutomationConfig>(json);

        var configId = settings.Id ?? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

        if (await ConfigIdExists(configId, cancellationToken))
        {
            OutputHelper.WriteError(settings, $"Config ID '{configId}' is already in use. Pick a different --id.");
            return 1;
        }

        await api.PostAsync<JsonElement>(
            $"/api/config/automation/config/{configId}", config, cancellationToken);

        OutputHelper.WriteResult(settings, new { action = "created", id = configId },
            () => AnsiConsole.MarkupLine($"[green]Created[/] [bold]{configId.EscapeMarkup()}[/]"),
            () => Console.WriteLine(configId));

        return 0;
    }

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
