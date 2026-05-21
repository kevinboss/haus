using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Haus.Commands.Automation;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record AutomationAttributes(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("friendly_name")] string? FriendlyName);

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record AutomationState(
    [property: JsonPropertyName("entity_id")] string EntityId,
    [property: JsonPropertyName("state")] string State,
    [property: JsonPropertyName("attributes")] AutomationAttributes Attributes);
