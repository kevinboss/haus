using Haus.Auth;
using Haus.HassClient;
using Haus.Output;
using Spectre.Console;

namespace Haus.Commands.Device;

public sealed class DeviceListCommand(IAuthService auth, IHassClient client)
    : HausCommand<DeviceListCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings;

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var entries = await client.Device.ListAsync(cancellationToken);
        var sorted = entries.OrderBy(d => d.DisplayName, StringComparer.OrdinalIgnoreCase).ToList();

        OutputHelper.WriteResult(settings, sorted,
            () =>
            {
                var table = new Table()
                    .Border(TableBorder.Rounded)
                    .AddColumn(new TableColumn("Device ID").NoWrap())
                    .AddColumn("Name")
                    .AddColumn("Manufacturer")
                    .AddColumn("Model")
                    .AddColumn("Area")
                    .AddColumn("Status");

                foreach (var entry in sorted)
                {
                    table.AddRow(
                        entry.Id.EscapeMarkup(),
                        entry.DisplayName.EscapeMarkup(),
                        (entry.Manufacturer ?? "").EscapeMarkup(),
                        (entry.Model ?? "").EscapeMarkup(),
                        (entry.AreaId ?? "").EscapeMarkup(),
                        entry.Status == "disabled" ? "[red]disabled[/]" : "[green]active[/]");
                }

                AnsiConsole.Write(table);
                AnsiConsole.MarkupLine($"[dim]{sorted.Count} devices[/]");
            },
            () => OutputHelper.WriteColumns(
                ["DEVICE ID", "NAME", "MANUFACTURER", "MODEL", "AREA", "STATUS"],
                sorted.Select(d => new[]
                {
                    d.Id, d.DisplayName, d.Manufacturer ?? "", d.Model ?? "", d.AreaId ?? "", d.Status
                })));

        return 0;
    }
}
