
namespace Haus.HassClient;

public interface IStatesClient
{
    Task<IReadOnlyList<T>> ListAsync<T>(CancellationToken cancellationToken = default);
    Task<T> GetAsync<T>(string entityId, CancellationToken cancellationToken = default);
    Task<T> SetAsync<T>(string entityId, object body, CancellationToken cancellationToken = default);
    Task DeleteAsync(string entityId, CancellationToken cancellationToken = default);
}

internal sealed class StatesClient(IHassApiClient api) : IStatesClient
{
    public Task<IReadOnlyList<T>> ListAsync<T>(CancellationToken cancellationToken = default) =>
        api.ListStatesAsync<T>(cancellationToken);

    public Task<T> GetAsync<T>(string entityId, CancellationToken cancellationToken = default) =>
        api.GetStateAsync<T>(entityId, cancellationToken);

    public Task<T> SetAsync<T>(string entityId, object body, CancellationToken cancellationToken = default) =>
        api.SetStateAsync<T>(entityId, body, cancellationToken);

    public Task DeleteAsync(string entityId, CancellationToken cancellationToken = default) =>
        api.DeleteStateAsync(entityId, cancellationToken);
}
