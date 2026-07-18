using System.ComponentModel;
using Haus.HassClient;
using Haus.Auth;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Label;

public sealed class LabelCreateCommand(IAuthService auth, IHassClient client)
    : HausCommand<LabelCreateCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandOption("--name <NAME>")]
        [Description("Display name for the new label (e.g. 'Critical')")]
        public required string Name { get; init; }

        [CommandOption("--color <COLOR>")]
        [Description("Color name (e.g. red, blue, primary)")]
        public string? Color { get; init; }

        [CommandOption("--icon <ICON>")]
        [Description("MDI icon (e.g. mdi:alert)")]
        public string? Icon { get; init; }

        [CommandOption("--description <TEXT>")]
        [Description("Optional description")]
        public string? Description { get; init; }
    }

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var created = await client.Label.CreateAsync(
            new NewLabel(Name: settings.Name, Color: settings.Color, Icon: settings.Icon, Description: settings.Description),
            cancellationToken);

        OutputHelper.WriteResult(settings, created,
            () => AnsiConsole.MarkupLine(
                $"[green]Created[/] [bold]{created.LabelId.EscapeMarkup()}[/] — \"{created.Name.EscapeMarkup()}\""),
            () => Console.WriteLine(created.LabelId));

        return 0;
    }
}
