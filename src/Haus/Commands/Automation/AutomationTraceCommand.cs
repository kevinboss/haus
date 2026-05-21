using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using Haus.Auth;
using Haus.Connection;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Automation;

public sealed class AutomationTraceCommand(IAuthService auth, IHassApiClient api, IHassWebSocketClient ws)
    : HausCommand<AutomationTraceCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<automation_id>")]
        [Description("Automation entity ID (e.g. automation.morning_routine)")]
        public required string AutomationId { get; init; }

        [CommandOption("--run <RUN_ID>")]
        [Description("Show full step tree for a specific run")]
        public string? RunId { get; init; }

        [CommandOption("--last")]
        [Description("Show full step tree for the most recent run")]
        public bool Last { get; init; }

        public override ValidationResult Validate() =>
            RunId is not null && Last
                ? ValidationResult.Error("--run and --last are mutually exclusive.")
                : ValidationResult.Success();
    }

    protected override async Task<int> RunAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var state = await api.GetAsync<AutomationState>($"/api/states/{settings.AutomationId}", cancellationToken);
        if (state.Attributes.Id is null)
        {
            OutputHelper.WriteError(settings, $"No config ID found for '{settings.AutomationId}'.");
            return 1;
        }

        var itemId = state.Attributes.Id;
        var list = await ws.SendCommandAsync(new Dictionary<string, object?>
        {
            ["type"] = TraceCommands.List,
            ["domain"] = "automation",
            ["item_id"] = itemId
        }, cancellationToken);
        var summaries = list.Deserialize<List<TraceSummary>>() ?? [];

        if (summaries.Count == 0)
        {
            if (settings is { Json: false, Porcelain: false })
                AnsiConsole.MarkupLine($"[dim]No traces recorded yet for {settings.AutomationId.EscapeMarkup()}. The automation may not have fired since HA started, or traces may have been purged.[/]");
            else
                OutputHelper.WriteResult(settings, Array.Empty<TraceSummary>(), () => { }, () => { });
            return 0;
        }

        var runId = settings.RunId ?? (settings.Last ? summaries[^1].RunId : null);
        if (runId is null)
            return WriteSummaryList(settings, summaries);

        var match = summaries.SingleOrDefault(s => s.RunId == runId);
        if (match is null)
        {
            OutputHelper.WriteError(settings, $"Run '{runId}' not found. Use `automation trace {settings.AutomationId}` to list recent runs.");
            return 1;
        }

        var trace = await ws.SendCommandAsync(new Dictionary<string, object?>
        {
            ["type"] = TraceCommands.Get,
            ["domain"] = "automation",
            ["item_id"] = itemId,
            ["run_id"] = runId
        }, cancellationToken);

        return WriteRunDetails(settings, match, trace);
    }

    private static int WriteSummaryList(Settings settings, List<TraceSummary> summaries)
    {
        // newest last in the API; reverse so newest first in human output
        var ordered = summaries.AsEnumerable().Reverse().Take(10).ToList();

        OutputHelper.WriteResult(settings, ordered,
            () =>
            {
                var table = new Table()
                    .Border(TableBorder.Rounded)
                    .AddColumn("Run ID")
                    .AddColumn("Started")
                    .AddColumn("Trigger")
                    .AddColumn("Result")
                    .AddColumn(new TableColumn("Duration").RightAligned());

                foreach (var s in ordered)
                    table.AddRow(
                        StackRunId(s.RunId).EscapeMarkup(),
                        FormatLocal(s.Timestamp.Start).EscapeMarkup(),
                        (s.Trigger ?? "[dim]—[/]"),
                        FormatResultMarkup(s.ScriptExecution),
                        FormatDuration(s.Timestamp.Start, s.Timestamp.Finish));

                AnsiConsole.Write(table);
                AnsiConsole.MarkupLine($"[dim]Showing {ordered.Count} most recent run(s). Use --run <id> or --last to expand.[/]");
            },
            () => OutputHelper.WriteColumns(
                ["RUN ID", "STARTED", "TRIGGER", "RESULT", "DURATION"],
                ordered.Select(s => new[]
                {
                    s.RunId,
                    s.Timestamp.Start ?? "",
                    s.Trigger ?? "",
                    s.ScriptExecution ?? "",
                    FormatDurationPlain(s.Timestamp.Start, s.Timestamp.Finish)
                })));

        return 0;
    }

    private static int WriteRunDetails(Settings settings, TraceSummary summary, JsonElement trace)
    {
        if (settings.Json)
        {
            Console.WriteLine(JsonSerializer.Serialize(trace, HausJsonOptions.Default));
            return 0;
        }

        if (settings.Porcelain)
        {
            WriteStepTreePorcelain(summary, trace);
            return 0;
        }

        WriteRunHuman(summary, trace);
        return 0;
    }

    private static void WriteRunHuman(TraceSummary summary, JsonElement trace)
    {
        AnsiConsole.MarkupLine($"[bold]Run {summary.RunId.EscapeMarkup()}[/]");
        AnsiConsole.MarkupLine($"[dim]Started:[/]  {FormatLocal(summary.Timestamp.Start).EscapeMarkup()}");
        AnsiConsole.MarkupLine($"[dim]Trigger:[/]  {(summary.Trigger ?? "—").EscapeMarkup()}");
        AnsiConsole.MarkupLine($"[dim]Result:[/]   {FormatResultMarkup(summary.ScriptExecution)}");
        AnsiConsole.MarkupLine($"[dim]Duration:[/] {FormatDuration(summary.Timestamp.Start, summary.Timestamp.Finish)}");
        AnsiConsole.WriteLine();

        if (!trace.TryGetProperty("trace", out var stepsMap) || stepsMap.ValueKind != JsonValueKind.Object)
        {
            AnsiConsole.MarkupLine("[dim]No step data.[/]");
            return;
        }

        AnsiConsole.MarkupLine("[bold]Steps[/]");
        var startUtc = DateTimeOffset.Parse(summary.Timestamp.Start!, CultureInfo.InvariantCulture);
        var containerKinds = DetectContainerKinds(stepsMap);
        foreach (var step in stepsMap.EnumerateObject())
        {
            var entries = step.Value;
            if (entries.ValueKind != JsonValueKind.Array || entries.GetArrayLength() == 0) continue;
            var first = entries[0];
            var depth = step.Name.Count(c => c == '/');
            var indent = new string(' ', depth * 2);
            var elapsed = ElapsedFrom(first, startUtc);
            var summaryText = SummarizeStep(step.Name, first, containerKinds);
            var count = entries.GetArrayLength();
            var multi = count > 1 ? $" [dim](×{count})[/]" : "";

            AnsiConsole.MarkupLine($"  {indent}[dim]{elapsed,8}[/]  {step.Name.EscapeMarkup()}{multi}  {summaryText}");
        }
    }

    private static Dictionary<string, string> DetectContainerKinds(JsonElement stepsMap)
    {
        // For each parent path, return the container type by inspecting the first child's next segment.
        var allPaths = stepsMap.EnumerateObject().Select(p => p.Name).ToList();
        var result = new Dictionary<string, string>();
        foreach (var path in allPaths)
        {
            var firstChild = allPaths.FirstOrDefault(p => p.StartsWith(path + "/", StringComparison.Ordinal));
            if (firstChild is null) continue;
            var nextSegment = firstChild[(path.Length + 1)..].Split('/')[0];
            result[path] = nextSegment switch
            {
                "parallel" => "parallel",
                "repeat" => "repeat",
                "then" or "else" => "if",
                "choose" or "default" => "choose",
                "sequence" => "sequence",
                "conditions" => "conditions",
                _ => nextSegment
            };
        }
        return result;
    }

    private static void WriteStepTreePorcelain(TraceSummary summary, JsonElement trace)
    {
        if (!trace.TryGetProperty("trace", out var stepsMap) || stepsMap.ValueKind != JsonValueKind.Object)
            return;
        var startUtc = DateTimeOffset.Parse(summary.Timestamp.Start!, CultureInfo.InvariantCulture);
        var containerKinds = DetectContainerKinds(stepsMap);
        OutputHelper.WriteColumns(
            ["ELAPSED", "PATH", "COUNT", "SUMMARY"],
            stepsMap.EnumerateObject().Select(step =>
            {
                var first = step.Value[0];
                return new[]
                {
                    ElapsedFrom(first, startUtc),
                    step.Name,
                    step.Value.GetArrayLength().ToString(CultureInfo.InvariantCulture),
                    StripMarkup(SummarizeStep(step.Name, first, containerKinds))
                };
            }));
    }

    private static string SummarizeStep(string path, JsonElement entry, Dictionary<string, string> containerKinds)
    {
        var kind = path.Split('/')[0];

        if (kind == "trigger")
            return "[green]triggered[/]";

        if (path.Contains("condition") && entry.TryGetProperty("result", out var condR)
            && condR.TryGetProperty("result", out var b) && b.ValueKind is JsonValueKind.True or JsonValueKind.False)
            return b.GetBoolean() ? "[green]pass[/]" : "[red]fail[/]";

        if (entry.TryGetProperty("result", out var res))
        {
            if (res.TryGetProperty("delay", out var d))
                return $"[dim]delay[/] {d.GetDouble().ToString("0.##", CultureInfo.InvariantCulture)}s";

            if (res.TryGetProperty("params", out var p)
                && p.TryGetProperty("domain", out var dom)
                && p.TryGetProperty("service", out var svc))
            {
                var domStr = dom.GetString() ?? "";
                var svcStr = svc.GetString() ?? "";
                var target = "";
                if (p.TryGetProperty("target", out var t) && t.TryGetProperty("entity_id", out var eid))
                    target = FormatEntityIds(eid);
                else if (p.TryGetProperty("service_data", out var sd) && sd.TryGetProperty("entity_id", out var seid))
                    target = FormatEntityIds(seid);
                return string.IsNullOrEmpty(target)
                    ? $"[cyan]{domStr}.{svcStr}[/]"
                    : $"[cyan]{domStr}.{svcStr}[/] → {target.EscapeMarkup()}";
            }
        }

        if (containerKinds.TryGetValue(path, out var containerKind))
            return $"[magenta]{containerKind}[/]";

        if (entry.TryGetProperty("changed_variables", out _))
            return "[dim](variables set)[/]";

        return "";
    }

    private static string FormatEntityIds(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.String => value.GetString() ?? "",
        JsonValueKind.Array when value.GetArrayLength() <= 3 =>
            string.Join(", ", value.EnumerateArray().Select(x => x.GetString())),
        JsonValueKind.Array =>
            $"{value[0].GetString()} +{value.GetArrayLength() - 1} more",
        _ => ""
    };

    private static string ElapsedFrom(JsonElement entry, DateTimeOffset start)
    {
        if (!entry.TryGetProperty("timestamp", out var ts) || ts.GetString() is not { } s)
            return "";
        var t = DateTimeOffset.Parse(s, CultureInfo.InvariantCulture);
        var ms = (long)(t - start).TotalMilliseconds;
        return ms < 1000
            ? $"+{ms}ms"
            : $"+{(ms / 1000.0).ToString("0.##", CultureInfo.InvariantCulture)}s";
    }

    private static string FormatLocal(string? iso) =>
        DateTimeOffset.TryParse(iso, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt)
            ? dt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
            : iso ?? "";

    private static string FormatDuration(string? start, string? finish)
    {
        if (start is null || finish is null) return "—";
        var startDt = DateTimeOffset.Parse(start, CultureInfo.InvariantCulture);
        var endDt = DateTimeOffset.Parse(finish, CultureInfo.InvariantCulture);
        var ms = (long)(endDt - startDt).TotalMilliseconds;
        return ms < 1000
            ? $"{ms} ms"
            : $"{(ms / 1000.0).ToString("0.##", CultureInfo.InvariantCulture)} s";
    }

    private static string FormatDurationPlain(string? start, string? finish) =>
        FormatDuration(start, finish);

    private static string FormatResultMarkup(string? script_execution) => script_execution switch
    {
        "finished" => "[green]finished[/]",
        "failed" => "[red]failed[/]",
        "cancelled" => "[yellow]cancelled[/]",
        "aborted" => "[yellow]aborted[/]",
        "condition-skipped" or "condition_skipped" => "[dim]skipped (condition)[/]",
        null => "—",
        _ => script_execution
    };

    private static string StackRunId(string runId)
    {
        var sb = new System.Text.StringBuilder(runId.Length + runId.Length / 8);
        for (var i = 0; i < runId.Length; i += 8)
        {
            if (i > 0) sb.Append('\n');
            sb.Append(runId.AsSpan(i, Math.Min(8, runId.Length - i)));
        }
        return sb.ToString();
    }

    private static string StripMarkup(string s)
    {
        // strip [tag]…[/] markup for porcelain output
        var sb = new System.Text.StringBuilder();
        var inTag = false;
        foreach (var c in s)
        {
            if (c == '[') inTag = true;
            else if (c == ']') inTag = false;
            else if (!inTag) sb.Append(c);
        }
        return sb.ToString();
    }
}

