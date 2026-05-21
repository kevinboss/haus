using System.Text.Json;
using JetBrains.Annotations;

namespace Haus.Ws;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed record SystemLogEntry(
    double Timestamp,
    string Level,
    string Name,
    string Message,
    string? Exception,
    int Count)
{
    public static SystemLogEntry From(JsonElement el)
    {
        var msgEl = el.GetProperty("message");
        var message = msgEl.ValueKind == JsonValueKind.Array
            ? string.Join(" ", msgEl.EnumerateArray().Select(m => m.GetString() ?? ""))
            : msgEl.GetString() ?? "";

        return new SystemLogEntry(
            Timestamp: el.GetProperty("timestamp").GetDouble(),
            Level: el.TryGetProperty("level", out var lv) ? lv.GetString() ?? "" : "",
            Name: el.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "",
            Message: message,
            Exception: el.TryGetProperty("exception", out var ex) ? ex.GetString() : null,
            Count: el.TryGetProperty("count", out var c) && c.ValueKind == JsonValueKind.Number ? c.GetInt32() : 1);
    }
}
