
namespace Haus.HassClient;

public interface IConfigClient
{
    Task<ConfigCheckResult> CheckAsync(CancellationToken cancellationToken = default);
}

internal sealed class ConfigClient(IHassApiClient api) : IConfigClient
{
    public Task<ConfigCheckResult> CheckAsync(CancellationToken cancellationToken = default) =>
        api.CheckConfigAsync(cancellationToken);
}
