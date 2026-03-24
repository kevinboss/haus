using System.ComponentModel;
using System.Text.Json;
using Haus.Connection;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.State;

public sealed class StateGetCommand(IHassApiClient api) : HausCommand<StateGetCommand.Settings>(api)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<entity_id>")]
        [Description("Entity ID (e.g. light.kitchen, vacuum.l40_ultra)")]
        public required string EntityId { get; init; }
    }

    protected override async Task<int> RunAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var state = await Api.GetAsync<EntityState>($"/api/states/{settings.EntityId}", cancellationToken);

        OutputHelper.WriteResult(settings.Json, state, () =>
        {
            var table = new Table()
                .Border(TableBorder.Rounded)
                .AddColumn("Property")
                .AddColumn("Value");

            table.AddRow("[bold]Entity ID[/]", state.EntityId.EscapeMarkup());
            table.AddRow("[bold]State[/]", state.State.EscapeMarkup());
            table.AddRow("[bold]Last Changed[/]", state.LastChanged.LocalDateTime.ToString("g").EscapeMarkup());
            table.AddRow("[bold]Last Updated[/]", state.LastUpdated.LocalDateTime.ToString("g").EscapeMarkup());

            foreach (var (key, value) in state.Attributes.OrderBy(a => a.Key))
            {
                var display = value switch
                {
                    JsonElement el => el.ToString(),
                    null => "",
                    _ => value.ToString() ?? ""
                };
                table.AddRow($"[dim]{key.EscapeMarkup()}[/]", display.EscapeMarkup());
            }

            AnsiConsole.Write(table);
        });

        return 0;
    }
}
