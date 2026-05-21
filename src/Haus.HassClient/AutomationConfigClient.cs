
namespace Haus.HassClient;

public interface IAutomationConfigClient
{
    Task<T> GetAsync<T>(string configId, CancellationToken cancellationToken = default);
    Task SaveAsync(string configId, object config, CancellationToken cancellationToken = default);
    Task DeleteAsync(string configId, CancellationToken cancellationToken = default);
}

internal sealed class AutomationConfigClient(IHassApiClient api) : IAutomationConfigClient
{
    public Task<T> GetAsync<T>(string configId, CancellationToken cancellationToken = default) =>
        api.GetAutomationConfigAsync<T>(configId, cancellationToken);

    public Task SaveAsync(string configId, object config, CancellationToken cancellationToken = default) =>
        api.SaveAutomationConfigAsync(configId, config, cancellationToken);

    public Task DeleteAsync(string configId, CancellationToken cancellationToken = default) =>
        api.DeleteAutomationConfigAsync(configId, cancellationToken);
}
