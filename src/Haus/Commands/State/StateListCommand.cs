using System.Text.Json.Serialization;
using Haus.Auth;
using Haus.Connection;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.State;

public sealed class StateListCommand(IAuthService auth, IHassApiClient api) : HausCommand<StateListCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings;

    protected override async Task<int> RunAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var states = await api.GetAsync<List<EntityState>>("/api/states", cancellationToken);

        OutputHelper.WriteResult(settings.Json, states, () =>
        {
            var table = new Table()
                .Border(TableBorder.Rounded)
                .AddColumn("Entity ID")
                .AddColumn("State")
                .AddColumn("Last Changed");

            foreach (var state in states.OrderBy(s => s.EntityId))
            {
                table.AddRow(
                    state.EntityId.EscapeMarkup(),
                    state.State.EscapeMarkup(),
                    state.LastChanged.LocalDateTime.ToString("g").EscapeMarkup());
            }

            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine($"[dim]{states.Count} entities[/]");
        });

        return 0;
    }
}

internal sealed record EntityState(
    [property: JsonPropertyName("entity_id")] string EntityId,
    [property: JsonPropertyName("state")] string State,
    [property: JsonPropertyName("attributes")] Dictionary<string, object?> Attributes,
    [property: JsonPropertyName("last_changed")] DateTimeOffset LastChanged,
    [property: JsonPropertyName("last_updated")] DateTimeOffset LastUpdated);
