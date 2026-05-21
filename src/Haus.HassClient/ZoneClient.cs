
namespace Haus.HassClient;

public interface IZoneClient
{
    Task UpdateAsync(string zoneId, ZoneUpdate update, CancellationToken cancellationToken = default);
    Task UpdateCoreConfigAsync(
        double? latitude = null,
        double? longitude = null,
        double? radius = null,
        CancellationToken cancellationToken = default);
}

internal sealed class ZoneClient(IHassWebSocketClient ws) : IZoneClient
{
    public Task UpdateAsync(string zoneId, ZoneUpdate update, CancellationToken cancellationToken = default) =>
        ws.UpdateZoneAsync(zoneId, update, cancellationToken);

    public Task UpdateCoreConfigAsync(
        double? latitude = null,
        double? longitude = null,
        double? radius = null,
        CancellationToken cancellationToken = default) =>
        ws.UpdateCoreConfigAsync(latitude, longitude, radius, cancellationToken);
}
