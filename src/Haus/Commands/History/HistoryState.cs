using System.Text.Json;
using System.Text.Json.Serialization;

namespace Haus.Commands.History;

internal static class RecorderCommands
{
    public const string StatisticsDuringPeriod = "recorder/statistics_during_period";
}

internal sealed record HistoryState(
    [property: JsonPropertyName("state")] string? State,
    [property: JsonPropertyName("last_changed")] string? LastChanged,
    [property: JsonPropertyName("last_updated")] string? LastUpdated,
    [property: JsonPropertyName("entity_id")] string? EntityId);

internal sealed record StatisticsRow(
    [property: JsonPropertyName("start")] JsonElement Start,
    [property: JsonPropertyName("mean")] double? Mean,
    [property: JsonPropertyName("min")] double? Min,
    [property: JsonPropertyName("max")] double? Max,
    [property: JsonPropertyName("sum")] double? Sum);
