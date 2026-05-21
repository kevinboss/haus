using System.ComponentModel;
using System.Text.Json;
using Haus.Auth;
using Haus.Rest;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Automation;

public sealed class AutomationToggleCommand(IAuthService auth, IHassApiClient api)
    : HausCommand<AutomationToggleCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<automation_id>")]
        [Description("Automation entity ID (e.g. automation.morning_routine)")]
        public required string AutomationId { get; init; }
    }

    protected override async Task<int> RunAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var result = await api.PostAsync<JsonElement>(
            "/api/services/automation/toggle",
            new { entity_id = settings.AutomationId },
            cancellationToken);

        var state = await api.GetAsync<JsonElement>($"/api/states/{settings.AutomationId}", cancellationToken);
        var newState = state.GetProperty("state").GetString() ?? "unknown";

        OutputHelper.WriteResult(settings, new { entity_id = settings.AutomationId, state = newState },
            () => AnsiConsole.MarkupLine($"[green]Toggled[/] [bold]{settings.AutomationId.EscapeMarkup()}[/] → [bold]{newState}[/]"),
            () => Console.WriteLine($"{settings.AutomationId}\t{newState}"));

        return 0;
    }
}
