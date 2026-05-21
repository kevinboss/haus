using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Haus.HassClient;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed record ApiStatus(
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("version")] string? Version = null);

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed record EventType(
    [property: JsonPropertyName("event")] string Event,
    [property: JsonPropertyName("listener_count")] int ListenerCount);

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed record ServiceDomain(
    [property: JsonPropertyName("domain")] string Domain,
    [property: JsonPropertyName("services")] Dictionary<string, object> Services);

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed record ConfigCheckResult(
    [property: JsonPropertyName("result")] string? Result,
    [property: JsonPropertyName("errors")] string? Errors);

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed record LogbookEntry(
    [property: JsonPropertyName("when")] JsonElement When,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("message")] string? Message,
    [property: JsonPropertyName("domain")] string? Domain,
    [property: JsonPropertyName("entity_id")] string? EntityId,
    [property: JsonPropertyName("state")] string? State,
    [property: JsonPropertyName("context_user_id")] string? ContextUserId);

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed record HistoryState(
    [property: JsonPropertyName("state")] string? State,
    [property: JsonPropertyName("last_changed")] string? LastChanged,
    [property: JsonPropertyName("last_updated")] string? LastUpdated,
    [property: JsonPropertyName("entity_id")] string? EntityId);
