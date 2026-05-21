using System.ComponentModel;
using Haus.HassClient;
using System.Text.Json;
using Haus.Auth;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Automation;

public sealed class AutomationEnableCommand(IAuthService auth, IHassClient client)
    : HausCommand<AutomationEnableCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<automation_id>")]
        [Description("Automation entity ID (e.g. automation.morning_routine)")]
        public required string AutomationId { get; init; }
    }

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        await client.Services.CallAsync("automation", "turn_on", new { entity_id = settings.AutomationId }, cancellationToken);

        var state = await client.States.GetAsync<JsonElement>(settings.AutomationId, cancellationToken);
        var newState = state.GetProperty("state").GetString() ?? "unknown";

        OutputHelper.WriteResult(settings, new { entity_id = settings.AutomationId, state = newState },
            () => AnsiConsole.MarkupLine($"[green]Enabled[/] [bold]{settings.AutomationId.EscapeMarkup()}[/] → [bold]{newState}[/]"),
            () => Console.WriteLine($"{settings.AutomationId}\t{newState}"));

        return 0;
    }
}
