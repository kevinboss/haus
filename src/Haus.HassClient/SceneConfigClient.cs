
namespace Haus.HassClient;

public interface ISceneConfigClient
{
    Task<T> GetAsync<T>(string configId, CancellationToken cancellationToken = default);
    Task SaveAsync(string configId, object config, CancellationToken cancellationToken = default);
    Task DeleteAsync(string configId, CancellationToken cancellationToken = default);
}

internal sealed class SceneConfigClient(IHassApiClient api) : ISceneConfigClient
{
    public Task<T> GetAsync<T>(string configId, CancellationToken cancellationToken = default) =>
        api.GetSceneConfigAsync<T>(configId, cancellationToken);

    public Task SaveAsync(string configId, object config, CancellationToken cancellationToken = default) =>
        api.SaveSceneConfigAsync(configId, config, cancellationToken);

    public Task DeleteAsync(string configId, CancellationToken cancellationToken = default) =>
        api.DeleteSceneConfigAsync(configId, cancellationToken);
}
