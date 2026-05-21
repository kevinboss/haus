using System.Globalization;
using Haus.Auth;
using Haus.Rest;
using Haus.Output;
using Spectre.Console;

namespace Haus.Commands.Scene;

public sealed class SceneListCommand(IAuthService auth, IHassApiClient api)
    : HausCommand<SceneListCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings;

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var states = await api.GetAsync<List<SceneState>>("/api/states", cancellationToken);
        var scenes = states
            .Where(s => s.EntityId.StartsWith("scene.", StringComparison.Ordinal))
            .OrderBy(s => s.EntityId)
            .ToList();

        OutputHelper.WriteResult(settings, scenes,
            () => WriteHuman(scenes),
            () => WritePorcelain(scenes));

        return 0;
    }

    private static void WriteHuman(List<SceneState> scenes)
    {
        if (scenes.Count == 0)
        {
            AnsiConsole.MarkupLine("[dim]No scenes found.[/]");
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn(new TableColumn("Entity ID").NoWrap())
            .AddColumn("Name")
            .AddColumn("Type")
            .AddColumn(new TableColumn("Entities").RightAligned())
            .AddColumn("Last Activated");

        foreach (var scene in scenes)
        {
            var a = scene.Attributes;
            table.AddRow(
                scene.EntityId.EscapeMarkup(),
                (a.FriendlyName ?? "").EscapeMarkup(),
                a.Id is null ? "[yellow]runtime[/]" : "[green]config[/]",
                (a.EntityIds?.Count ?? 0).ToString(CultureInfo.InvariantCulture),
                FormatLastActivated(scene.State).EscapeMarkup());
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"[dim]{scenes.Count} scene{(scenes.Count == 1 ? "" : "s")}[/]");
    }

    private static void WritePorcelain(List<SceneState> scenes)
    {
        OutputHelper.WriteColumns(
            ["ENTITY ID", "NAME", "TYPE", "ENTITIES", "LAST ACTIVATED"],
            scenes.Select(s => new[]
            {
                s.EntityId,
                s.Attributes.FriendlyName ?? "",
                s.Attributes.Id is null ? "runtime" : "config",
                (s.Attributes.EntityIds?.Count ?? 0).ToString(CultureInfo.InvariantCulture),
                s.State
            }));
    }

    private static string FormatLastActivated(string state) =>
        DateTimeOffset.TryParse(state, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt)
            ? dt.ToLocalTime().ToString("g", CultureInfo.InvariantCulture)
            : state;
}
