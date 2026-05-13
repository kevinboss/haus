using System.Text.Json;

namespace Haus.Commands.Automation;

internal static class AutomationSummarizer
{
    public static string SummarizeTrigger(JsonElement trigger)
    {
        var type = trigger.TryGetProperty("trigger", out var t) ? t.GetString() ?? "unknown" : "unknown";

        return type switch
        {
            "time" => FormatTimeTrigger(trigger),
            "state" => FormatStateTrigger(trigger),
            "numeric_state" => FormatEntityTarget(trigger, "entity_id", "numeric state"),
            "event" => trigger.TryGetProperty("event_type", out var e) ? $"event: {e.GetString()}" : "event",
            "sun" => trigger.TryGetProperty("event", out var s) ? $"sun {s.GetString()}" : "sun",
            "zone" => "zone",
            "device" => "device trigger",
            "template" => "template",
            "webhook" => trigger.TryGetProperty("webhook_id", out var w) ? $"webhook: {w.GetString()}" : "webhook",
            _ => type
        };
    }

    public static string SummarizeCondition(JsonElement condition)
    {
        var type = condition.TryGetProperty("condition", out var c) ? c.GetString() ?? "unknown" : "unknown";

        return type switch
        {
            "state" => FormatStateCondition(condition),
            "not" => FormatWrappedCondition("not", condition),
            "or" => FormatWrappedCondition("or", condition),
            "and" => FormatWrappedCondition("and", condition),
            "numeric_state" => FormatEntityTarget(condition, "entity_id", "numeric state"),
            "template" => "template",
            "time" => "time",
            "zone" => "zone",
            _ => type
        };
    }

    public static string SummarizeAction(JsonElement action)
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

        if (action.TryGetProperty("variables", out var vars))
            return FormatVariables(vars);
        if (action.TryGetProperty("parallel", out var parallel))
            return FormatParallel(parallel);
        if (action.TryGetProperty("if", out var ifBlock))
            return FormatIf(ifBlock, action);
        if (action.TryGetProperty("repeat", out var repeat))
            return FormatRepeat(repeat);
        if (action.TryGetProperty("choose", out var choose))
            return FormatChoose(choose);
        if (action.TryGetProperty("delay", out var delay))
            return $"delay: {delay}";
        if (action.TryGetProperty("condition", out var cond))
            return $"condition: {cond.GetString()}";
        if (action.TryGetProperty("wait_template", out _))
            return "wait_template";
        if (action.TryGetProperty("stop", out var stop))
            return stop.ValueKind == JsonValueKind.String ? $"stop: {stop.GetString()}" : "stop";

        return "unknown action";
    }

    private static string FormatVariables(JsonElement variables)
    {
        if (variables.ValueKind != JsonValueKind.Object)
            return "variables";
        var names = variables.EnumerateObject().Select(p => p.Name);
        return $"variables: {string.Join(", ", names)}";
    }

    private static string FormatParallel(JsonElement parallel)
    {
        if (parallel.ValueKind != JsonValueKind.Array)
            return "parallel";
        var count = parallel.GetArrayLength();
        return $"parallel: {count} {(count == 1 ? "branch" : "branches")}";
    }

    private static string FormatIf(JsonElement ifBlock, JsonElement action)
    {
        var conditionSummary = ifBlock.ValueKind switch
        {
            JsonValueKind.Array => string.Join(", ", ifBlock.EnumerateArray().Select(SummarizeCondition)),
            JsonValueKind.Object => SummarizeCondition(ifBlock),
            _ => ""
        };

        var thenCount = action.TryGetProperty("then", out var then) && then.ValueKind == JsonValueKind.Array
            ? then.GetArrayLength() : 0;
        var elseCount = action.TryGetProperty("else", out var elseEl) && elseEl.ValueKind == JsonValueKind.Array
            ? elseEl.GetArrayLength() : 0;

        var result = $"if ({conditionSummary}) → {thenCount} {Pluralize("then-step", thenCount)}";
        if (elseCount > 0)
            result += $", {elseCount} {Pluralize("else-step", elseCount)}";
        return result;
    }

    private static string FormatRepeat(JsonElement repeat)
    {
        if (repeat.ValueKind != JsonValueKind.Object)
            return "repeat";
        if (repeat.TryGetProperty("count", out var count))
            return $"repeat × {count}";
        if (repeat.TryGetProperty("while", out _))
            return "repeat × while";
        if (repeat.TryGetProperty("until", out _))
            return "repeat × until";
        if (repeat.TryGetProperty("for_each", out _))
            return "repeat × for_each";
        return "repeat";
    }

    private static string FormatChoose(JsonElement choose)
    {
        if (choose.ValueKind != JsonValueKind.Array)
            return "choose";
        var count = choose.GetArrayLength();
        return $"choose: {count} {(count == 1 ? "branch" : "branches")}";
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
        var result = FormatEntityTarget(trigger, "entity_id", "state change");
        if (trigger.TryGetProperty("attribute", out var attr))
            result += $" [{attr.GetString()}";
        if (trigger.TryGetProperty("to", out var to))
            result += trigger.TryGetProperty("attribute", out _) ? $" = {to}" : $" → {to}";
        if (trigger.TryGetProperty("attribute", out _))
            result += "]";
        return result;
    }

    private static string FormatEntityTarget(JsonElement element, string entityProp, string label)
    {
        if (!element.TryGetProperty(entityProp, out var e))
            return label;

        var entities = e.ValueKind == JsonValueKind.Array
            ? string.Join(", ", e.EnumerateArray().Select(x => x.GetString()))
            : e.GetString() ?? "";
        return $"{label}: {entities}";
    }

    private static string Pluralize(string singular, int count) =>
        count == 1 ? singular : singular + "s";
}
