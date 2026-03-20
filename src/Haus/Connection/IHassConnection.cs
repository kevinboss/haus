using HassClient.WS;

namespace Haus.Connection;

public interface IHassConnection
{
    ConnectionState State { get; }
    event Action<ConnectionState>? StateChanged;
    HassWSApi Client { get; }
    Task ConnectAsync(CancellationToken cancellationToken = default);
    Task DisconnectAsync();
}
