using System.Text.Json;

namespace Haus.HassClient;

public interface IEventsClient
{
    Task<IReadOnlyList<EventType>> ListTypesAsync(CancellationToken cancellationToken = default);
    Task<JsonElement> FireAsync(string eventType, object? data = null, CancellationToken cancellationToken = default);
}

internal sealed class EventsClient(IHassApiClient api) : IEventsClient
{
    public Task<IReadOnlyList<EventType>> ListTypesAsync(CancellationToken cancellationToken = default) =>
        api.ListEventTypesAsync(cancellationToken);

    public Task<JsonElement> FireAsync(string eventType, object? data = null, CancellationToken cancellationToken = default) =>
        api.FireEventAsync(eventType, data, cancellationToken);
}
