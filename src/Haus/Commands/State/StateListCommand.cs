using System.Text.Json.Serialization;
using Haus.Auth;
using Haus.Rest;
using Haus.Output;
using Spectre.Console;
using JetBrains.Annotations;

namespace Haus.Commands.State;

public sealed class StateListCommand(IAuthService auth, IHassApiClient api) : HausCommand<StateListCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings;

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var states = await api.ListStatesAsync<EntityState>(cancellationToken);
        var sorted = states.OrderBy(s => s.EntityId).ToList();

        OutputHelper.WriteResult(settings, states,
            () =>
            {
                var table = new Table()
                    .Border(TableBorder.Rounded)
                    .AddColumn(new TableColumn("Entity ID").NoWrap())
                    .AddColumn("State")
                    .AddColumn("Last Changed");

                foreach (var state in sorted)
                {
                    table.AddRow(
                        state.EntityId.EscapeMarkup(),
                        state.State.EscapeMarkup(),
                        state.LastChanged.LocalDateTime.ToString("g").EscapeMarkup());
                }

                AnsiConsole.Write(table);
                AnsiConsole.MarkupLine($"[dim]{states.Count} entities[/]");
            },
            () =>
            {
                OutputHelper.WriteColumns(
                    ["ENTITY ID", "STATE", "LAST CHANGED"],
                    sorted.Select(s => new[]
                    {
                        s.EntityId, s.State, s.LastChanged.LocalDateTime.ToString("g")
                    }));
            });

        return 0;
    }
}

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record EntityState(
    [property: JsonPropertyName("entity_id")] string EntityId,
    [property: JsonPropertyName("state")] string State,
    [property: JsonPropertyName("attributes")] Dictionary<string, object?> Attributes,
    [property: JsonPropertyName("last_changed")] DateTimeOffset LastChanged,
    [property: JsonPropertyName("last_updated")] DateTimeOffset LastUpdated);
