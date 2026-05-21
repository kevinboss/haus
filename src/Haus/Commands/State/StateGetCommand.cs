using System.ComponentModel;
using System.Text.Json;
using Haus.Auth;
using Haus.Rest;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.State;

public sealed class StateGetCommand(IAuthService auth, IHassApiClient api) : HausCommand<StateGetCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<entity_id>")]
        [Description("Entity ID (e.g. light.kitchen, vacuum.l40_ultra)")]
        public required string EntityId { get; init; }
    }

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var state = await api.GetAsync<EntityState>($"/api/states/{settings.EntityId}", cancellationToken);

        OutputHelper.WriteResult(settings, state,
            () =>
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
                    var display = FormatAttributeValue(value);
                    table.AddRow($"[dim]{key.EscapeMarkup()}[/]", display.EscapeMarkup());
                }

                AnsiConsole.Write(table);
            },
            () =>
            {
                OutputHelper.WriteKeyValue("entity_id", state.EntityId);
                OutputHelper.WriteKeyValue("state", state.State);
                OutputHelper.WriteKeyValue("last_changed", state.LastChanged.LocalDateTime.ToString("g"));
                OutputHelper.WriteKeyValue("last_updated", state.LastUpdated.LocalDateTime.ToString("g"));

                foreach (var (key, value) in state.Attributes.OrderBy(a => a.Key))
                    OutputHelper.WriteKeyValue(key, FormatAttributeValue(value));
            });

        return 0;
    }

    private static string FormatAttributeValue(object? value) => value switch
    {
        JsonElement el => el.ToString(),
        null => "",
        _ => value.ToString() ?? ""
    };
}
