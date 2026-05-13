using System.Text.Json.Serialization;

namespace Haus.Commands.Zone;

internal static class ZoneCommands
{
    public const string List = "zone/list";
    public const string Update = "zone/update";
    public const string CoreConfigUpdate = "config/core/update";
}

internal sealed record ZoneAttributes(
    [property: JsonPropertyName("latitude")] double Latitude,
    [property: JsonPropertyName("longitude")] double Longitude,
    [property: JsonPropertyName("radius")] double Radius,
    [property: JsonPropertyName("passive")] bool Passive,
    [property: JsonPropertyName("persons")] List<string>? Persons,
    [property: JsonPropertyName("editable")] bool Editable,
    [property: JsonPropertyName("icon")] string? Icon,
    [property: JsonPropertyName("friendly_name")] string? FriendlyName);

internal sealed record ZoneState(
    [property: JsonPropertyName("entity_id")] string EntityId,
    [property: JsonPropertyName("state")] string State,
    [property: JsonPropertyName("attributes")] ZoneAttributes Attributes);

internal sealed record ZoneConfig(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("latitude")] double Latitude,
    [property: JsonPropertyName("longitude")] double Longitude,
    [property: JsonPropertyName("radius")] double Radius,
    [property: JsonPropertyName("icon")] string? Icon = null,
    [property: JsonPropertyName("passive")] bool Passive = false);
