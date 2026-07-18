using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Haus.HassClient;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed record NewLabel(
    string Name,
    string? Color = null,
    string? Icon = null,
    string? Description = null);

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed record LabelUpdate(
    string? Name = null,
    string? Color = null,
    string? Icon = null,
    string? Description = null);

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed record LabelEntry(
    [property: JsonPropertyName("label_id")] string LabelId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("color")] string? Color,
    [property: JsonPropertyName("icon")] string? Icon,
    [property: JsonPropertyName("description")] string? Description);
