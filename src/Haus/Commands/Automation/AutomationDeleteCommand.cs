using System.ComponentModel;
using Haus.HassClient;
using Haus.Auth;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Automation;

public sealed class AutomationDeleteCommand(IAuthService auth, IHassClient client)
    : HausCommand<AutomationDeleteCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<automation_id>")]
        [Description("Automation entity ID (e.g. automation.morning_routine)")]
        public required string AutomationId { get; init; }
    }

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var state = await client.States.GetAsync<AutomationState>(settings.AutomationId, cancellationToken);
        if (state.Attributes.Id is null)
        {
            OutputHelper.WriteError(settings, $"No config ID found for '{settings.AutomationId}'. Is it a valid automation?");
            return 1;
        }

        await client.AutomationConfig.DeleteAsync(state.Attributes.Id, cancellationToken);

        OutputHelper.WriteResult(settings, new { deleted = settings.AutomationId },
            () => AnsiConsole.MarkupLine($"[green]Deleted[/] [bold]{settings.AutomationId.EscapeMarkup()}[/]"),
            () => Console.WriteLine(settings.AutomationId));

        return 0;
    }
}
