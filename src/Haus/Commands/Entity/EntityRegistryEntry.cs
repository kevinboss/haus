using System.Text.Json.Serialization;

namespace Haus.Commands.Entity;

internal static class EntityRegistryCommands
{
    public const string List = "config/entity_registry/list";
    public const string Get = "config/entity_registry/get";
    public const string Update = "config/entity_registry/update";
    public const string Remove = "config/entity_registry/remove";
}

internal sealed record EntityRegistryEntry(
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
    public string DisplayName => Name ?? OriginalName ?? EntityId;

    public string Status =>
        DisabledBy is not null ? "disabled" :
        HiddenBy is not null ? "hidden" : "active";
}
