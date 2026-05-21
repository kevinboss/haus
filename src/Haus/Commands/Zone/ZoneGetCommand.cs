using System.ComponentModel;
using Haus.HassClient;
using System.Globalization;
using Haus.Auth;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Zone;

public sealed class ZoneGetCommand(IAuthService auth, IHassClient client)
    : HausCommand<ZoneGetCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<zone_id>")]
        [Description("Zone entity ID (e.g. zone.home)")]
        public required string ZoneId { get; init; }
    }

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var zone = await client.States.GetAsync<ZoneState>(settings.ZoneId, cancellationToken);

        OutputHelper.WriteResult(settings, zone,
            humanOutput: () => WriteHuman(zone),
            porcelainOutput: () => WritePorcelain(zone));

        return 0;
    }

    private static void WriteHuman(ZoneState zone)
    {
        var a = zone.Attributes;
        AnsiConsole.MarkupLine($"[bold]{(a.FriendlyName ?? zone.EntityId).EscapeMarkup()}[/]");
        AnsiConsole.WriteLine();

        var table = new Table().Border(TableBorder.None).HideHeaders()
            .AddColumn("Key").AddColumn("Value");
        table.AddRow("[dim]Entity ID[/]", zone.EntityId.EscapeMarkup());
        table.AddRow("[dim]Coordinates[/]",
            $"{a.Latitude.ToString("F6", CultureInfo.InvariantCulture)}, {a.Longitude.ToString("F6", CultureInfo.InvariantCulture)}");
        table.AddRow("[dim]Radius[/]", $"{a.Radius.ToString("F0", CultureInfo.InvariantCulture)} m");
        table.AddRow("[dim]Passive[/]", a.Passive ? "yes" : "no");
        table.AddRow("[dim]Editable[/]", a.Editable ? "yes" : "no");
        if (a.Icon is not null) table.AddRow("[dim]Icon[/]", a.Icon.EscapeMarkup());
        table.AddRow("[dim]Persons inside[/]",
            a.Persons is { Count: > 0 } ? string.Join(", ", a.Persons).EscapeMarkup() : "[dim](none)[/]");

        AnsiConsole.Write(table);
    }

    private static void WritePorcelain(ZoneState zone)
    {
        var a = zone.Attributes;
        OutputHelper.WriteKeyValue("entity_id", zone.EntityId);
        OutputHelper.WriteKeyValue("name", a.FriendlyName ?? "");
        OutputHelper.WriteKeyValue("latitude", a.Latitude.ToString("F6", CultureInfo.InvariantCulture));
        OutputHelper.WriteKeyValue("longitude", a.Longitude.ToString("F6", CultureInfo.InvariantCulture));
        OutputHelper.WriteKeyValue("radius", a.Radius.ToString("F0", CultureInfo.InvariantCulture));
        OutputHelper.WriteKeyValue("passive", a.Passive ? "yes" : "no");
        OutputHelper.WriteKeyValue("editable", a.Editable ? "yes" : "no");
        OutputHelper.WriteKeyValue("icon", a.Icon ?? "");
        OutputHelper.WriteKeyValue("persons", a.Persons is null ? "" : string.Join(",", a.Persons));
    }
}
