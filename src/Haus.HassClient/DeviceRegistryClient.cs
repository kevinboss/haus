
namespace Haus.HassClient;

public interface IDeviceRegistryClient
{
    Task<IReadOnlyList<DeviceRegistryEntry>> ListAsync(CancellationToken cancellationToken = default);
    Task<DeviceRegistryEntry?> GetAsync(string deviceId, CancellationToken cancellationToken = default);
    Task UpdateAsync(string deviceId, DeviceRegistryUpdate update, CancellationToken cancellationToken = default);
}

internal sealed class DeviceRegistryClient(IHassWebSocketClient ws) : IDeviceRegistryClient
{
    public Task<IReadOnlyList<DeviceRegistryEntry>> ListAsync(CancellationToken cancellationToken = default) =>
        ws.ListDeviceRegistryAsync(cancellationToken);

    // HA exposes no device_registry/get endpoint — resolve against the list.
    public async Task<DeviceRegistryEntry?> GetAsync(string deviceId, CancellationToken cancellationToken = default)
    {
        var devices = await ws.ListDeviceRegistryAsync(cancellationToken);
        return devices.FirstOrDefault(d => d.Id == deviceId);
    }

    public Task UpdateAsync(string deviceId, DeviceRegistryUpdate update, CancellationToken cancellationToken = default) =>
        ws.UpdateDeviceAsync(deviceId, update, cancellationToken);
}
