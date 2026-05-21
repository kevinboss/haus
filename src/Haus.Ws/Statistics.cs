using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Haus.Ws;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed record StatisticsRow(
    [property: JsonPropertyName("start")] JsonElement Start,
    [property: JsonPropertyName("mean")] double? Mean,
    [property: JsonPropertyName("min")] double? Min,
    [property: JsonPropertyName("max")] double? Max,
    [property: JsonPropertyName("sum")] double? Sum);
