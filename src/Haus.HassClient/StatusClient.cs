
namespace Haus.HassClient;

public interface IStatusClient
{
    Task<ApiStatus> GetAsync(CancellationToken cancellationToken = default);
}

internal sealed class StatusClient(IHassApiClient api) : IStatusClient
{
    public Task<ApiStatus> GetAsync(CancellationToken cancellationToken = default) =>
        api.GetApiStatusAsync(cancellationToken);
}
