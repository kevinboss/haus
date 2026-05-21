using System.ComponentModel;
using Haus.Auth;
using Haus.Rest;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Automation;

public sealed class AutomationDeleteCommand(IAuthService auth, IHassApiClient api)
    : HausCommand<AutomationDeleteCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<automation_id>")]
        [Description("Automation entity ID (e.g. automation.morning_routine)")]
        public required string AutomationId { get; init; }
    }

    protected override async Task<int> RunAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var state = await api.GetAsync<AutomationState>($"/api/states/{settings.AutomationId}", cancellationToken);
        if (state.Attributes.Id is null)
        {
            OutputHelper.WriteError(settings, $"No config ID found for '{settings.AutomationId}'. Is it a valid automation?");
            return 1;
        }

        await api.DeleteAsync($"/api/config/automation/config/{state.Attributes.Id}", cancellationToken);

        OutputHelper.WriteResult(settings, new { deleted = settings.AutomationId },
            () => AnsiConsole.MarkupLine($"[green]Deleted[/] [bold]{settings.AutomationId.EscapeMarkup()}[/]"),
            () => Console.WriteLine(settings.AutomationId));

        return 0;
    }
}
