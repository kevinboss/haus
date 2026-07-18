using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Haus.HassClient;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed record NewArea(
    string Name,
    string? Icon = null,
    string? FloorId = null);

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed record AreaRegistryUpdate(
    string? Name = null,
    string? Icon = null,
    string? FloorId = null,
    IReadOnlyList<string>? Labels = null);

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed record AreaRegistryEntry(
    [property: JsonPropertyName("area_id")] string AreaId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("floor_id")] string? FloorId,
    [property: JsonPropertyName("icon")] string? Icon,
    [property: JsonPropertyName("picture")] string? Picture,
    [property: JsonPropertyName("aliases")] List<string>? Aliases,
    [property: JsonPropertyName("labels")] List<string>? Labels);
