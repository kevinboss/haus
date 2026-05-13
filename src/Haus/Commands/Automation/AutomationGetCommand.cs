using System.ComponentModel;
using System.Text.Json;
using Haus.Auth;
using Haus.Connection;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Automation;

public sealed class AutomationGetCommand(IAuthService auth, IHassApiClient api)
    : HausCommand<AutomationGetCommand.Settings>(auth)
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

        var config = await api.GetAsync<AutomationConfig>($"/api/config/automation/config/{state.Attributes.Id}", cancellationToken);

        OutputHelper.WriteResult(settings, config,
            () => WriteHumanOutput(settings.AutomationId, state, config),
            () => Console.WriteLine(JsonSerializer.Serialize(config, HausJsonOptions.Default)));

        return 0;
    }

    private static void WriteHumanOutput(string entityId, AutomationState state, AutomationConfig config)
    {
        AnsiConsole.MarkupLine($"[bold]{config.Alias.EscapeMarkup()}[/]");
        if (!string.IsNullOrEmpty(config.Description))
            AnsiConsole.MarkupLine($"[dim]{config.Description.EscapeMarkup()}[/]");
        AnsiConsole.WriteLine();

        var table = new Table().Border(TableBorder.None).HideHeaders()
            .AddColumn("Key").AddColumn("Value");
        table.AddRow("[dim]Entity ID[/]", entityId.EscapeMarkup());
        table.AddRow("[dim]State[/]", state.State == "on" ? "[green]on[/]" : "[red]off[/]");
        table.AddRow("[dim]Mode[/]", config.Mode.ToString().ToLowerInvariant());
        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();

        WriteList("Triggers", config.Triggers, AutomationSummarizer.SummarizeTrigger);
        WriteList("Conditions", config.Conditions, AutomationSummarizer.SummarizeCondition);
        WriteList("Actions", config.Actions, AutomationSummarizer.SummarizeAction);
    }

    private static void WriteList(string heading, JsonElement[]? items, Func<JsonElement, string> summarize)
    {
        if (items is null || items.Length == 0) return;
        AnsiConsole.MarkupLine($"[bold]{heading}[/]");
        for (var i = 0; i < items.Length; i++)
            AnsiConsole.MarkupLine($"  [dim]{i + 1}.[/] {summarize(items[i]).EscapeMarkup()}");
        AnsiConsole.WriteLine();
    }
}
