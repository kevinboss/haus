using System.ComponentModel;
using Haus.HassClient;
using Haus.Auth;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Label;

public sealed class LabelUpdateCommand(IAuthService auth, IHassClient client)
    : HausCommand<LabelUpdateCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<label_id>")]
        [Description("Label ID to update (e.g. critical)")]
        public required string LabelId { get; init; }

        [CommandOption("--name <NAME>")]
        [Description("Set display name")]
        public string? Name { get; init; }

        [CommandOption("--color <COLOR>")]
        [Description("Set color; pass empty to clear")]
        public string? Color { get; init; }

        [CommandOption("--icon <ICON>")]
        [Description("Set icon (e.g. mdi:alert); pass empty to clear")]
        public string? Icon { get; init; }

        [CommandOption("--description <TEXT>")]
        [Description("Set description; pass empty to clear")]
        public string? Description { get; init; }

        public override ValidationResult Validate()
        {
            var hasAnyChange = Name is not null || Color is not null || Icon is not null || Description is not null;
            return hasAnyChange
                ? ValidationResult.Success()
                : ValidationResult.Error("No fields to update. Pass at least one option (--name, --color, --icon, --description).");
        }
    }

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        await client.Label.UpdateAsync(
            settings.LabelId,
            new(Name: settings.Name, Color: settings.Color, Icon: settings.Icon, Description: settings.Description),
            cancellationToken);

        OutputHelper.WriteResult(settings, new { action = "updated", id = settings.LabelId },
            () => AnsiConsole.MarkupLine($"[green]Updated[/] [bold]{settings.LabelId.EscapeMarkup()}[/]"),
            () => Console.WriteLine(settings.LabelId));

        return 0;
    }
}
