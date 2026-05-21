using System.ComponentModel;
using System.Text.Json;
using Haus.Auth;
using Haus.Rest;
using Haus.Hass;
using Haus.Ws;
using Haus.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Haus.Commands.Zone;

public sealed class ZoneUpdateCommand(IAuthService auth, IHassApiClient api, IHassWebSocketClient ws)
    : HausCommand<ZoneUpdateCommand.Settings>(auth)
{
    public sealed class Settings : HausSettings
    {
        [CommandArgument(0, "<zone_id>")]
        [Description("Zone entity ID (e.g. zone.home)")]
        public required string ZoneId { get; init; }

        [CommandOption("--radius <METERS>")]
        [Description("Set the zone radius in meters")]
        public double? Radius { get; init; }

        [CommandOption("--lat <LAT>")]
        [Description("Set the zone latitude (-90 to 90)")]
        public double? Latitude { get; init; }

        [CommandOption("--lng <LNG>")]
        [Description("Set the zone longitude (-180 to 180)")]
        public double? Longitude { get; init; }

        [CommandOption("--icon <ICON>")]
        [Description("Set the zone icon (e.g. mdi:home)")]
        public string? Icon { get; init; }

        [CommandOption("--passive")]
        [Description("Mark zone as passive (won't trigger automations)")]
        public bool Passive { get; init; }

        [CommandOption("--active")]
        [Description("Mark zone as active (default; cancels --passive)")]
        public bool Active { get; init; }

        [CommandOption("--data <JSON>")]
        [Description("Full zone configuration as JSON (replaces all fields)")]
        public string? Data { get; init; }

        [CommandOption("--from-file <PATH>")]
        [Description("Read configuration JSON from a file (use --from-file=- for stdin)")]
        public string? FromFile { get; init; }

        public bool HasPartialField => Radius is not null || Latitude is not null || Longitude is not null
            || Icon is not null || Passive || Active;

        public override ValidationResult Validate()
        {
            var jsonResult = JsonInput.ValidateOptional(Data, FromFile);
            if (!jsonResult.Successful) return jsonResult;

            var hasFullBody = Data is not null || FromFile is not null;
            if (hasFullBody && HasPartialField)
                return ValidationResult.Error("--data/--from-file cannot be combined with partial flags (--radius, --lat, --lng, --icon, --passive, --active).");

            if (!hasFullBody && !HasPartialField)
                return ValidationResult.Error("Provide at least one field to update (--radius, --lat, --lng, --icon, --passive/--active, --data, --from-file).");

            if (Passive && Active)
                return ValidationResult.Error("Cannot pass both --passive and --active.");

            if (Radius is <= 0)
                return ValidationResult.Error("--radius must be positive.");
            if (Latitude is < -90 or > 90)
                return ValidationResult.Error("--lat must be between -90 and 90.");
            if (Longitude is < -180 or > 180)
                return ValidationResult.Error("--lng must be between -180 and 180.");

            return ValidationResult.Success();
        }
    }

    protected override async Task<int> RunAsync(Settings settings, CancellationToken cancellationToken)
    {
        var objectId = StripPrefix(settings.ZoneId);

        if (objectId == "home")
            return await UpdateHomeZoneAsync(settings, cancellationToken);

        var update = await BuildZoneUpdateAsync(settings, cancellationToken);
        await ws.UpdateZoneAsync(objectId, update, cancellationToken);
        return WriteSuccess(settings);
    }

    private async Task<int> UpdateHomeZoneAsync(Settings settings, CancellationToken cancellationToken)
    {
        if (settings.Icon is not null || settings.Passive || settings.Active
            || settings.Data is not null || settings.FromFile is not null)
        {
            OutputHelper.WriteError(settings,
                "zone.home is linked to HA's installation config — only --radius, --lat, --lng are supported.");
            return 1;
        }

        await ws.UpdateCoreConfigAsync(
            latitude: settings.Latitude,
            longitude: settings.Longitude,
            radius: settings.Radius,
            cancellationToken: cancellationToken);
        return WriteSuccess(settings);
    }

    private static int WriteSuccess(Settings settings)
    {
        OutputHelper.WriteResult(settings, new { action = "updated", id = settings.ZoneId },
            () => AnsiConsole.MarkupLine($"[green]Updated[/] [bold]{settings.ZoneId.EscapeMarkup()}[/]"),
            () => Console.WriteLine(settings.ZoneId));
        return 0;
    }

    private async Task<ZoneUpdate> BuildZoneUpdateAsync(Settings settings, CancellationToken cancellationToken)
    {
        var rawJson = TextInput.Resolve(settings.Data, settings.FromFile);
        if (rawJson is not null)
        {
            var parsed = JsonSerializer.Deserialize<ZoneUpdate>(rawJson, HassJsonOptions.Default);
            return parsed ?? throw new InvalidOperationException("--data deserialized to null.");
        }

        var current = await api.GetStateAsync<ZoneState>(settings.ZoneId, cancellationToken);
        var a = current.Attributes;

        return new ZoneUpdate(
            Name: a.FriendlyName ?? StripPrefix(settings.ZoneId),
            Latitude: settings.Latitude ?? a.Latitude,
            Longitude: settings.Longitude ?? a.Longitude,
            Radius: settings.Radius ?? a.Radius,
            Icon: settings.Icon ?? a.Icon,
            Passive: settings.Passive || (!settings.Active && a.Passive));
    }

    private static string StripPrefix(string entityId) =>
        entityId.StartsWith("zone.", StringComparison.Ordinal) ? entityId[5..] : entityId;
}
