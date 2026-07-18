using System.Text.Json;
using Haus.HassClient;
using Haus.Output;
using Spectre.Console;

namespace Haus.Commands.Integration;

// Shared rendering for config-entry flows (reauth, reconfigure). A flow step is either a
// form asking for input, an external (browser/OAuth) step, or a terminal result. Reauth and
// reconfigure flows signal success with an `abort` whose reason ends in "_successful".
internal static class ConfigFlow
{
    public static bool IsSuccess(OptionsFlowStep step) =>
        step.Type == "create_entry" ||
        (step is { Type: "abort", Reason: { } r } && r.EndsWith("_successful", StringComparison.Ordinal));

    // Human-readable view of what a not-yet-submitted step needs.
    public static void WriteInspectBody(OptionsFlowStep step, string submitHint)
    {
        switch (step.Type)
        {
            case "form":
                WriteFormSchema(step);
                AnsiConsole.MarkupLine($"[dim]{submitHint}[/]");
                break;
            case "external_step":
                AnsiConsole.MarkupLine("[yellow]Browser-based (OAuth) step[/] — open this URL to complete, then re-run:");
                AnsiConsole.WriteLine(step.Url ?? "(no URL provided)");
                break;
            case "menu":
                AnsiConsole.MarkupLine("[dim]This flow presents a menu; submit --data '{\"next_step_id\": \"<option>\"}'.[/]");
                break;
            default:
                AnsiConsole.MarkupLine($"[dim]Step type: {step.Type.EscapeMarkup()}[/]");
                break;
        }
    }

    public static void WritePorcelainInspect(OptionsFlowStep step)
    {
        if (step.Type == "external_step")
        {
            OutputHelper.WriteKeyValue("url", step.Url ?? "");
            return;
        }
        WritePorcelainSchema(step);
    }

    // Result of submitting input to a flow. Returns the process exit code.
    public static int WriteResult(IOutputSettings settings, OptionsFlowStep result, string entryId, string verb)
    {
        if (IsSuccess(result))
        {
            OutputHelper.WriteResult(settings, result,
                () => AnsiConsole.MarkupLine($"[green]{verb}[/] [bold]{entryId.EscapeMarkup()}[/]"),
                () => Console.WriteLine(entryId));
            return 0;
        }

        return result.Type switch
        {
            "form" => Fail(settings, FormMessage(result)),
            "external_step" => Fail(settings,
                "This integration uses a browser-based (OAuth) step that can't be completed from the CLI. " +
                $"Open this URL to finish, then re-run:\n{result.Url ?? "(no URL provided)"}"),
            "abort" => Fail(settings, $"Flow ended: {result.Reason ?? "unknown reason"}"),
            _ => Fail(settings, $"Unexpected flow result type: {result.Type}")
        };
    }

    private static string FormMessage(OptionsFlowStep result) =>
        result.Errors is { ValueKind: JsonValueKind.Object } e && e.EnumerateObject().Any()
            ? $"Submission rejected: {e}"
            : "The flow needs more input (multi-step); run the same command without --data to see the next fields.";

    private static void WriteFormSchema(OptionsFlowStep step)
    {
        if (step.DataSchema is not { ValueKind: JsonValueKind.Array } fields)
        {
            AnsiConsole.MarkupLine("[dim]This step takes no fields — submit an empty object: --data '{}'[/]");
            return;
        }

        var table = new Table().Border(TableBorder.Rounded)
            .AddColumn("Field").AddColumn("Type").AddColumn("Default").AddColumn("Required");
        foreach (var f in fields.EnumerateArray())
        {
            var name = f.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";
            var type = f.TryGetProperty("type", out var t) ? t.GetString() ?? "" : "";
            var def = f.TryGetProperty("default", out var d) ? d.ToString() : "";
            var required = f.TryGetProperty("required", out var r) && r.GetBoolean();
            table.AddRow(name.EscapeMarkup(), type.EscapeMarkup(), def.EscapeMarkup(),
                required ? "[yellow]yes[/]" : "[dim]no[/]");
        }
        AnsiConsole.Write(table);
    }

    private static void WritePorcelainSchema(OptionsFlowStep step)
    {
        if (step.DataSchema is not { ValueKind: JsonValueKind.Array } fields) return;
        OutputHelper.WriteColumns(
            ["FIELD", "TYPE", "DEFAULT", "REQUIRED"],
            fields.EnumerateArray().Select(f =>
            {
                var name = f.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";
                var type = f.TryGetProperty("type", out var t) ? t.GetString() ?? "" : "";
                var def = f.TryGetProperty("default", out var d) ? d.ToString() : "";
                var required = f.TryGetProperty("required", out var r) && r.GetBoolean();
                return new[] { name, type, def, required ? "yes" : "no" };
            }));
    }

    private static int Fail(IOutputSettings settings, string message)
    {
        OutputHelper.WriteError(settings, message);
        return 1;
    }
}
