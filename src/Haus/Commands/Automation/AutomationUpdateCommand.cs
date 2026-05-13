using System.ComponentModel;
using System.Text.Json;
using Haus.Auth;
using Haus.Connection;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Automation;

public sealed class AutomationUpdateCommand(IAuthService auth, IHassApiClient api)
    : HausCommand<AutomationUpdateCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<automation_id>")]
        [Description("Automation entity ID (e.g. automation.morning_routine)")]
        public required string AutomationId { get; init; }

        [CommandOption("--data <JSON>")]
        [Description("Full automation configuration as JSON")]
        public string? Data { get; init; }

        [CommandOption("--from-file <PATH>")]
        [Description("Read configuration JSON from a file (use --from-file=- for stdin)")]
        public string? FromFile { get; init; }

        public override ValidationResult Validate() =>
            JsonInput.ValidateRequired(Data, FromFile);
    }

    protected override async Task<int> RunAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var state = await api.GetAsync<AutomationState>($"/api/states/{settings.AutomationId}", cancellationToken);
        if (state.Attributes.Id is null)
        {
            OutputHelper.WriteError(settings, $"No config ID found for '{settings.AutomationId}'. Is it a valid automation?");
            return 1;
        }

        var json = JsonInput.Resolve(settings.Data, settings.FromFile)!;
        var config = ParseTyped<AutomationConfig>(json);
        var result = await api.PostAsync<JsonElement>(
            $"/api/config/automation/config/{state.Attributes.Id}", config, cancellationToken);

        OutputHelper.WriteResult(settings, result,
            () => AnsiConsole.MarkupLine($"[green]Updated[/] [bold]{settings.AutomationId.EscapeMarkup()}[/]"),
            () => Console.WriteLine(settings.AutomationId));

        return 0;
    }
}
