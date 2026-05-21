using System.Text.Json;

namespace Haus.HassClient;

public interface ITraceClient
{
    Task<IReadOnlyList<TraceSummary>> ListAsync(string domain, string itemId, CancellationToken cancellationToken = default);
    Task<JsonElement> GetAsync(string domain, string itemId, string runId, CancellationToken cancellationToken = default);
}

internal sealed class TraceClient(IHassWebSocketClient ws) : ITraceClient
{
    public Task<IReadOnlyList<TraceSummary>> ListAsync(string domain, string itemId, CancellationToken cancellationToken = default) =>
        ws.ListTracesAsync(domain, itemId, cancellationToken);

    public Task<JsonElement> GetAsync(string domain, string itemId, string runId, CancellationToken cancellationToken = default) =>
        ws.GetTraceAsync(domain, itemId, runId, cancellationToken);
}
