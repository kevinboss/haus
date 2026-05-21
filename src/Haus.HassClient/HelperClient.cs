using System.Text.Json;

namespace Haus.HassClient;

public interface IHelperClient
{
    Task<JsonElement> CreateAsync(string domain, IReadOnlyDictionary<string, object?> fields, CancellationToken cancellationToken = default);
    Task DeleteAsync(string domain, string id, CancellationToken cancellationToken = default);
}

internal sealed class HelperClient(IHassWebSocketClient ws) : IHelperClient
{
    public Task<JsonElement> CreateAsync(string domain, IReadOnlyDictionary<string, object?> fields, CancellationToken cancellationToken = default) =>
        ws.CreateHelperAsync(domain, fields, cancellationToken);

    public Task DeleteAsync(string domain, string id, CancellationToken cancellationToken = default) =>
        ws.DeleteHelperAsync(domain, id, cancellationToken);
}
