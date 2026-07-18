
namespace Haus.HassClient;

public interface IAreaRegistryClient
{
    Task<IReadOnlyList<AreaRegistryEntry>> ListAsync(CancellationToken cancellationToken = default);
    Task<AreaRegistryEntry?> GetAsync(string areaId, CancellationToken cancellationToken = default);
    Task<AreaRegistryEntry> CreateAsync(NewArea area, CancellationToken cancellationToken = default);
    Task UpdateAsync(string areaId, AreaRegistryUpdate update, CancellationToken cancellationToken = default);
    Task DeleteAsync(string areaId, CancellationToken cancellationToken = default);
}

internal sealed class AreaRegistryClient(IHassWebSocketClient ws) : IAreaRegistryClient
{
    public Task<IReadOnlyList<AreaRegistryEntry>> ListAsync(CancellationToken cancellationToken = default) =>
        ws.ListAreaRegistryAsync(cancellationToken);

    // HA exposes no area_registry/get endpoint — resolve against the list.
    public async Task<AreaRegistryEntry?> GetAsync(string areaId, CancellationToken cancellationToken = default)
    {
        var areas = await ws.ListAreaRegistryAsync(cancellationToken);
        return areas.FirstOrDefault(a => a.AreaId == areaId);
    }

    public Task<AreaRegistryEntry> CreateAsync(NewArea area, CancellationToken cancellationToken = default) =>
        ws.CreateAreaAsync(area, cancellationToken);

    public Task UpdateAsync(string areaId, AreaRegistryUpdate update, CancellationToken cancellationToken = default) =>
        ws.UpdateAreaAsync(areaId, update, cancellationToken);

    public Task DeleteAsync(string areaId, CancellationToken cancellationToken = default) =>
        ws.DeleteAreaAsync(areaId, cancellationToken);
}
