using System.ComponentModel;
using Haus.HassClient;
using Haus.Auth;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Label;

public sealed class LabelRemoveCommand(IAuthService auth, IHassClient client)
    : HausCommand<LabelRemoveCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<label_id>")]
        [Description("Label ID to remove (from `haus label list`)")]
        public required string LabelId { get; init; }

        [CommandOption("--entity <ENTITY_ID>")]
        [Description("Entity to unlabel (repeatable)")]
        public string[] Entities { get; init; } = [];

        [CommandOption("--area <AREA_ID>")]
        [Description("Area to unlabel (repeatable)")]
        public string[] Areas { get; init; } = [];

        public override ValidationResult Validate() =>
            Entities.Length + Areas.Length > 0
                ? ValidationResult.Success()
                : ValidationResult.Error("Pass at least one target: --entity and/or --area.");
    }

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        // No label-existence check here: removing a stale/deleted label id from entries is valid cleanup.
        var (changed, total) = await LabelAssignment.ApplyAsync(
            client, settings.LabelId, settings.Entities, settings.Areas, add: false, cancellationToken);

        OutputHelper.WriteResult(settings,
            new { action = "removed", label_id = settings.LabelId, changed, total },
            () => AnsiConsole.MarkupLine(
                $"[green]Removed[/] [bold]{settings.LabelId.EscapeMarkup()}[/] from {changed} of {total} target(s) [dim]({total - changed} didn't have it)[/]"),
            () => Console.WriteLine(settings.LabelId));

        return 0;
    }
}
