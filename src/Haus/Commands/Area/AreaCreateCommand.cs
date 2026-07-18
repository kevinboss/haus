using System.ComponentModel;
using Haus.HassClient;
using Haus.Auth;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Area;

public sealed class AreaCreateCommand(IAuthService auth, IHassClient client)
    : HausCommand<AreaCreateCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandOption("--name <NAME>")]
        [Description("Display name for the new area (e.g. 'Living Room')")]
        public required string Name { get; init; }

        [CommandOption("--icon <ICON>")]
        [Description("MDI icon (e.g. mdi:sofa)")]
        public string? Icon { get; init; }

        [CommandOption("--floor <FLOOR_ID>")]
        [Description("Floor to place this area on (e.g. ground_floor)")]
        public string? FloorId { get; init; }
    }

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var created = await client.Area.CreateAsync(
            new NewArea(Name: settings.Name, Icon: settings.Icon, FloorId: settings.FloorId),
            cancellationToken);

        OutputHelper.WriteResult(settings, created,
            () => AnsiConsole.MarkupLine(
                $"[green]Created[/] [bold]{created.AreaId.EscapeMarkup()}[/] — \"{created.Name.EscapeMarkup()}\""),
            () => Console.WriteLine(created.AreaId));

        return 0;
    }
}
