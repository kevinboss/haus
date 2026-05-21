using Haus.Auth;
using Haus.Commands.State;
using Haus.Rest;
using Haus.Output;
using Spectre.Console;

namespace Haus.Commands.Automation;

public sealed class AutomationListCommand(IAuthService auth, IHassApiClient api)
    : HausCommand<AutomationListCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings;

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var states = await api.ListStatesAsync<EntityState>(cancellationToken);
        var automations = states
            .Where(s => s.EntityId.StartsWith("automation.", StringComparison.Ordinal))
            .OrderBy(s => s.EntityId)
            .ToList();

        OutputHelper.WriteResult(settings, automations,
            () => WriteHumanOutput(automations),
            () => OutputHelper.WriteColumns(
                ["ENTITY ID", "ALIAS", "STATE", "LAST CHANGED"],
                automations.Select(s => new[]
                {
                    s.EntityId,
                    GetFriendlyName(s),
                    s.State,
                    s.LastChanged.LocalDateTime.ToString("g")
                })));

        return 0;
    }

    private static void WriteHumanOutput(IReadOnlyList<EntityState> automations)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn(new TableColumn("Entity ID").NoWrap())
            .AddColumn("Alias")
            .AddColumn("State")
            .AddColumn("Last Changed");

        foreach (var s in automations)
        {
            var stateMarkup = s.State == "on" ? "[green]on[/]" : "[red]off[/]";
            table.AddRow(
                s.EntityId.EscapeMarkup(),
                GetFriendlyName(s).EscapeMarkup(),
                stateMarkup,
                s.LastChanged.LocalDateTime.ToString("g").EscapeMarkup());
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"[dim]{automations.Count} automations[/]");
    }

    private static string GetFriendlyName(EntityState s) =>
        s.Attributes.TryGetValue("friendly_name", out var name) && name is not null
            ? name.ToString() ?? ""
            : "";
}
