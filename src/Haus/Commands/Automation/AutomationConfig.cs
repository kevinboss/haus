using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Haus.Commands.Automation;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal sealed record AutomationConfig
{
    public string? Id { get; init; }

    [JsonRequired]
    public required string Alias { get; init; }

    public string? Description { get; init; }

    public AutomationMode Mode { get; init; } = AutomationMode.Single;

    [JsonRequired]
    public required JsonElement[] Triggers { get; init; }

    public JsonElement[]? Conditions { get; init; }

    [JsonRequired]
    public required JsonElement[] Actions { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; init; }
}

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal enum AutomationMode
{
    Single,
    Restart,
    Queued,
    Parallel,
}
