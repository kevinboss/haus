using Haus.Auth;
using Haus.HassClient;
using Haus.Output;
using Spectre.Console;

namespace Haus.Commands.Label;

public sealed class LabelListCommand(IAuthService auth, IHassClient client)
    : HausCommand<LabelListCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings;

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var entries = await client.Label.ListAsync(cancellationToken);
        var sorted = entries.OrderBy(l => l.Name, StringComparer.OrdinalIgnoreCase).ToList();

        OutputHelper.WriteResult(settings, sorted,
            () =>
            {
                var table = new Table()
                    .Border(TableBorder.Rounded)
                    .AddColumn(new TableColumn("Label ID").NoWrap())
                    .AddColumn("Name")
                    .AddColumn("Color")
                    .AddColumn("Icon")
                    .AddColumn("Description");

                foreach (var entry in sorted)
                {
                    table.AddRow(
                        entry.LabelId.EscapeMarkup(),
                        entry.Name.EscapeMarkup(),
                        (entry.Color ?? "").EscapeMarkup(),
                        (entry.Icon ?? "").EscapeMarkup(),
                        (entry.Description ?? "").EscapeMarkup());
                }

                AnsiConsole.Write(table);
                AnsiConsole.MarkupLine($"[dim]{sorted.Count} labels[/]");
            },
            () => OutputHelper.WriteColumns(
                ["LABEL ID", "NAME", "COLOR", "ICON", "DESCRIPTION"],
                sorted.Select(l => new[]
                {
                    l.LabelId, l.Name, l.Color ?? "", l.Icon ?? "", l.Description ?? ""
                })));

        return 0;
    }
}
