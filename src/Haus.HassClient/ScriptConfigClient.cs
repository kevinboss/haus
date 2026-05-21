
namespace Haus.HassClient;

public interface IScriptConfigClient
{
    Task<T> GetAsync<T>(string objectId, CancellationToken cancellationToken = default);
    Task SaveAsync(string objectId, object config, CancellationToken cancellationToken = default);
    Task DeleteAsync(string objectId, CancellationToken cancellationToken = default);
}

internal sealed class ScriptConfigClient(IHassApiClient api) : IScriptConfigClient
{
    public Task<T> GetAsync<T>(string objectId, CancellationToken cancellationToken = default) =>
        api.GetScriptConfigAsync<T>(objectId, cancellationToken);

    public Task SaveAsync(string objectId, object config, CancellationToken cancellationToken = default) =>
        api.SaveScriptConfigAsync(objectId, config, cancellationToken);

    public Task DeleteAsync(string objectId, CancellationToken cancellationToken = default) =>
        api.DeleteScriptConfigAsync(objectId, cancellationToken);
}
