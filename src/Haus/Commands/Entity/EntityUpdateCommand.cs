using System.ComponentModel;
using Haus.Auth;
using Haus.Rest;
using Haus.Hass;
using Haus.Ws;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Entity;

public sealed class EntityUpdateCommand(IAuthService auth, IHassWebSocketClient ws)
    : HausCommand<EntityUpdateCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<entity_id>")]
        [Description("Entity ID to update (e.g. sensor.living_room_temperature)")]
        public required string EntityId { get; init; }

        [CommandOption("--name <NAME>")]
        [Description("Set display name")]
        public string? Name { get; init; }

        [CommandOption("--icon <ICON>")]
        [Description("Set icon (e.g. mdi:thermometer)")]
        public string? Icon { get; init; }

        [CommandOption("--area <AREA_ID>")]
        [Description("Set area (e.g. living_room)")]
        public string? AreaId { get; init; }

        [CommandOption("--new-id <ENTITY_ID>")]
        [Description("Change the entity ID itself")]
        public string? NewEntityId { get; init; }

        [CommandOption("--disable")]
        [Description("Disable the entity")]
        public bool Disable { get; init; }

        [CommandOption("--enable")]
        [Description("Re-enable the entity")]
        public bool Enable { get; init; }

        [CommandOption("--hide")]
        [Description("Hide the entity from the UI")]
        public bool Hide { get; init; }

        [CommandOption("--show")]
        [Description("Unhide the entity")]
        public bool Show { get; init; }

        public override ValidationResult Validate()
        {
            if (Disable && Enable)
                return ValidationResult.Error("Cannot pass both --disable and --enable.");
            if (Hide && Show)
                return ValidationResult.Error("Cannot pass both --hide and --show.");

            var hasAnyChange = Name is not null || Icon is not null || AreaId is not null
                || NewEntityId is not null || Disable || Enable || Hide || Show;
            return hasAnyChange
                ? ValidationResult.Success()
                : ValidationResult.Error("No fields to update. Pass at least one option (--name, --icon, --area, --new-id, --disable, --enable, --hide, --show).");
        }
    }

    protected override async Task<int> RunAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var payload = new Dictionary<string, object?>
        {
            ["type"] = EntityRegistryCommands.Update,
            ["entity_id"] = settings.EntityId
        };

        if (settings.Name is not null) payload["name"] = settings.Name;
        if (settings.Icon is not null) payload["icon"] = settings.Icon;
        if (settings.AreaId is not null) payload["area_id"] = settings.AreaId;
        if (settings.NewEntityId is not null) payload["new_entity_id"] = settings.NewEntityId;
        if (settings.Disable) payload["disabled_by"] = "user";
        if (settings.Enable) payload["disabled_by"] = null;
        if (settings.Hide) payload["hidden_by"] = "user";
        if (settings.Show) payload["hidden_by"] = null;

        await ws.SendCommandAsync(payload, cancellationToken);
        var finalId = settings.NewEntityId ?? settings.EntityId;

        OutputHelper.WriteResult(settings, new { action = "updated", id = finalId },
            () => AnsiConsole.MarkupLine($"[green]Updated[/] [bold]{finalId.EscapeMarkup()}[/]"),
            () => Console.WriteLine(finalId));

        return 0;
    }
}
