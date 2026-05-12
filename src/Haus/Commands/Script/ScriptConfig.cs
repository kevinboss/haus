using System.Text.Json;
using System.Text.Json.Serialization;

namespace Haus.Commands.Script;

internal sealed record ScriptConfig
{
    [JsonRequired]
    public required string Alias { get; init; }

    public string? Description { get; init; }

    public ScriptMode Mode { get; init; } = ScriptMode.Single;

    public string? Icon { get; init; }

    public Dictionary<string, JsonElement>? Fields { get; init; }

    [JsonRequired]
    public required JsonElement[] Sequence { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; init; }
}

internal enum ScriptMode
{
    Single,
    Restart,
    Queued,
    Parallel,
}
