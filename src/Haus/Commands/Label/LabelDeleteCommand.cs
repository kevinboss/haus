using System.ComponentModel;
using Haus.HassClient;
using Haus.Auth;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Label;

public sealed class LabelDeleteCommand(IAuthService auth, IHassClient client)
    : HausCommand<LabelDeleteCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<label_id>")]
        [Description("Label ID to delete from the registry")]
        public required string LabelId { get; init; }
    }

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        await client.Label.DeleteAsync(settings.LabelId, cancellationToken);

        OutputHelper.WriteResult(settings, new { action = "deleted", id = settings.LabelId },
            () => AnsiConsole.MarkupLine($"[green]Deleted[/] [bold]{settings.LabelId.EscapeMarkup()}[/]"),
            () => Console.WriteLine(settings.LabelId));

        return 0;
    }
}
