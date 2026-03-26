using System.Text.Json;

namespace Haus.Connection;

public interface IHassWebSocketClient
{
    Task<JsonElement> SendCommandAsync(object command, CancellationToken cancellationToken = default);
}
