using Haus.Auth;
using Haus.HassClient;
using Haus.Output;
using Spectre.Console;

namespace Haus.Commands.Backup;

public sealed class BackupListCommand(IAuthService auth, IHassClient client)
    : HausCommand<BackupListCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings;

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var entries = await client.Backup.ListAsync(cancellationToken);
        var sorted = entries.OrderByDescending(b => b.Date, StringComparer.Ordinal).ToList();

        OutputHelper.WriteResult(settings, sorted,
            () =>
            {
                var table = new Table()
                    .Border(TableBorder.Rounded)
                    .AddColumn(new TableColumn("Backup ID").NoWrap())
                    .AddColumn("Name")
                    .AddColumn("Date")
                    .AddColumn("HA Version")
                    .AddColumn(new TableColumn("Size (MB)").RightAligned())
                    .AddColumn("Protected");

                foreach (var b in sorted)
                {
                    table.AddRow(
                        b.BackupId.EscapeMarkup(),
                        (b.Name ?? "").EscapeMarkup(),
                        (b.Date ?? "").EscapeMarkup(),
                        (b.HomeassistantVersion ?? "").EscapeMarkup(),
                        b.SizeMb?.ToString("0.0") ?? "",
                        b.Protected ? "[yellow]yes[/]" : "[dim]no[/]");
                }

                AnsiConsole.Write(table);
                AnsiConsole.MarkupLine($"[dim]{sorted.Count} backups[/]");
            },
            () => OutputHelper.WriteColumns(
                ["BACKUP ID", "NAME", "DATE", "HA_VERSION", "SIZE_MB", "PROTECTED"],
                sorted.Select(b => new[]
                {
                    b.BackupId, b.Name ?? "", b.Date ?? "", b.HomeassistantVersion ?? "", b.SizeMb?.ToString("0.0") ?? "", b.Protected ? "yes" : "no"
                })));

        return 0;
    }
}
