using System.ComponentModel;
using Haus.HassClient;
using Haus.Auth;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Backup;

public sealed class BackupCreateCommand(IAuthService auth, IHassClient client)
    : HausCommand<BackupCreateCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandOption("--name <NAME>")]
        [Description("Name for the backup (Supervisor generates one if omitted)")]
        public string? Name { get; init; }

        [CommandOption("--partial")]
        [Description("Back up Home Assistant config only (no database/add-ons); default is a full backup")]
        public bool Partial { get; init; }
    }

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var result = await client.Backup.CreateAsync(settings.Name, full: !settings.Partial, cancellationToken);
        var job = result.BackupJobId ?? "";

        OutputHelper.WriteResult(settings, result,
            () => AnsiConsole.MarkupLine(
                $"[green]Started[/] {(settings.Partial ? "partial" : "full")} backup [dim](job {job.EscapeMarkup()})[/] — runs in the background; check `haus backup list`"),
            () => Console.WriteLine(job));

        return 0;
    }
}
