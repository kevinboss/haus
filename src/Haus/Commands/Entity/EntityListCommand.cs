using System.Text.Json;
using Haus.Auth;
using Haus.Rest;
using Haus.Hass;
using Haus.Ws;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Entity;

public sealed class EntityListCommand(IAuthService auth, IHassWebSocketClient ws)
    : HausCommand<EntityListCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings;

    protected override async Task<int> RunAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var entries = await ws.ListEntityRegistryAsync(cancellationToken);
        var sorted = entries.OrderBy(e => e.EntityId).ToList();

        OutputHelper.WriteResult(settings, sorted,
            () =>
            {
                var table = new Table()
                    .Border(TableBorder.Rounded)
                    .AddColumn(new TableColumn("Entity ID").NoWrap())
                    .AddColumn("Name")
                    .AddColumn("Platform")
                    .AddColumn("Area")
                    .AddColumn("Status");

                foreach (var entry in sorted)
                {
                    table.AddRow(
                        entry.EntityId.EscapeMarkup(),
                        entry.DisplayName.EscapeMarkup(),
                        (entry.Platform ?? "").EscapeMarkup(),
                        (entry.AreaId ?? "").EscapeMarkup(),
                        entry.Status switch
                        {
                            "disabled" => "[red]disabled[/]",
                            "hidden" => "[yellow]hidden[/]",
                            _ => "[green]active[/]"
                        });
                }

                AnsiConsole.Write(table);
                AnsiConsole.MarkupLine($"[dim]{sorted.Count} entities[/]");
            },
            () => OutputHelper.WriteColumns(
                ["ENTITY ID", "NAME", "PLATFORM", "AREA", "STATUS"],
                sorted.Select(e => new[]
                {
                    e.EntityId, e.DisplayName, e.Platform ?? "", e.AreaId ?? "", e.Status
                })));

        return 0;
    }
}
