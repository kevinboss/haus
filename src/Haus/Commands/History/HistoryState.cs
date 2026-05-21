using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Haus.Commands.History;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record HistoryState(
    [property: JsonPropertyName("state")] string? State,
    [property: JsonPropertyName("last_changed")] string? LastChanged,
    [property: JsonPropertyName("last_updated")] string? LastUpdated,
    [property: JsonPropertyName("entity_id")] string? EntityId);
