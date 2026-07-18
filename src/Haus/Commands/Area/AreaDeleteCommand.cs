using System.ComponentModel;
using Haus.HassClient;
using Haus.Auth;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Area;

public sealed class AreaDeleteCommand(IAuthService auth, IHassClient client)
    : HausCommand<AreaDeleteCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<area_id>")]
        [Description("Area ID to delete from the registry")]
        public required string AreaId { get; init; }
    }

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        await client.Area.DeleteAsync(settings.AreaId, cancellationToken);

        OutputHelper.WriteResult(settings, new { action = "deleted", id = settings.AreaId },
            () => AnsiConsole.MarkupLine($"[green]Deleted[/] [bold]{settings.AreaId.EscapeMarkup()}[/]"),
            () => Console.WriteLine(settings.AreaId));

        return 0;
    }
}
