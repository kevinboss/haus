using System.ComponentModel;
using Haus.HassClient;
using System.Globalization;
using System.Text.Json;
using Haus.Auth;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Scene;

public sealed class SceneGetCommand(IAuthService auth, IHassClient client)
    : HausCommand<SceneGetCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<scene_id>")]
        [Description("Scene entity ID (e.g. scene.movies)")]
        public required string SceneId { get; init; }
    }

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var state = await client.States.GetAsync<SceneState>(settings.SceneId, cancellationToken);

        // Runtime scenes have no config endpoint — render from state only
        if (state.Attributes.Id is null)
        {
            OutputHelper.WriteResult(settings, state,
                () => WriteRuntimeHuman(state),
                () => WriteRuntimePorcelain(state));
            return 0;
        }

        var config = await client.SceneConfig.GetAsync<SceneConfig>(state.Attributes.Id, cancellationToken);

        OutputHelper.WriteResult(settings, config,
            () => WriteConfigHuman(state, config),
            () => WriteConfigPorcelain(state, config));

        return 0;
    }

    private static void WriteRuntimeHuman(SceneState state)
    {
        var a = state.Attributes;
        AnsiConsole.MarkupLine($"[bold]{(a.FriendlyName ?? state.EntityId).EscapeMarkup()}[/] [yellow](runtime scene)[/]");
        AnsiConsole.MarkupLine("[dim]Created via scene.create service — not editable.[/]");
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine("[bold]Entities[/]");
        if (a.EntityIds is { Count: > 0 })
            foreach (var entity in a.EntityIds)
                AnsiConsole.MarkupLine($"  [dim]•[/] {entity.EscapeMarkup()}");
        else
            AnsiConsole.MarkupLine("  [dim](none)[/]");
    }

    private static void WriteRuntimePorcelain(SceneState state)
    {
        OutputHelper.WriteKeyValue("entity_id", state.EntityId);
        OutputHelper.WriteKeyValue("type", "runtime");
        OutputHelper.WriteKeyValue("name", state.Attributes.FriendlyName ?? "");
        OutputHelper.WriteKeyValue("last_activated", state.State);
        OutputHelper.WriteKeyValue("entities", state.Attributes.EntityIds is null
            ? ""
            : string.Join(",", state.Attributes.EntityIds));
    }

    private static void WriteConfigHuman(SceneState state, SceneConfig config)
    {
        AnsiConsole.MarkupLine($"[bold]{config.Name.EscapeMarkup()}[/]");
        AnsiConsole.WriteLine();

        var table = new Table().Border(TableBorder.None).HideHeaders()
            .AddColumn("Key").AddColumn("Value");
        table.AddRow("[dim]Entity ID[/]", state.EntityId.EscapeMarkup());
        table.AddRow("[dim]Type[/]", "[green]config[/]");
        if (config.Icon is not null) table.AddRow("[dim]Icon[/]", config.Icon.EscapeMarkup());
        table.AddRow("[dim]Last Activated[/]", FormatLastActivated(state.State).EscapeMarkup());
        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine("[bold]Entities[/]");
        foreach (var (entityId, target) in config.Entities)
            AnsiConsole.MarkupLine($"  [dim]•[/] {entityId.EscapeMarkup()} → {SummarizeTarget(target).EscapeMarkup()}");
    }

    private static void WriteConfigPorcelain(SceneState state, SceneConfig config)
    {
        OutputHelper.WriteKeyValue("entity_id", state.EntityId);
        OutputHelper.WriteKeyValue("config_id", config.Id ?? state.Attributes.Id ?? "");
        OutputHelper.WriteKeyValue("type", "config");
        OutputHelper.WriteKeyValue("name", config.Name);
        OutputHelper.WriteKeyValue("icon", config.Icon ?? "");
        OutputHelper.WriteKeyValue("last_activated", state.State);
        foreach (var (entityId, target) in config.Entities)
            OutputHelper.WriteKeyValue($"entity.{entityId}", SummarizeTarget(target));
    }

    private static readonly HashSet<string> NoisyAttrs =
    [
        "state",
        "icon", "friendly_name", "entity_id",
        "supported_features", "supported_color_modes",
        "min_mireds", "max_mireds", "min_color_temp_kelvin", "max_color_temp_kelvin",
        "color_mode"
    ];

    private static string SummarizeTarget(JsonElement target) => target.ValueKind switch
    {
        JsonValueKind.String => target.GetString() ?? "",
        JsonValueKind.Object => SummarizeObjectTarget(target),
        _ => target.ToString()
    };

    private static string SummarizeObjectTarget(JsonElement target)
    {
        var state = target.TryGetProperty("state", out var s) ? s.GetString() ?? "" : "";
        var extras = target.EnumerateObject()
            .Where(p => !NoisyAttrs.Contains(p.Name) && !IsEmptyValue(p.Value))
            .Select(p => $"{p.Name}={FormatAttrValue(p.Value)}")
            .ToList();
        return extras.Count == 0 ? state : $"{state} ({string.Join(", ", extras)})";
    }

    private static bool IsEmptyValue(JsonElement v) => v.ValueKind switch
    {
        JsonValueKind.Null => true,
        JsonValueKind.String => string.IsNullOrEmpty(v.GetString()),
        JsonValueKind.Array => v.GetArrayLength() == 0,
        _ => false
    };

    private static string FormatAttrValue(JsonElement v) => v.ValueKind switch
    {
        JsonValueKind.String => v.GetString() ?? "",
        JsonValueKind.Array => "[" + string.Join(",", v.EnumerateArray().Select(FormatAttrValue)) + "]",
        _ => v.ToString()
    };

    private static string FormatLastActivated(string state) =>
        DateTimeOffset.TryParse(state, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt)
            ? dt.ToLocalTime().ToString("g", CultureInfo.InvariantCulture)
            : state;
}
