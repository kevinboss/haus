using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Haus.HassClient;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed record DeviceRegistryUpdate(
    string? NameByUser = null,
    string? AreaId = null,
    bool? Disabled = null,
    IReadOnlyList<string>? Labels = null);

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed record DeviceRegistryEntry(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("name_by_user")] string? NameByUser,
    [property: JsonPropertyName("manufacturer")] string? Manufacturer,
    [property: JsonPropertyName("model")] string? Model,
    [property: JsonPropertyName("model_id")] string? ModelId,
    [property: JsonPropertyName("sw_version")] string? SwVersion,
    [property: JsonPropertyName("hw_version")] string? HwVersion,
    [property: JsonPropertyName("area_id")] string? AreaId,
    [property: JsonPropertyName("config_entries")] List<string>? ConfigEntries,
    [property: JsonPropertyName("via_device_id")] string? ViaDeviceId,
    [property: JsonPropertyName("entry_type")] string? EntryType,
    [property: JsonPropertyName("disabled_by")] string? DisabledBy,
    [property: JsonPropertyName("configuration_url")] string? ConfigurationUrl,
    [property: JsonPropertyName("labels")] List<string>? Labels)
{
    // HA may return name_by_user/name as "" (empty) rather than null, so coalesce on emptiness.
    [JsonIgnore]
    public string DisplayName =>
        !string.IsNullOrEmpty(NameByUser) ? NameByUser :
        !string.IsNullOrEmpty(Name) ? Name : Id;

    [JsonIgnore]
    public string Status => DisabledBy is not null ? "disabled" : "active";
}
