using System.Text.Json.Serialization;

namespace Haus.Commands.History;

internal sealed record HistoryState(
    [property: JsonPropertyName("state")] string? State,
    [property: JsonPropertyName("last_changed")] string? LastChanged,
    [property: JsonPropertyName("last_updated")] string? LastUpdated,
    [property: JsonPropertyName("entity_id")] string? EntityId);
