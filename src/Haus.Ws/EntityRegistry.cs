using System.Text.Json.Serialization;

namespace Haus.Ws;

public sealed record EntityRegistryUpdate(
    string? Name = null,
    string? Icon = null,
    string? AreaId = null,
    string? NewEntityId = null);

public sealed record EntityRegistryEntry(
    [property: JsonPropertyName("entity_id")] string EntityId,
    [property: JsonPropertyName("unique_id")] string? UniqueId,
    [property: JsonPropertyName("platform")] string? Platform,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("original_name")] string? OriginalName,
    [property: JsonPropertyName("icon")] string? Icon,
    [property: JsonPropertyName("device_id")] string? DeviceId,
    [property: JsonPropertyName("area_id")] string? AreaId,
    [property: JsonPropertyName("disabled_by")] string? DisabledBy,
    [property: JsonPropertyName("hidden_by")] string? HiddenBy,
    [property: JsonPropertyName("entity_category")] string? EntityCategory,
    [property: JsonPropertyName("device_class")] string? DeviceClass,
    [property: JsonPropertyName("labels")] List<string>? Labels)
{
    [JsonIgnore]
    public string DisplayName => Name ?? OriginalName ?? EntityId;

    [JsonIgnore]
    public string Status =>
        DisabledBy is not null ? "disabled" :
        HiddenBy is not null ? "hidden" : "active";
}
