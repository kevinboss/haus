using System.ComponentModel;
using Haus.HassClient;
using Haus.Auth;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Area;

public sealed class AreaUpdateCommand(IAuthService auth, IHassClient client)
    : HausCommand<AreaUpdateCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<area_id>")]
        [Description("Area ID to update (e.g. living_room)")]
        public required string AreaId { get; init; }

        [CommandOption("--name <NAME>")]
        [Description("Set display name")]
        public string? Name { get; init; }

        [CommandOption("--icon <ICON>")]
        [Description("Set icon (e.g. mdi:sofa); pass empty to clear")]
        public string? Icon { get; init; }

        [CommandOption("--floor <FLOOR_ID>")]
        [Description("Set floor (e.g. ground_floor); pass empty to clear")]
        public string? FloorId { get; init; }

        public override ValidationResult Validate()
        {
            var hasAnyChange = Name is not null || Icon is not null || FloorId is not null;
            return hasAnyChange
                ? ValidationResult.Success()
                : ValidationResult.Error("No fields to update. Pass at least one option (--name, --icon, --floor).");
        }
    }

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        await client.Area.UpdateAsync(
            settings.AreaId,
            new(Name: settings.Name, Icon: settings.Icon, FloorId: settings.FloorId),
            cancellationToken);

        OutputHelper.WriteResult(settings, new { action = "updated", id = settings.AreaId },
            () => AnsiConsole.MarkupLine($"[green]Updated[/] [bold]{settings.AreaId.EscapeMarkup()}[/]"),
            () => Console.WriteLine(settings.AreaId));

        return 0;
    }
}
