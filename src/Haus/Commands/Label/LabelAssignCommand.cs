using System.ComponentModel;
using Haus.HassClient;
using Haus.Auth;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Label;

public sealed class LabelAssignCommand(IAuthService auth, IHassClient client)
    : HausCommand<LabelAssignCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<label_id>")]
        [Description("Label ID to assign (from `haus label list`)")]
        public required string LabelId { get; init; }

        [CommandOption("--entity <ENTITY_ID>")]
        [Description("Entity to label (repeatable)")]
        public string[] Entities { get; init; } = [];

        [CommandOption("--area <AREA_ID>")]
        [Description("Area to label (repeatable)")]
        public string[] Areas { get; init; } = [];

        public override ValidationResult Validate() =>
            Entities.Length + Areas.Length > 0
                ? ValidationResult.Success()
                : ValidationResult.Error("Pass at least one target: --entity and/or --area.");
    }

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var label = await client.Label.GetAsync(settings.LabelId, cancellationToken);
        if (label is null)
        {
            OutputHelper.WriteError(settings, $"Label '{settings.LabelId}' not found. Create it first with `haus label create`.");
            return 1;
        }

        var (changed, total) = await LabelAssignment.ApplyAsync(
            client, settings.LabelId, settings.Entities, settings.Areas, add: true, cancellationToken);

        OutputHelper.WriteResult(settings,
            new { action = "assigned", label_id = settings.LabelId, changed, total },
            () => AnsiConsole.MarkupLine(
                $"[green]Assigned[/] [bold]{settings.LabelId.EscapeMarkup()}[/] to {changed} of {total} target(s) [dim]({total - changed} already had it)[/]"),
            () => Console.WriteLine(settings.LabelId));

        return 0;
    }
}
