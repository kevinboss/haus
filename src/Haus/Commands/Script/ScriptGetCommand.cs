using System.ComponentModel;
using System.Text.Json;
using Haus.Auth;
using Haus.Connection;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Script;

public sealed class ScriptGetCommand(IAuthService auth, IHassApiClient api)
    : HausCommand<ScriptGetCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<script_id>")]
        [Description("Script entity ID (e.g. script.notify_all_phones)")]
        public required string ScriptId { get; init; }
    }

    protected override async Task<int> RunAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var objectId = StripPrefix(settings.ScriptId);
        var config = await api.GetAsync<ScriptConfig>($"/api/config/script/config/{objectId}", cancellationToken);

        OutputHelper.WriteResult(settings, config,
            () => WriteHumanOutput(settings.ScriptId, config),
            () => Console.WriteLine(JsonSerializer.Serialize(config, HausJsonOptions.Default)));

        return 0;
    }

    internal static string StripPrefix(string id) =>
        id.StartsWith("script.", StringComparison.OrdinalIgnoreCase) ? id["script.".Length..] : id;

    private static void WriteHumanOutput(string entityId, ScriptConfig config)
    {
        AnsiConsole.MarkupLine($"[bold]{config.Alias.EscapeMarkup()}[/]");
        if (!string.IsNullOrEmpty(config.Description))
            AnsiConsole.MarkupLine($"[dim]{config.Description.EscapeMarkup()}[/]");
        AnsiConsole.WriteLine();

        var table = new Table().Border(TableBorder.None).HideHeaders()
            .AddColumn("Key").AddColumn("Value");
        table.AddRow("[dim]Entity ID[/]", entityId.EscapeMarkup());
        table.AddRow("[dim]Mode[/]", config.Mode.ToString().ToLowerInvariant());
        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();

        WriteFields(config.Fields);
        WriteSequence(config.Sequence);
    }

    private static void WriteFields(Dictionary<string, JsonElement>? fields)
    {
        if (fields is null || fields.Count == 0) return;

        AnsiConsole.MarkupLine("[bold]Fields[/]");
        foreach (var (name, value) in fields)
        {
            var required = value.TryGetProperty("required", out var r) && r.ValueKind == JsonValueKind.True;
            var marker = required ? " [dim](required)[/]" : "";
            AnsiConsole.MarkupLine($"  [dim]•[/] {name.EscapeMarkup()}{marker}");
        }
        AnsiConsole.WriteLine();
    }

    private static void WriteSequence(JsonElement[] sequence)
    {
        if (sequence.Length == 0) return;

        AnsiConsole.MarkupLine("[bold]Actions[/]");
        for (var i = 0; i < sequence.Length; i++)
            AnsiConsole.MarkupLine($"  [dim]{i + 1}.[/] {SummarizeAction(sequence[i]).EscapeMarkup()}");
        AnsiConsole.WriteLine();
    }

    private static string SummarizeAction(JsonElement action)
    {
        var actionName = action.TryGetProperty("action", out var a) ? a.GetString() ?? "" : "";
        var target = "";
        if (action.TryGetProperty("target", out var t) &&
            t.TryGetProperty("entity_id", out var eid))
            target = eid.ValueKind == JsonValueKind.Array
                ? string.Join(", ", eid.EnumerateArray().Select(x => x.GetString()))
                : eid.GetString() ?? "";

        if (!string.IsNullOrEmpty(actionName) && !string.IsNullOrEmpty(target))
            return $"{actionName} → {target}";
        if (!string.IsNullOrEmpty(actionName))
            return actionName;

        if (action.TryGetProperty("delay", out var delay)) return $"delay: {delay}";
        if (action.TryGetProperty("choose", out _)) return "choose";
        if (action.TryGetProperty("repeat", out _)) return "repeat";
        if (action.TryGetProperty("if", out _)) return "if/then";
        if (action.TryGetProperty("variables", out _)) return "variables";
        if (action.TryGetProperty("parallel", out _)) return "parallel";
        return "unknown action";
    }
}
