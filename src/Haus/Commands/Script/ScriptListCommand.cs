using Haus.Auth;
using Haus.Commands.State;
using Haus.Rest;
using Haus.Output;
using Spectre.Console;

namespace Haus.Commands.Script;

public sealed class ScriptListCommand(IAuthService auth, IHassApiClient api)
    : HausCommand<ScriptListCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings;

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var states = await api.ListStatesAsync<EntityState>(cancellationToken);
        var scripts = states
            .Where(s => s.EntityId.StartsWith("script.", StringComparison.Ordinal))
            .OrderBy(s => s.EntityId)
            .ToList();

        OutputHelper.WriteResult(settings, scripts,
            () => WriteHumanOutput(scripts),
            () => OutputHelper.WriteColumns(
                ["ENTITY ID", "ALIAS", "LAST TRIGGERED"],
                scripts.Select(s => new[]
                {
                    s.EntityId,
                    GetFriendlyName(s),
                    GetLastTriggered(s)
                })));

        return 0;
    }

    private static void WriteHumanOutput(IReadOnlyList<EntityState> scripts)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn(new TableColumn("Entity ID").NoWrap())
            .AddColumn("Alias")
            .AddColumn("Last Triggered");

        foreach (var s in scripts)
        {
            table.AddRow(
                s.EntityId.EscapeMarkup(),
                GetFriendlyName(s).EscapeMarkup(),
                GetLastTriggered(s).EscapeMarkup());
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"[dim]{scripts.Count} scripts[/]");
    }

    private static string GetFriendlyName(EntityState s) =>
        s.Attributes.TryGetValue("friendly_name", out var name) && name is not null
            ? name.ToString() ?? ""
            : "";

    private static string GetLastTriggered(EntityState s)
    {
        if (!s.Attributes.TryGetValue("last_triggered", out var lt) || lt is null)
            return "never";

        var text = lt.ToString();
        if (string.IsNullOrEmpty(text)) return "never";

        return DateTimeOffset.TryParse(text, out var dt)
            ? dt.LocalDateTime.ToString("g")
            : text;
    }
}
