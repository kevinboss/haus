using System.Globalization;
using Haus.Auth;
using Haus.Connection;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Zone;

public sealed class ZoneListCommand(IAuthService auth, IHassApiClient api)
    : HausCommand<ZoneListCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings;

    protected override async Task<int> RunAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var states = await api.GetAsync<List<ZoneState>>("/api/states", cancellationToken);
        var zones = states
            .Where(s => s.EntityId.StartsWith("zone.", StringComparison.Ordinal))
            .OrderBy(s => s.EntityId)
            .ToList();

        OutputHelper.WriteResult(settings, zones,
            humanOutput: () => WriteHuman(zones),
            porcelainOutput: () => WritePorcelain(zones));

        return 0;
    }

    private static void WriteHuman(List<ZoneState> zones)
    {
        if (zones.Count == 0)
        {
            AnsiConsole.MarkupLine("[dim]No zones configured.[/]");
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Entity ID")
            .AddColumn("Name")
            .AddColumn("Coords")
            .AddColumn(new TableColumn("Radius").RightAligned())
            .AddColumn(new TableColumn("Inside").RightAligned())
            .AddColumn("Editable");

        foreach (var zone in zones)
        {
            var a = zone.Attributes;
            table.AddRow(
                zone.EntityId.EscapeMarkup(),
                (a.FriendlyName ?? "").EscapeMarkup(),
                FormatCoords(a.Latitude, a.Longitude),
                FormatRadius(a.Radius),
                (a.Persons?.Count ?? 0).ToString(CultureInfo.InvariantCulture),
                a.Editable ? "[green]yes[/]" : "[dim]no[/]");
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"[dim]{zones.Count} zone{(zones.Count == 1 ? "" : "s")}[/]");
    }

    private static void WritePorcelain(List<ZoneState> zones)
    {
        OutputHelper.WriteColumns(
            ["ENTITY ID", "NAME", "LAT", "LNG", "RADIUS", "INSIDE", "EDITABLE"],
            zones.Select(z => new[]
            {
                z.EntityId,
                z.Attributes.FriendlyName ?? "",
                z.Attributes.Latitude.ToString("F6", CultureInfo.InvariantCulture),
                z.Attributes.Longitude.ToString("F6", CultureInfo.InvariantCulture),
                z.Attributes.Radius.ToString("F0", CultureInfo.InvariantCulture),
                (z.Attributes.Persons?.Count ?? 0).ToString(CultureInfo.InvariantCulture),
                z.Attributes.Editable ? "yes" : "no"
            }));
    }

    private static string FormatCoords(double lat, double lng) =>
        $"{lat.ToString("F4", CultureInfo.InvariantCulture)}, {lng.ToString("F4", CultureInfo.InvariantCulture)}";

    private static string FormatRadius(double radius) =>
        $"{radius.ToString("F0", CultureInfo.InvariantCulture)} m";
}
