
namespace Haus.HassClient;

public interface IEntityRegistryClient
{
    Task<IReadOnlyList<EntityRegistryEntry>> ListAsync(CancellationToken cancellationToken = default);
    Task<EntityRegistryEntry?> GetAsync(string entityId, CancellationToken cancellationToken = default);
    Task UpdateAsync(string entityId, EntityRegistryUpdate update, CancellationToken cancellationToken = default);
    Task SetEnabledAsync(string entityId, bool enabled, CancellationToken cancellationToken = default);
    Task SetHiddenAsync(string entityId, bool hidden, CancellationToken cancellationToken = default);
    Task RemoveAsync(string entityId, CancellationToken cancellationToken = default);
}

internal sealed class EntityRegistryClient(IHassWebSocketClient ws) : IEntityRegistryClient
{
    public Task<IReadOnlyList<EntityRegistryEntry>> ListAsync(CancellationToken cancellationToken = default) =>
        ws.ListEntityRegistryAsync(cancellationToken);

    public Task<EntityRegistryEntry?> GetAsync(string entityId, CancellationToken cancellationToken = default) =>
        ws.GetEntityRegistryEntryAsync(entityId, cancellationToken);

    public Task UpdateAsync(string entityId, EntityRegistryUpdate update, CancellationToken cancellationToken = default) =>
        ws.UpdateEntityRegistryEntryAsync(entityId, update, cancellationToken);

    public Task SetEnabledAsync(string entityId, bool enabled, CancellationToken cancellationToken = default) =>
        ws.SetEntityEnabledAsync(entityId, enabled, cancellationToken);

    public Task SetHiddenAsync(string entityId, bool hidden, CancellationToken cancellationToken = default) =>
        ws.SetEntityHiddenAsync(entityId, hidden, cancellationToken);

    public Task RemoveAsync(string entityId, CancellationToken cancellationToken = default) =>
        ws.RemoveEntityRegistryEntryAsync(entityId, cancellationToken);
}
