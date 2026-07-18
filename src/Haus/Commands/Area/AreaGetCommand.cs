using System.ComponentModel;
using Haus.HassClient;
using Haus.Auth;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Area;

public sealed class AreaGetCommand(IAuthService auth, IHassClient client)
    : HausCommand<AreaGetCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<area_id>")]
        [Description("Area ID (e.g. living_room)")]
        public required string AreaId { get; init; }
    }

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var entry = await client.Area.GetAsync(settings.AreaId, cancellationToken);
        if (entry is null)
        {
            OutputHelper.WriteError(settings, $"Area '{settings.AreaId}' not found in registry.");
            return 1;
        }

        OutputHelper.WriteResult(settings, entry,
            () => WriteHumanOutput(entry),
            () => WritePorcelainOutput(entry));

        return 0;
    }

    private static void WriteHumanOutput(AreaRegistryEntry entry)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Property")
            .AddColumn("Value");

        table.AddRow("[bold]Area ID[/]", entry.AreaId.EscapeMarkup());
        table.AddRow("[bold]Name[/]", entry.Name.EscapeMarkup());
        if (entry.FloorId is not null) table.AddRow("[dim]Floor[/]", entry.FloorId.EscapeMarkup());
        if (entry.Icon is not null) table.AddRow("[dim]Icon[/]", entry.Icon.EscapeMarkup());
        if (entry.Picture is not null) table.AddRow("[dim]Picture[/]", entry.Picture.EscapeMarkup());
        if (entry.Aliases is { Count: > 0 }) table.AddRow("[dim]Aliases[/]", string.Join(", ", entry.Aliases).EscapeMarkup());
        if (entry.Labels is { Count: > 0 }) table.AddRow("[dim]Labels[/]", string.Join(", ", entry.Labels).EscapeMarkup());

        AnsiConsole.Write(table);
    }

    private static void WritePorcelainOutput(AreaRegistryEntry entry)
    {
        OutputHelper.WriteKeyValue("area_id", entry.AreaId);
        OutputHelper.WriteKeyValue("name", entry.Name);
        OutputHelper.WriteKeyValue("floor_id", entry.FloorId ?? "");
        OutputHelper.WriteKeyValue("icon", entry.Icon ?? "");
        OutputHelper.WriteKeyValue("picture", entry.Picture ?? "");
        OutputHelper.WriteKeyValue("aliases", entry.Aliases is null ? "" : string.Join(",", entry.Aliases));
        OutputHelper.WriteKeyValue("labels", entry.Labels is null ? "" : string.Join(",", entry.Labels));
    }
}
