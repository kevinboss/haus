using System.Text.Json.Serialization;

namespace Haus.Commands.Automation;

internal static class TraceCommands
{
    public const string List = "trace/list";
    public const string Get = "trace/get";
}

internal sealed record TraceSummary(
    [property: JsonPropertyName("run_id")] string RunId,
    [property: JsonPropertyName("state")] string? State,
    [property: JsonPropertyName("script_execution")] string? ScriptExecution,
    [property: JsonPropertyName("trigger")] string? Trigger,
    [property: JsonPropertyName("timestamp")] TraceTimestamp Timestamp);

internal sealed record TraceTimestamp(
    [property: JsonPropertyName("start")] string? Start,
    [property: JsonPropertyName("finish")] string? Finish);
