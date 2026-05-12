using System.ComponentModel;
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
        public required string Data { get; init; }

        [CommandOption("--id <ID>")]
        [Description("Config ID for the new automation (default: millisecond timestamp)")]
        public string? Id { get; init; }

        public override ValidationResult Validate() => ValidateJsonData(Data);
    }

    protected override async Task<int> RunAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var configId = settings.Id ?? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        var config = ParseTyped<AutomationConfig>(settings.Data);

        await api.PostAsync<JsonElement>(
            $"/api/config/automation/config/{configId}", config, cancellationToken);

        OutputHelper.WriteResult(settings, new { action = "created", id = configId },
            () => AnsiConsole.MarkupLine($"[green]Created[/] [bold]{configId.EscapeMarkup()}[/]"),
            () => Console.WriteLine(configId));

        return 0;
    }
}
