using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Haus.HassClient;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed record TraceSummary(
    [property: JsonPropertyName("run_id")] string RunId,
    [property: JsonPropertyName("state")] string? State,
    [property: JsonPropertyName("script_execution")] string? ScriptExecution,
    [property: JsonPropertyName("trigger")] string? Trigger,
    [property: JsonPropertyName("timestamp")] TraceTimestamp Timestamp);

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed record TraceTimestamp(
    [property: JsonPropertyName("start")] string? Start,
    [property: JsonPropertyName("finish")] string? Finish);
