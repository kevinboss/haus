using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Haus.Commands.Scene;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record SceneAttributes(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("icon")] string? Icon,
    [property: JsonPropertyName("friendly_name")] string? FriendlyName,
    [property: JsonPropertyName("entity_id")] List<string>? EntityIds);

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record SceneState(
    [property: JsonPropertyName("entity_id")] string EntityId,
    [property: JsonPropertyName("state")] string State,
    [property: JsonPropertyName("attributes")] SceneAttributes Attributes);

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record SceneConfig
{
    public string? Id { get; init; }

    [JsonRequired]
    public required string Name { get; init; }

    public string? Icon { get; init; }

    [JsonRequired]
    public required Dictionary<string, JsonElement> Entities { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; init; }
}
