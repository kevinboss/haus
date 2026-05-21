using System.Text.Json;
using Haus.Auth;
using Haus.Rest;
using Haus.Output;
using Spectre.Console;

namespace Haus.Commands.Helper;

public sealed class HelperListCommand(IAuthService auth, IHassApiClient api)
    : HausCommand<HelperListCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings;

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var states = await api.ListStatesAsync<JsonElement>(cancellationToken);
        var helpers = states
            .Where(s => HelperKinds.FromDomain(s.GetProperty("entity_id").GetString()!.Split('.', 2)[0]) is not null)
            .Select(HelperRow.From)
            .OrderBy(r => r.EntityId)
            .ToList();

        OutputHelper.WriteResult(settings, helpers,
            () =>
            {
                var table = new Table()
                    .Border(TableBorder.Rounded)
                    .AddColumn("Entity ID")
                    .AddColumn("Kind")
                    .AddColumn("Name")
                    .AddColumn("State");

                foreach (var h in helpers)
                    table.AddRow(
                        h.EntityId.EscapeMarkup(),
                        h.Kind.EscapeMarkup(),
                        h.Name.EscapeMarkup(),
                        h.State.EscapeMarkup());

                AnsiConsole.Write(table);
                AnsiConsole.MarkupLine($"[dim]{helpers.Count} helper(s)[/]");
            },
            () => OutputHelper.WriteColumns(
                ["ENTITY ID", "KIND", "NAME", "STATE"],
                helpers.Select(h => new[] { h.EntityId, h.Kind, h.Name, h.State })));

        return 0;
    }
}

internal sealed record HelperRow(string EntityId, string Kind, string Name, string State)
{
    public static HelperRow From(JsonElement state)
    {
        var entityId = state.GetProperty("entity_id").GetString()!;
        var domain = entityId.Split('.', 2)[0];
        var kind = HelperKinds.FromDomain(domain)!.Value.ToString().ToLowerInvariant();
        var name = state.TryGetProperty("attributes", out var attrs)
                   && attrs.TryGetProperty("friendly_name", out var fn)
                   && fn.GetString() is { } fnStr
            ? fnStr
            : entityId;
        var stateValue = state.GetProperty("state").GetString() ?? "";
        return new HelperRow(entityId, kind, name, stateValue);
    }
}
