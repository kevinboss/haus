using System.ComponentModel;
using Haus.HassClient;
using Haus.Auth;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Device;

public sealed class DeviceUpdateCommand(IAuthService auth, IHassClient client)
    : HausCommand<DeviceUpdateCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<device_id>")]
        [Description("Device ID to update (from `haus device list`)")]
        public required string DeviceId { get; init; }

        [CommandOption("--name <NAME>")]
        [Description("Set the user-facing name; pass empty to revert to the integration's name")]
        public string? Name { get; init; }

        [CommandOption("--area <AREA_ID>")]
        [Description("Set area (e.g. living_room); pass empty to clear")]
        public string? AreaId { get; init; }

        [CommandOption("--disable")]
        [Description("Disable the device (and its entities)")]
        public bool Disable { get; init; }

        [CommandOption("--enable")]
        [Description("Re-enable the device")]
        public bool Enable { get; init; }

        public override ValidationResult Validate()
        {
            if (Disable && Enable)
                return ValidationResult.Error("Cannot pass both --disable and --enable.");

            var hasAnyChange = Name is not null || AreaId is not null || Disable || Enable;
            return hasAnyChange
                ? ValidationResult.Success()
                : ValidationResult.Error("No fields to update. Pass at least one option (--name, --area, --disable, --enable).");
        }
    }

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        bool? disabled = settings.Disable ? true : settings.Enable ? false : null;

        await client.Device.UpdateAsync(
            settings.DeviceId,
            new(NameByUser: settings.Name, AreaId: settings.AreaId, Disabled: disabled),
            cancellationToken);

        OutputHelper.WriteResult(settings, new { action = "updated", id = settings.DeviceId },
            () => AnsiConsole.MarkupLine($"[green]Updated[/] [bold]{settings.DeviceId.EscapeMarkup()}[/]"),
            () => Console.WriteLine(settings.DeviceId));

        return 0;
    }
}
