using System.ComponentModel;
using Haus.HassClient;
using Haus.Auth;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Backup;

public sealed class BackupDeleteCommand(IAuthService auth, IHassClient client)
    : HausCommand<BackupDeleteCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<backup_id>")]
        [Description("Backup ID to delete (from `haus backup list`)")]
        public required string BackupId { get; init; }
    }

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        await client.Backup.DeleteAsync(settings.BackupId, cancellationToken);

        OutputHelper.WriteResult(settings, new { action = "deleted", backup_id = settings.BackupId },
            () => AnsiConsole.MarkupLine($"[green]Deleted[/] backup [bold]{settings.BackupId.EscapeMarkup()}[/]"),
            () => Console.WriteLine(settings.BackupId));

        return 0;
    }
}
