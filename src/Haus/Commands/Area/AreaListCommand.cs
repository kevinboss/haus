using Haus.Auth;
using Haus.HassClient;
using Haus.Output;
using Spectre.Console;

namespace Haus.Commands.Area;

public sealed class AreaListCommand(IAuthService auth, IHassClient client)
    : HausCommand<AreaListCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings;

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var entries = await client.Area.ListAsync(cancellationToken);
        var sorted = entries.OrderBy(a => a.Name, StringComparer.OrdinalIgnoreCase).ToList();

        OutputHelper.WriteResult(settings, sorted,
            () =>
            {
                var table = new Table()
                    .Border(TableBorder.Rounded)
                    .AddColumn(new TableColumn("Area ID").NoWrap())
                    .AddColumn("Name")
                    .AddColumn("Floor")
                    .AddColumn("Icon")
                    .AddColumn("Labels");

                foreach (var entry in sorted)
                {
                    table.AddRow(
                        entry.AreaId.EscapeMarkup(),
                        entry.Name.EscapeMarkup(),
                        (entry.FloorId ?? "").EscapeMarkup(),
                        (entry.Icon ?? "").EscapeMarkup(),
                        (entry.Labels is { Count: > 0 } ? string.Join(", ", entry.Labels) : "").EscapeMarkup());
                }

                AnsiConsole.Write(table);
                AnsiConsole.MarkupLine($"[dim]{sorted.Count} areas[/]");
            },
            () => OutputHelper.WriteColumns(
                ["AREA ID", "NAME", "FLOOR", "ICON", "LABELS"],
                sorted.Select(a => new[]
                {
                    a.AreaId, a.Name, a.FloorId ?? "", a.Icon ?? "",
                    a.Labels is null ? "" : string.Join(",", a.Labels)
                })));

        return 0;
    }
}
