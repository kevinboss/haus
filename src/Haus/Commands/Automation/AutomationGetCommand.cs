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

        var config = await api.GetAsync<JsonElement>($"/api/config/automation/config/{state.Attributes.Id}", cancellationToken);

        OutputHelper.WriteResult(settings, config,
            () => WriteHumanOutput(settings.AutomationId, state, config),
            () => Console.WriteLine(JsonSerializer.Serialize(config)));

        return 0;
    }

    private static void WriteHumanOutput(string entityId, AutomationState state, JsonElement config)
    {
        var alias = config.TryGetProperty("alias", out var a) ? a.GetString() ?? entityId : entityId;
        var mode = config.TryGetProperty("mode", out var m) ? m.GetString() ?? "single" : "single";
        var description = config.TryGetProperty("description", out var d) ? d.GetString() ?? "" : "";

        AnsiConsole.MarkupLine($"[bold]{alias.EscapeMarkup()}[/]");
        if (!string.IsNullOrEmpty(description))
            AnsiConsole.MarkupLine($"[dim]{description.EscapeMarkup()}[/]");
        AnsiConsole.WriteLine();

        var table = new Table().Border(TableBorder.None).HideHeaders()
            .AddColumn("Key").AddColumn("Value");
        table.AddRow("[dim]Entity ID[/]", entityId.EscapeMarkup());
        table.AddRow("[dim]State[/]", state.State == "on" ? "[green]on[/]" : "[red]off[/]");
        table.AddRow("[dim]Mode[/]", mode);
        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();

        WriteTriggers(config);
        WriteConditions(config);
        WriteActions(config);
    }

    private static void WriteTriggers(JsonElement config)
    {
        if (!config.TryGetProperty("triggers", out var triggers) || triggers.GetArrayLength() == 0)
            return;

        AnsiConsole.MarkupLine("[bold]Triggers[/]");
        var i = 1;
        foreach (var trigger in triggers.EnumerateArray())
        {
            var summary = SummarizeTrigger(trigger);
            AnsiConsole.MarkupLine($"  [dim]{i}.[/] {summary.EscapeMarkup()}");
            i++;
        }
        AnsiConsole.WriteLine();
    }

    private static void WriteConditions(JsonElement config)
    {
        if (!config.TryGetProperty("conditions", out var conditions) || conditions.GetArrayLength() == 0)
            return;

        AnsiConsole.MarkupLine("[bold]Conditions[/]");
        var i = 1;
        foreach (var condition in conditions.EnumerateArray())
        {
            AnsiConsole.MarkupLine($"  [dim]{i}.[/] {SummarizeCondition(condition).EscapeMarkup()}");
            i++;
        }
        AnsiConsole.WriteLine();
    }

    private static string SummarizeCondition(JsonElement condition)
    {
        var type = condition.TryGetProperty("condition", out var c) ? c.GetString() ?? "unknown" : "unknown";

        return type switch
        {
            "state" => FormatStateCondition(condition),
            "not" => FormatWrappedCondition("not", condition),
            "or" => FormatWrappedCondition("or", condition),
            "and" => FormatWrappedCondition("and", condition),
            "numeric_state" => FormatEntityTrigger(condition, "entity_id", "numeric state"),
            "template" => "template",
            "time" => "time",
            "zone" => "zone",
            _ => type
        };
    }

    private static string FormatStateCondition(JsonElement condition)
    {
        var entity = condition.TryGetProperty("entity_id", out var e)
            ? (e.ValueKind == JsonValueKind.Array
                ? string.Join(", ", e.EnumerateArray().Select(x => x.GetString()))
                : e.GetString() ?? "")
            : "";
        return $"state: {entity}";
    }

    private static string FormatWrappedCondition(string wrapper, JsonElement condition)
    {
        if (!condition.TryGetProperty("conditions", out var inner))
            return wrapper;
        var summaries = inner.EnumerateArray().Select(SummarizeCondition);
        return $"{wrapper}({string.Join(", ", summaries)})";
    }

    private static void WriteActions(JsonElement config)
    {
        if (!config.TryGetProperty("actions", out var actions) || actions.GetArrayLength() == 0)
            return;

        AnsiConsole.MarkupLine("[bold]Actions[/]");
        var i = 1;
        foreach (var action in actions.EnumerateArray())
        {
            var summary = SummarizeAction(action);
            AnsiConsole.MarkupLine($"  [dim]{i}.[/] {summary.EscapeMarkup()}");
            i++;
        }
        AnsiConsole.WriteLine();
    }

    private static string SummarizeTrigger(JsonElement trigger)
    {
        var type = trigger.TryGetProperty("trigger", out var t) ? t.GetString() ?? "unknown" : "unknown";

        return type switch
        {
            "time" => FormatTimeTrigger(trigger),
            "state" => FormatStateTrigger(trigger),
            "numeric_state" => FormatEntityTrigger(trigger, "entity_id", "numeric state"),
            "event" => trigger.TryGetProperty("event_type", out var e) ? $"event: {e.GetString()}" : "event",
            "sun" => trigger.TryGetProperty("event", out var s) ? $"sun {s.GetString()}" : "sun",
            "zone" => "zone",
            "device" => "device trigger",
            "template" => "template",
            "webhook" => trigger.TryGetProperty("webhook_id", out var w) ? $"webhook: {w.GetString()}" : "webhook",
            _ => type
        };
    }

    private static string FormatTimeTrigger(JsonElement trigger)
    {
        var at = trigger.TryGetProperty("at", out var a) ? a.GetString() ?? "" : "";
        if (trigger.TryGetProperty("weekday", out var days) && days.ValueKind == JsonValueKind.Array)
        {
            var dayList = string.Join(", ", days.EnumerateArray().Select(d => d.GetString()));
            return $"time at {at} ({dayList})";
        }
        return $"time at {at}";
    }

    private static string FormatStateTrigger(JsonElement trigger)
    {
        var result = FormatEntityTrigger(trigger, "entity_id", "state change");
        if (trigger.TryGetProperty("attribute", out var attr))
            result += $" [{attr.GetString()}";
        if (trigger.TryGetProperty("to", out var to))
            result += trigger.TryGetProperty("attribute", out _) ? $" = {to}" : $" → {to}";
        if (trigger.TryGetProperty("attribute", out _))
            result += "]";
        return result;
    }

    private static string FormatEntityTrigger(JsonElement trigger, string entityProp, string label)
    {
        if (!trigger.TryGetProperty(entityProp, out var e))
            return label;

        var entities = e.ValueKind == JsonValueKind.Array
            ? string.Join(", ", e.EnumerateArray().Select(x => x.GetString()))
            : e.GetString() ?? "";
        return $"{label}: {entities}";
    }

    private static string SummarizeAction(JsonElement action)
    {
        var actionName = action.TryGetProperty("action", out var a) ? a.GetString() ?? "" : "";
        var target = "";
        if (action.TryGetProperty("target", out var t) &&
            t.TryGetProperty("entity_id", out var eid))
            target = eid.GetString() ?? "";

        if (!string.IsNullOrEmpty(actionName) && !string.IsNullOrEmpty(target))
            return $"{actionName} → {target}";
        if (!string.IsNullOrEmpty(actionName))
            return actionName;

        if (action.TryGetProperty("choose", out var choose))
        {
            var branchCount = choose.GetArrayLength();
            return $"choose ({branchCount} {(branchCount == 1 ? "branch" : "branches")})";
        }
        if (action.TryGetProperty("delay", out var delay))
            return $"delay: {delay}";
        if (action.TryGetProperty("condition", out var cond))
            return $"condition: {cond.GetString()}";
        if (action.TryGetProperty("repeat", out _))
            return "repeat";
        if (action.TryGetProperty("wait_template", out _))
            return "wait_template";
        if (action.TryGetProperty("if", out _))
            return "if/then";

        return "unknown action";
    }
}
