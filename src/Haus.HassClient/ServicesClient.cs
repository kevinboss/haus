using System.Text.Json;

namespace Haus.HassClient;

public interface IServicesClient
{
    Task<IReadOnlyList<ServiceDomain>> ListAsync(CancellationToken cancellationToken = default);
    Task<JsonElement> CallAsync(string domain, string service, object? data = null, CancellationToken cancellationToken = default);
}

internal sealed class ServicesClient(IHassApiClient api) : IServicesClient
{
    public Task<IReadOnlyList<ServiceDomain>> ListAsync(CancellationToken cancellationToken = default) =>
        api.ListServiceDomainsAsync(cancellationToken);

    public Task<JsonElement> CallAsync(string domain, string service, object? data = null, CancellationToken cancellationToken = default) =>
        api.CallServiceAsync(domain, service, data, cancellationToken);
}
