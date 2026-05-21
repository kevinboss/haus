using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Haus.Commands.Zone;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ZoneAttributes(
    [property: JsonPropertyName("latitude")] double Latitude,
    [property: JsonPropertyName("longitude")] double Longitude,
    [property: JsonPropertyName("radius")] double Radius,
    [property: JsonPropertyName("passive")] bool Passive,
    [property: JsonPropertyName("persons")] List<string>? Persons,
    [property: JsonPropertyName("editable")] bool Editable,
    [property: JsonPropertyName("icon")] string? Icon,
    [property: JsonPropertyName("friendly_name")] string? FriendlyName);

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record ZoneState(
    [property: JsonPropertyName("entity_id")] string EntityId,
    [property: JsonPropertyName("state")] string State,
    [property: JsonPropertyName("attributes")] ZoneAttributes Attributes);
